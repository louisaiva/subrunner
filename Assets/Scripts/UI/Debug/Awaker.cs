using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this class is for calling the InitAwake() method on some specific Awakable
/// that needs to be called on the awake (like few singletons, etc)
/// useful for awaking monobehaviours that needs to be disabled in the inspector (ui)
/// </summary>
public class Awaker : MonoBehaviour
{
    public List<GameObject> objectsToAwake;
    private void Awake()
    {
        foreach (GameObject obj in objectsToAwake)
        {
            if (obj == null) { continue; }
            Awakable awakable = obj.GetComponent<Awakable>();
            if (awakable == null) { continue; }
            awakable.InitAwake();
        }
    }
}

public interface Awakable
{
    void InitAwake();
}