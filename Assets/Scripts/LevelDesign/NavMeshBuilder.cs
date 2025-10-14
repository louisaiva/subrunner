using UnityEngine;
using System.Collections.Generic;
using NavMeshPlus.Components;
using System.Collections;

public class NavMeshBuilder : MonoBehaviour
{
    [Header("NavMesh Surfaces to Build on 2nd fixed update")]
    [SerializeField] private List<NavMeshSurface> surfaces = new List<NavMeshSurface>();

    [Header("Logs")]
    [SerializeField] private bool log = false;

    public void Start()
    {
        StartCoroutine(BuildNavMesh());
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
}