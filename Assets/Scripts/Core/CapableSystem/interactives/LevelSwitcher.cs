using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LevelSwitcher : Capable, Interactable
{

    [Header("Levels")]
    public List<string> levels_names = new List<string>();
    public int current_level = 0;

    [Header("Components")]
    public World2 world;
    private GameObject floating_dmg_provider;
    public Door elevator_door;

    [Header("Switches")]
    public int elevator_uses = 0;

    protected virtual void Start()
    {

        // we get the world
        world = GameObject.Find("/world").GetComponent<World2>();

        // we get the floating_dmg_provider
        floating_dmg_provider = GameObject.Find("/game/dmgs_provider");

        // we get the elevator_door
        elevator_door = transform.parent.Find("door_elevator").GetComponent<Door>();

        // we check if we have at least one level
        if (levels_names.Count == 0)
        {
            Debug.LogWarning("(LevelSwitcher) No levels found in the list");
            return;
        }
    }

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get { return InteractType.Other; } }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we react to the interaction
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
                if (log) { Debug.LogWarning("(LevelSwitcher) Can't switch level, the elevator door is open but can't close"); }
                StartCoroutine(floating_dmg_provider.GetComponent<TextManager>().TalkLines("why door ?\nwhy don't u want to close ?", Perso.Instance));
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

    public void OnLevelLoaded(Level2 level)
    {
        if (elevator_door == null) { Start(); }

        // we get the room connected to the elevator
        Room2 room_connected_to_elevator = level.RoomConnectedToElevator;
        elevator_door.room1 = room_connected_to_elevator;
        current_level = levels_names.IndexOf(level.name);

        // we check if the elevator has been used (if not, it is the first LevelLoading from World2, so we don't talk)
        if (elevator_uses != 0)
        {
            // we talk
            if (Perso.Instance != null)
            {
                StartCoroutine(floating_dmg_provider.GetComponent<TextManager>().TalkLines("here we go\nlevel/. " + level.name, Perso.Instance));
            }

            // we open the elevator door
            elevator_door.OnInteract(null);
        }

    }
}