using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using NavMeshPlus.Components;
using System;
using System.Threading.Tasks;
using System.Linq;

public class LevelNavBaker : MonoBehaviour
{

    [Header("Surfaces to build for each level")]
    [SerializeField] private List<NavMeshSurface> surfaces = new List<NavMeshSurface>();

    [Header("RTO Useful data")]
    private List<LevelNavData> level_nav_data = new List<LevelNavData>(); // RTO
    // private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>(); // RTO

    [Header("Logs")]
    [SerializeField] private Loggable<LevelNavBaker> log;
    [SerializeField] private Loggable<LevelNavBaker> log_baking;

    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///

    public async Task LoadLevelNavMesh(Level level)
    {
        log.Log($"Loading navmesh for level '{level.ID}'");

        // we check if we already have the navmesh data for this level
        if (try_get_out_level(level, out LevelNavData nav_data))
        {
            log.LogExtended($"Found existing navmesh data for level '{level.ID}'. Assigning it to surfaces.");
            // we have the data, we assign it to the surfaces
            assign_nav_data_to_surfaces(nav_data);
            return;
        }
        
        log.LogExtended($"No existing navmesh data found for level '{level.ID}'. Stop RoomEngine Ticking & loading AIO level for baking it.");

        // we want to build the navmesh data for this level and save it for later use

        // stop the room engine from ticking
        RoomEngine.Instance?.StopTicking();

        // we load the level in aio mode
        level = SaveEngine.AIO_Loader.LoadAIO_Level(WorldManager.StaticSelectedWorld, level.ID, navmesh_only: true);

        // wait a frame
        await Task.Yield();

        log.LogExtended($"AIO Level for level '{level.ID}' loaded, baking navmesh.");

        // we build the navmesh data for this level
        BuildLevelNavMesh(level, force_rebuild: true);

        // wait a frame
        await Task.Yield();

        log.LogExtended($"Navmesh baked for AIO Level '{level.ID}'. Clearing cache.");

        // we unload the level in aio mode
        await SaveEngine.AIO_Loader.ClearCache();

        RoomEngine.Instance?.StartTicking();
        log.Log($"Navmesh baked from AIO Level. Started RoomEngine Ticking again.");
    }

    public void BuildLevelNavMesh(Level level, bool force_rebuild = false)
    {
        log_baking.Log($"Building navmesh for level '{level.ID}'. Force rebuild: {force_rebuild}");

        // check if we already baked the nav mesh for this level
        if (!try_get_out_level(level, out LevelNavData nav_data))
        {
            // we have no saved LevelNavData, we create a new one
            nav_data = new LevelNavData(level, surfaces);
            level_nav_data.Add(nav_data);

            log_baking.LogExtended($"No existing navmesh data found for level '{level.ID}'. Added a new LevelNavData with bounds: {nav_data.bounds} (level bounds: {level.GetStaticBounds(verbose: true)})");
        }
        else if (!force_rebuild)
        {
            log_baking.LogExtended($"Found existing navmesh data for level '{level.ID}'. Assigning it to surfaces");
            assign_nav_data_to_surfaces(nav_data);
            return;
        }

        // else we build it and save it for later use
        log_baking.LogExtended($"We need to rebake navmesh for level '{level.ID}' (force_rebuild: {force_rebuild}). Building it now.");
        build_nav_mesh(nav_data);
    }
    private bool try_get_out_level(Level level, out LevelNavData nav_data)
    {
        foreach (var data in level_nav_data)
        {
            if (data.level_id == level.ID)
            {
                nav_data = data;
                return true;
            }
        }
        nav_data = null;
        return false;
    }
    private void assign_nav_data_to_surfaces(LevelNavData nav_data)
    {
        NavMeshSurface surface;
        for (int i = 0; i < surfaces.Count; i++)
        {
            surface = surfaces[i];

            // set the bounds
            surface.center = new Vector3(nav_data.bounds.center.x, 0f, nav_data.bounds.center.y);
            surface.size = new Vector3(nav_data.bounds.size.x, 1f, nav_data.bounds.size.y);

            // remove the old navmesh data if exists
            surface.RemoveData();

            // assign the saved navmesh data to the surface 
            surface.navMeshData = nav_data.navmeshdata[i];
            if (surface.isActiveAndEnabled) { surface.AddData(); }
        }
    }


    ///
    //
    /// LOW LEVEL NAV MESH BAKING
    //
    ///


    private void build_nav_mesh(LevelNavData level_nav)
    {
        log_baking.LogExtended($"Baking navmesh for level '{level_nav.level_id}' with bounds: {level_nav.bounds}");
        Bounds navMeshBounds = level_nav.bounds;
        NavMeshSurface surface;
        for (int i = 0; i < surfaces.Count; i++)
        {
            log_baking.LogSpecific($"Baking NavMeshData for surface '{surfaces[i].name}'");

            surface = surfaces[i];
            // remove the old navmesh data if exists
            log_baking.LogSpecific($"Removing old navmesh data");
            surface.RemoveData();

            // build the navmesh data for this surface
            log_baking.LogSpecific($"Baking navmesh data into dict");
            build_surface_nav_data(surface, level_nav.navmeshdata[i], navMeshBounds);

            // assign the saved navmesh data to the surface
            log_baking.LogSpecific($"Assigning baked navmesh data back to surface '{surfaces[i].name}'");
            surface.navMeshData = level_nav.navmeshdata[i];
            if (surface.isActiveAndEnabled) { surface.AddData(); }
        }
    }
    private void build_surface_nav_data(NavMeshSurface surface, NavMeshData data, Bounds navMeshBounds)
    {
        // set the bounds
        surface.center = new Vector3(navMeshBounds.center.x, 0f, navMeshBounds.center.y);
        surface.size = new Vector3(navMeshBounds.size.x, 1f, navMeshBounds.size.y);

        surface.BuildNavMesh();
        data = surface.navMeshData;
    }
}

public class LevelNavData
{
    public string level_id;
    public List<NavMeshData> navmeshdata = new List<NavMeshData>();
    public Bounds bounds;

    public LevelNavData(Level level, List<NavMeshSurface> surfaces)
    {
        this.level_id = level.ID;
        this.bounds = level.GetStaticBounds();
        foreach (var surface in surfaces)
        {
            NavMeshData nav_mesh_data = new NavMeshData(surface.agentTypeID);
            navmeshdata.Add(nav_mesh_data);
        }
    }
}