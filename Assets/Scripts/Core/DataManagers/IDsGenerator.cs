using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class IDsGenerator : Singleton<IDsGenerator>
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

        foreach (Capable capable in all_capables)
        {
            generate_id_for_capable(capable, all_rooms);
        }
    }


    public void GenerateIDsOnlyForCapablesAndCapacities(List<Capable> old_capables, List<Capable> new_capables)
    {
        // we clear the list since we want to re generate ids
        capables_that_get_new_ids.Clear();
        capacities_that_get_new_ids.Clear();
        // World.LazyInstance.ClearGeneratedIDs(); // ! WE DON'T CLEAR ALL GENERATED IDS BCZ WE WANT TO KEEP THE IDS OF MOST CAPABLES

        // we un-interesct the old and new capables
        List<Capable> capables_to_remove = old_capables.Where(c => !new_capables.Contains(c)).ToList();

        // we gather the ids of the going-to-be-removed capables
        Dictionary<string, List<int>> free_ids_by_prefix = new Dictionary<string, List<int>>();
        foreach (Capable capable in capables_to_remove)
        {
            // these capables are going to be destroyed so we can remember their ids as free ids for the next capables that will be created
            if (capable.data == null || string.IsNullOrEmpty(capable.data.id)) { continue; }
            add_id_to_free_ids(capable.data.id, ref free_ids_by_prefix);

            // ? should we also free inventory capables & capacities ?

            // we also do the same for their capacities
            for (int i = 0; i < capable.data.capacities_ids.Count; i++)
            {
                string capacity_id = capable.data.capacities_ids[i];
                if (string.IsNullOrEmpty(capacity_id)) { continue; }
                add_id_to_free_ids(capacity_id, ref free_ids_by_prefix);
            }
        }

        // we sort these free ids by prefix and number (highest number first), so we can unregister these ids from the world ids.
        foreach (string prefix in free_ids_by_prefix.Keys.ToList())
        {
            if (log) { Debug.Log($"(IDsGenerator) Freeing ids for prefix {prefix}..."); }
            free_ids_by_prefix[prefix] = free_ids_by_prefix[prefix].OrderByDescending(id_nb => id_nb).ToList();
            if (log) { Debug.Log($"(IDsGenerator) freeing ids for prefix {prefix} : {string.Join(", ", free_ids_by_prefix[prefix])}"); }

            foreach (int id_nb in free_ids_by_prefix[prefix])
            {
                string full_id = $"{prefix}-{id_nb}";
                World.LazyInstance.UnregisterUniqueID(full_id);
                if (log) { Debug.Log($"(IDsGenerator) Unregistered id {full_id} from world generated ids"); }
            }
        }

        // ok now we should have free all the removed capables' ids.
        // we can now generate new ids for the new capables
        if (log) { Debug.Log($"(IDsGenerator) Generating new ids for BOUYA new capables..."); }
        if (log) { Debug.Log($"(IDsGenerator) Generating new ids for {new_capables.Count} new capables..."); }

        // then we generate new ids for the new capables and their capacities, we try to reuse the free ids if possible (if the name prefix is the same)
        foreach (Capable capable in new_capables)
        {
            string new_id = generate_id_for_capable_no_room_update(capable);
            if (log) { Debug.Log($"(IDsGenerator) Generated new id for capable {capable.name} : {new_id}"); }
        }
    }
    private bool add_id_to_free_ids(string id, ref Dictionary<string, List<int>> free_ids_by_prefix)
    {
        if (string.IsNullOrEmpty(id)) { return false; }
        string prefix = id.Split('-')[0]; // we consider the prefix as the part before the first '-'
        int id_nb;
        try
        {
            id_nb = int.Parse(id.Split('-')[1]); // we consider the number as the part after the first '-'
        }
        catch (Exception)
        {
            Debug.LogWarning($"(IDsGenerator) Capable/Capacity {id} has an id that doesn't follow the prefix-number format, we can't reuse its id");
            return false;
        }
        if (!free_ids_by_prefix.ContainsKey(prefix))
        {
            free_ids_by_prefix[prefix] = new List<int>();
        }
        free_ids_by_prefix[prefix].Add(id_nb);
        if (log) { Debug.Log($"(IDsGenerator) Memorized id {prefix}-{id_nb} for freeing next"); }
        return true;
    }


    // id generation
    private string generate_id_for_capable_no_room_update(Capable capable)
    {
        // we check if we already generated an id for this capable
        if (capables_that_get_new_ids.Contains(capable)) { return ""; }

        // generate new id
        string new_id = World.LazyInstance.GenerateUniqueID(capable.ID);

        // we generate unique IDs for all capables in the inventory of this capable
        List<Item> inventory_capables = capable.Inventory?.GetStaticItems() ?? new List<Item>();
        foreach (Item inventory_capable in inventory_capables)
        {
            string old_item_id = inventory_capable.data.id;
            string new_item_id = generate_id_for_capable_no_room_update(inventory_capable);

            // we check if we have something
            if (string.IsNullOrEmpty(new_item_id) || string.IsNullOrEmpty(old_item_id)) { continue; }

            // then we change the item_id in this capable inventory
            update_item_id_in_inventory(capable, old_item_id, new_item_id);
        }

        // we generate unique IDs for all capacities of this capable
        generate_ids_for_capacities(capable, new_owner_id: new_id);

        // add capable to generated id list
        capables_that_get_new_ids.Add(capable);

        // finally change capable's id
        capable.data.id = new_id;
        if (log) { Debug.Log($"(IDsGenerator - Capable) {capable.name} has now a new ID : {new_id}"); }

        return new_id;
    }
    private string generate_id_for_capable(Capable capable, List<Room> all_rooms)
    {
        // we check if we already generated an id for this capable
        if (capables_that_get_new_ids.Contains(capable)) { return ""; }

        // generate new id
        string new_id = World.LazyInstance.GenerateUniqueID(capable.ID);

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
            string new_item_id = generate_id_for_capable_no_room_update(inventory_capable); // we don't want to switch rooms for inventory capables because they are not in rooms

            // we check if we have something
            if (string.IsNullOrEmpty(new_item_id) || string.IsNullOrEmpty(old_item_id)) { continue; }

            // then we change the item_id in this capable inventory
            update_item_id_in_inventory(capable, old_item_id, new_item_id);
        }

        // we generate unique IDs for all capacities of this capable
        generate_ids_for_capacities(capable, new_owner_id: new_id);

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