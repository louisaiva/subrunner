using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("World")]
    [SerializeField] private Room elevator_room;
    public Level current_level;
    public List<Level> loaded_levels = new List<Level>();

    [Header("Levels")]
    public string levels_prefab_path = "prefabs/environments/Levels/";

    [Header("Rooms")]
    public List<Room> LoadedRooms
    {
        get
        {
            List<Room> rooms = new List<Room>() { };
            if (elevator_room != null) { rooms.Add(elevator_room); }
            if (current_level != null) { rooms.AddRange(current_level.rooms); }
            return rooms;
        }
    }

    [Header("Logs")]
    public bool debug = false;
    public bool debug_find_perso_room = false;

    private void Start()
    {
        // on récupère le level actif
        foreach (Transform child in transform)
        {
            Level level = child.GetComponent<Level>();
            if (level != null && level.gameObject.activeSelf)
            {
                current_level = level;
                break;
            }
        }

        // on initialise le level
        if (current_level != null)
        {
            load_level(current_level);

            // on regarde si on a déjà une room pour le perso 
            Room perso_room = findPersoRoom(LoadedRooms);
            if (perso_room != null) { return;}

            // sinon on tp le perso à l'elevator
        }

        // on récupère l'elevator room
        elevator_room = transform.Find("Room_Elevator")?.GetComponent<Room>();
        if (elevator_room == null)
        {
            if (debug) { Debug.LogWarning("(World) Elevator room not found !"); }

            // si on a pas d'elevator room alors on ne tp pas le perso
            return;
        }

        // on awake l'elevator room
        if (!elevator_room.loaded) { elevator_room.Awake(); }

        // on envoie le perso au milieu de l'elevator room
        Perso.Instance.transform.position = elevator_room.transform.Find("objects/elevator").transform.position - new Vector3(0,0.5f,0);
        if (debug) { Debug.LogWarning("(World) No Perso Room found !! teleporting perso to elevator");}
    }

    private void Update()
    {
        if (!Perso.Instance) { return; }

        Perso.Instance.current_room = findPersoRoom(LoadedRooms);

        if (Perso.Instance.current_room == null) { return; }

        if (!Perso.Instance.current_room.Alight) { Perso.Instance.current_room.Show(); }
    }

    private Room findPersoRoom(List<Room> rooms)
    {
        // définit la room du perso en faisant un raycast
        // get the position of the perso
        Vector2 perso_position = Perso.Instance.transform.position;

        string debug_message = "(World - findPersoRoom) Loaded rooms :\n\t";

        // we check if the perso is in a room
        foreach (Room room in rooms)
        {
            debug_message += room.name;
            if (room.RoomCollider.OverlapPoint(perso_position))
            {
                debug_message += " perso is here !!!!!" + "\n\t";
                if (debug_find_perso_room) { Debug.Log(debug_message); }
                return room;
            }
            debug_message += " no perso " + "\n\t";
        }

        return null;
    }

    // Level Loading/Unloading
    public void LoadLevelFromName(string level_name)
    {
        // we unload the current level
        if (current_level != null)
        {
            current_level.Hide();
            
            // we desactivate the current level
            current_level.gameObject.SetActive(false);
        }

        // we check if the level is already loaded
        List<Level> current_levels = loaded_levels.FindAll(l => l.name == level_name);
        if (current_levels.Count > 0)
        {
            // we activate the level
            current_levels[0].gameObject.SetActive(true);
            load_level(current_levels[0]);
            return;
        }

        // we try to find the level in the prefabs path
        Level level = Resources.Load<Level>(levels_prefab_path + level_name);
        if (level == null)
        {
            if (debug) { Debug.LogError("(World - LoadLevelFromName) Level not found : " + level_name); }
            return;
        }

        // we load the new level
        GameObject level_go = Instantiate(level.gameObject, transform);
        level_go.name = level_name;
        load_level(level_go.GetComponent<Level>());
    }
    private void load_level(Level level)
    {
        // we initialize the level
        if (!level.loaded) { level.Start(); }

        // we hide all rooms
        level.Hide();

        // we set the current level
        current_level = level;

        // we add the level to the loaded levels
        if (!loaded_levels.Contains(level)) { loaded_levels.Add(level); }

        // we play OnLevelLoaded event on the LevelSwitcher
        if (elevator_room)
        {
            elevator_room.transform.Find("objects/elevator").GetComponent<LevelSwitcher>().OnLevelLoaded(current_level);
        }

        if (debug) { Debug.Log("(World) Level loaded : " + level.name); }
    }

}