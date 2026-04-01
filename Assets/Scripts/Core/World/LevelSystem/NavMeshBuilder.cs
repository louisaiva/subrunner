using UnityEngine;
using System.Collections.Generic;
using NavMeshPlus.Components;
using System.Collections;
using UnityEngine.AI;

public class NavMeshBuilder : Singleton<NavMeshBuilder>
{
    [Header("NavMesh Surfaces to Build on 2nd fixed update")]
    [SerializeField] private List<NavMeshSurface> surfaces = new List<NavMeshSurface>();

    [Header("NavMesh Data saving")]
    [SerializeField] private string navmesh_data_folder = "Assets/Resources/data/navmeshes/";

    [Header("Logs")]
    [SerializeField] private bool log = false;

    public void Start()
    {
        // StartCoroutine(BuildNavMesh());
    }
    public IEnumerator BuildNavMesh()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        foreach (var surface in surfaces)
        {
            surface.BuildNavMesh();
        }
        if (log) { Debug.Log("(NavMeshBuilder) rebuilt navmeshes for " + surfaces.Count + " surfaces."); }
    }
    public void BuildNavMeshImmediate()
    {
        // try to find an enabled Level in the scene and assign the navmesh data to it
        Level[] levels = FindObjectsByType<Level>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (levels.Length != 1)
        {
            // we have not precisely 1 enabled level, we only bake the data
            foreach (var surface in surfaces) { surface.BuildNavMesh(); }
            if (log) { Debug.LogWarning("(NavMeshBuilder) found " + levels.Length + " enabled levels in the scene. Exiting. Need to be precisely 1 enabled level to assign nav mesh data."); }
            return;
        }
        Level level = levels[0];

        // else we have 1 level, we get its bounds
        Bounds bounds = level.GetStaticBounds();

        // now we build the navmesh data with the right bounds
        List<NavMeshData> navMeshDatas = new List<NavMeshData>();
        foreach (var surface in surfaces)
        {
            // set the bounds
            surface.center = new Vector3(bounds.center.x, 0f, bounds.center.y);
            surface.size = new Vector3(bounds.size.x, 1f, bounds.size.y);

            // we build the navmesh data
            surface.BuildNavMesh();
            navMeshDatas.Add(surface.navMeshData);
        }
        if (log) { Debug.Log("(NavMeshBuilder) rebuilt navmeshes for " + surfaces.Count + " surfaces."); }


        // we save the navmeshdatas as assets in the files
        for (int i = 0; i < navMeshDatas.Count; i++)
        {
            NavMeshData navMeshData = navMeshDatas[i];
            string path = navmesh_data_folder + level.GetStaticID() + "_navmesh_" + i + ".asset";
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(navMeshData, path);
            if (log) { Debug.Log("(NavMeshBuilder) saved navmesh data asset for level " + level.GetStaticID() + " at path : " + path); }

            // we add the path to the list of navmesh data paths to assign to the level
            level.AddNavMeshPath(path);
            #endif
        }
    }

    public void LoadLevelNavMeshData(List<NavMeshData> navMeshDatas)
    {
        for (int i = 0; i < navMeshDatas.Count; i++)
        {
            NavMeshData navMeshData = navMeshDatas[i];

            // get the surface
            if (i >= surfaces.Count)
            {
                if (log) { Debug.LogWarning("(NavMeshBuilder) not enough surfaces to assign navmesh data. Need " + navMeshDatas.Count + " surfaces but only have " + surfaces.Count + ". Exiting."); }
                return;
            }
            NavMeshSurface surface = surfaces[i];


            surface.RemoveData();
            surface.navMeshData = navMeshData;
            if (surface.isActiveAndEnabled)
            {
                surface.AddData();
                if (log) { Debug.Log("(NavMeshBuilder) assigned navmesh data to surface " + surface.name); }
            }
        }
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(NavMeshBuilder))]
    public class NavMeshBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            NavMeshBuilder manager = (NavMeshBuilder)target;

            if (GUILayout.Button("Bake NavMesh")) { manager.BuildNavMeshImmediate(); }
            DrawDefaultInspector();
        }
    }
#endif

}