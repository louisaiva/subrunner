using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LevelSwitcher : Capable, Interactable
{

    // inputs actions
    /*public PlayerInputActions input_actions
    {
        get => GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
    }*/

    [Header("Levels")]
    public List<string> levels_names = new List<string>();
    public int current_level = 0;

    [Header("Components")]
    public World world;
    private GameObject floating_dmg_provider;
    public Perso perso;
    public Door elevator_door;

    [Header("Switches")]
    public int elevator_uses = 0;

    protected virtual void Start()
    {
        // we get the world
        world = GameObject.Find("/world").GetComponent<World>();

        // we get the floating_dmg_provider
        floating_dmg_provider = GameObject.Find("/utils/dmgs_provider");

        // we get the perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // we get the elevator_door
        elevator_door = transform.parent.Find("door_elevator").GetComponent<Door>();

        // we check if we have at least one level
        if (levels_names.Count == 0)
        {
            Debug.LogWarning("(LevelSwitcher) No levels found in the list");
            return;
        }
    }

    public void OnInteract()
    {
        // if (!input_actions.perso.enabled) { return; }
        elevator_uses++;
        StartCoroutine(switchLevel());
    }

    private IEnumerator switchLevel()
    {
        // we check if the elevator door is open
        if (elevator_door.is_open)
        {
            if (elevator_door.Can("close"))
            {
                elevator_door.close();
                yield return new WaitUntil(() => !elevator_door.is_open && !elevator_door.is_moving);
            }
            else
            {
                if (debug) { Debug.LogWarning("(LevelSwitcher) Can't switch level, the elevator door is open but can't close"); }
                floating_dmg_provider.GetComponent<TextManager>().talk("why door ?\nwhy don't u want to close ?",perso);
                yield break;
            }
        }

        // we increment the current level
        int next_level = current_level + 1;
        if (next_level >= levels_names.Count) { next_level = 0; }

        // if (debug) { Debug.Log("(LevelSwitcher) switch level from " + levels_names[current_level] + "("+ current_level +") to " + levels_names[next_level] + "("+ next_level +")"); }

        // we switch the level
        world.LoadLevelFromName(levels_names[next_level]);
    }

    public void OnLevelLoaded(Level level)
    {
        if (elevator_door == null) { Start(); }

        // we get the room connected to the elevator
        Room room_connected_to_elevator = level.RoomConnectedToElevator;
        elevator_door.room1 = room_connected_to_elevator;
        current_level = levels_names.IndexOf(level.name);

        // we check if the elevator has been used (if not, it is the first LevelLoading from World, so we don't talk)
        if (elevator_uses != 0)
        {
            // we talk
            floating_dmg_provider.GetComponent<TextManager>().talk("here we go\nlevel/. " + level.name,perso);

            // we open the elevator door
            elevator_door.OnInteract();
        }

    }
}