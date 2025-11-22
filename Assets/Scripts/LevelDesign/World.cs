using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : Singleton<World>
{
    [Header("World")]
    [SerializeField] private Room elevator_room;
    public Level current_level;
    public List<Level> loaded_levels = new List<Level>();

    [Header("Spawn")]
    public Transform spawn_point;

    [Header("Levels")]
    public List<Level> Levels;
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

    // START
    private void Start()
    {
        // on charge l'elevator room
        load_elevator_room();

        // on vérifie que nos levels sont bien
        Levels.RemoveAll(l => l == null);
        if (debug) { Debug.Log($"(World) {Levels.Count} Levels: " + string.Join(", ", Levels.Select(l => l.name))); }

        // on charge le 1er level si on en a un
        if (Levels.Count > 0) { load_level(Levels[0]); }

        // on tp le perso au spawn si on en a un
        if (spawn_point == null)
        {
            if (debug) { Debug.LogWarning("(World) No spawn point found in world, please set it manually in the inspector"); }
        }
        else
        {
            Controller.Instance.Capable.transform.position = spawn_point.position;
        }
    }


    // UPDATE
    private void Update()
    {
        if (!Controller.Instance) { return; }

        Controller.Instance.current_room = findPersoRoom(LoadedRooms);

        if (Controller.Instance.current_room == null) { return; }

        if (!Controller.Instance.current_room.Alight) { Controller.Instance.current_room.Show(); }
    }
    private Room findPersoRoom(List<Room> rooms)
    {
        // définit la room du perso en faisant un raycast
        // get the position of the perso
        Vector2 perso_position = Controller.Instance.Capable.transform.position;

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
    private void load_elevator_room()
    {
        // on récupère l'elevator room
        if (elevator_room == null)
        {
            elevator_room = transform.Find("Room_Elevator")?.GetComponent<Room>();
            if (elevator_room == null && debug) { Debug.LogWarning("(World) No elevator room found in world, please set it manually in the inspector"); }
            
            // si on a pas d'elevator room alors pas besoin de la charger
            return;
        }

        // on awake l'elevator room
        if (!elevator_room.loaded) { elevator_room.Awake(); }
    }

}