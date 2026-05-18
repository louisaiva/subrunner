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
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_baking = false;
    [SerializeField] private bool log_sources = false;

    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///

    public async Task LoadLevelNavMesh(Level level)
    {
        if (log) { Debug.Log($"(LevelNavBaker) Loading navmesh for level '{level.ID}'"); }

        // we check if we already have the navmesh data for this level
        if (try_get_out_level(level, out LevelNavData nav_data))
        {
            if (log) { Debug.Log($"(LevelNavBaker) Found existing navmesh data for level '{level.ID}'. Assigning it to surfaces."); }
            // we have the data, we assign it to the surfaces
            assign_nav_data_to_surfaces(nav_data);
            return;
        }
        
        if (log) { Debug.Log($"(LevelNavBaker) No existing navmesh data found for level '{level.ID}'. Loading AIO level to bake it."); }

        // we want to build the navmesh data for this level and save it for later use

        // stop the room engine from ticking
        RoomEngine.Instance?.StopTicking();

        // we load the level in aio mode
        level = SaveEngine.AIO_Loader.LoadAIO_Level(WorldManager.StaticSelectedWorld, level.ID, navmesh_only: true);

        // wait a frame
        await Task.Yield();

        if (log) { Debug.Log($"(LevelNavBaker) AIO Level for level '{level.ID}' loaded, baking navmesh."); }

        // we build the navmesh data for this level
        BuildLevelNavMesh(level, force_rebuild: true);

        // wait a frame
        await Task.Yield();

        if (log) { Debug.Log($"(LevelNavBaker) Navmesh baked for AIO Level '{level.ID}'. Clearing cache."); }

        // we unload the level in aio mode
        await SaveEngine.AIO_Loader.ClearCache();

        RoomEngine.Instance?.StartTicking();
    }

    public void BuildLevelNavMesh(Level level, bool force_rebuild = false)
    {
        // check if we already baked the nav mesh for this level
        if (!try_get_out_level(level, out LevelNavData nav_data))
        {
            // we have no saved LevelNavData, we create a new one
            nav_data = new LevelNavData(level, surfaces);
            level_nav_data.Add(nav_data);

            if (log_baking)
            {
                Debug.Log($"(LevelNavBaker) No existing navmesh data found for level '{level.ID}'. Added a new LevelNavData with bounds: {nav_data.bounds} (level bounds: {level.GetStaticBounds(verbose: true)})");
            }
        }
        else if (!force_rebuild)
        {
            if (log_baking)
            {
                Debug.Log($"(LevelNavBaker) Found existing navmesh data for level '{level.ID}'. Assigning it to surfaces.");
            }
            assign_nav_data_to_surfaces(nav_data);
            return;
        }

        // else we build it and save it for later use
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
        if (log_baking)
        {
            Debug.Log($"(LevelNavBaker) Baking navmesh for level '{level_nav.level_id}', bounds: {level_nav.bounds}");
        }
        Bounds navMeshBounds = level_nav.bounds;
        NavMeshSurface surface;
        for (int i = 0; i < surfaces.Count; i++)
        {
            if (log_sources) { Debug.Log($"(LevelNavBaker) Baking sources for surface '{surfaces[i].name}'"); }

            surface = surfaces[i];
            // remove the old navmesh data if exists
            surface.RemoveData();

            // build the navmesh data for this surface
            build_surface_nav_data(surface, level_nav.navmeshdata[i], navMeshBounds);

            // assign the saved navmesh data to the surface 
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