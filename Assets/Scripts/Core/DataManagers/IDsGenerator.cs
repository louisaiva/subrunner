using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class IDsGenerator : MonoBehaviour
{

    [Header("Debug Lists")]
    [SerializeField] private List<Capable> capables_that_get_new_ids = new List<Capable>();
    [SerializeField] private List<Capacity> capacities_that_get_new_ids = new List<Capacity>();

    [Header("Components")]
    private GameManager _game_manager;
    private GameManager GameManager
    {
        get
        {
            if (_game_manager == null) { _game_manager = GameManager.Instance; }
            if (_game_manager == null) { _game_manager = FindFirstObjectByType<GameManager>(); } // because we need static finding for IDsGenerator
            return _game_manager;
        }
    }

    [Header("Logs")]
    public bool log = false;


    // MAIN ID GENERATOR METHOD
    public void GenerateIDsForAllCapablesAndCapacities()
    {
        // we clear the list since we want to re generate ids
        capables_that_get_new_ids.Clear();
        capacities_that_get_new_ids.Clear();
        World.LazyInstance.ClearGeneratedIDs();


        // we get all the rooms
        Room[] all_rooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Capable[] all_capables = FindObjectsByType<Capable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Capable capable in all_capables)
        {
            generate_id_for_capable(capable, all_rooms.ToList());
        }
    }
    public void GenerateIDsForAllCapablesAndCapacitiesInWorld()
    {
        // we clear the list since we want to re generate ids
        capables_that_get_new_ids.Clear();
        capacities_that_get_new_ids.Clear();
        World.LazyInstance.ClearGeneratedIDs();

        // get the world
        World world = World.LazyInstance;

        // get the levels of the world
        Level[] levels = world.GetStaticLevels();

        // we get all the rooms
        List<Room> all_rooms = new List<Room>();
        List<Capable> all_capables = new List<Capable>();
        foreach (Level level in levels)
        {
            all_rooms.AddRange(level.GetStaticRooms());
            all_capables.AddRange(level.GetStaticCapables());
        }

        // Capable[] all_capables = FindObjectsByType<Capable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Capable capable in all_capables)
        {
            generate_id_for_capable(capable, all_rooms);
        }
    }

    // id generation
    private string generate_id_for_capable(Capable capable, List<Room> all_rooms)
    {
        // we check if we already generated an id for this capable
        if (capables_that_get_new_ids.Contains(capable)) { return ""; }

        // generate new id
        string new_id = World.LazyInstance.GenerateUniqueID(capable.name);

        // check if a room has our old id then we change it to new id
        // (we must have an old id for this to work)
        if (capable.data != null && !string.IsNullOrEmpty(capable.data.id))
        {
            Room room = get_room_of_capable(capable, all_rooms);
            if (room != null)
            {
                int index = room.data.capables_ids.IndexOf(capable.data.id);
                room.data.capables_ids[index] = new_id;
                if (log) { Debug.Log($"(IDsGenerator - Room) Room assignement {room.name} updated capable {capable.data.id} to {new_id}"); }
            }
        }

        // we generate unique IDs for all capables in the inventory of this capable
        List<Item> inventory_capables = capable.Inventory?.GetStaticItems() ?? new List<Item>();
        foreach (Item inventory_capable in inventory_capables)
        {
            string old_item_id = inventory_capable.data.id;
            string new_item_id = generate_id_for_capable(inventory_capable, all_rooms);
            
            // we check if we have something
            if (string.IsNullOrEmpty(new_item_id) || string.IsNullOrEmpty(old_item_id)) { continue; }

            // then we change the item_id in this capable inventory
            update_item_id_in_inventory(capable, old_item_id, new_item_id);
        }

        // we generate unique IDs for all capacities of this capable
        generate_ids_for_capacities(capable, new_owner_id : new_id);

        // add capable to generated id list
        capables_that_get_new_ids.Add(capable);

        // finally change capable's id
        capable.data.id = new_id;
        if (log) { Debug.Log($"(IDsGenerator - Capable) {capable.name} has now a new ID : {new_id}"); }

        return new_id;
    }
    private void generate_ids_for_capacities(Capable capable, string new_owner_id = "")
    {
        // we get all capacities in the DIRECT children of this capable
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capable.transform.childCount; i++)
        {
            capacities.AddRange(capable.transform.GetChild(i).GetComponents<Capacity>());
        }

        if (capacities.Count == 0) { return; }

        foreach (Capacity capacity in capacities)
        {
            // we check if we already generated an id for this capacity
            if (capacities_that_get_new_ids.Contains(capacity)) { continue; }

            // generate new id
            string new_id = World.LazyInstance.GenerateUniqueID(capacity.name);

            // change the capacity id in the capable's capacities ids list
            if (!string.IsNullOrEmpty(capacity.data.id) && !string.IsNullOrEmpty(new_id))
            {
                for (int i=0; i<capable.data.capacities_ids.Count; i++)
                {
                    if (capable.data.capacities_ids[i] != capacity.data.id) { continue; }
                    capable.data.capacities_ids[i] = new_id;
                }
            }

            // change the owner id of the capacity to the new capable id
            capacity.data.owner_id = new_owner_id;

            // change capacity's id
            capacity.data.id = new_id;
            if (log) { Debug.Log($"(IDsGenerator - Capacity) {capacity.name} has now a new ID : {new_id}"); }

            // add capacity to generated id list
            capacities_that_get_new_ids.Add(capacity);
        }
    }

    // utils
    private Room get_room_of_capable(Capable capable, List<Room> all_rooms)
    {
        foreach (Room room in all_rooms)
        {
            if (room.data.capables_ids.Contains(capable.data.id))
            {
                return room;
            }
        }
        return null; // if we don't find any room, we return null
    }
    private void update_item_id_in_inventory(Capable capable, string old_item_id, string new_item_id)
    {
        if (capable.data == null || capable.data.inventory == null) { return; }
        foreach (ItemPoolData pool_data in capable.data.inventory.item_pools_data)
        {
            foreach (ItemStackData stack_data in pool_data.stacks_data)
            {
                for (int i = 0; i < stack_data.items_ids.Count; i++)
                {
                    if (stack_data.items_ids[i] == old_item_id)
                    {
                        stack_data.items_ids[i] = new_item_id;
                        if (log) { Debug.Log($"(IDsGenerator - Inventory) Updated item id in inventory of {capable.name} from {old_item_id} to {new_item_id}"); }
                        return; // an item can only be in ONE stack
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(IDsGenerator))]
    public class IDsGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            IDsGenerator manager = (IDsGenerator)target;

            if (GUILayout.Button("Generate IDs in World"))
            {
                manager.GenerateIDsForAllCapablesAndCapacitiesInWorld();

                // check if app is playing we return
                if (Application.isPlaying) { return; }
                
                // then we need to mark all capables & capacities as "dirty" so their data will be saved with the new ids
                foreach (Capable capable in manager.capables_that_get_new_ids)
                {
                    UnityEditor.EditorUtility.SetDirty(capable);
                }
                foreach (Capacity capacity in manager.capacities_that_get_new_ids)
                {
                    UnityEditor.EditorUtility.SetDirty(capacity);
                }

                // we also make sure GameManager is marked as dirty because it has all generated ids
                UnityEditor.EditorUtility.SetDirty(World.LazyInstance);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            }
            DrawDefaultInspector();
        }
    }
#endif
}