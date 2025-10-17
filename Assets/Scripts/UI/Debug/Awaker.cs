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
        for (int i = 0; i < objectsToAwake.Count; i++)
        {
            GameObject obj = objectsToAwake[i];
            if (obj == null) { continue; }
            Awakable awakable = obj.GetComponent<Awakable>();
            if (awakable == null) { continue; }
            awakable.InitAwake();
        }
    }

    private void Start()
    {
        for (int i = 0; i < objectsToAwake.Count; i++)
        {
            GameObject obj = objectsToAwake[i];
            if (obj == null) { continue; }
            Startable startable = obj.GetComponent<Startable>();
            if (startable == null) { continue; }
            startable.InitStart();
        }
    }
}

public interface Awakable
{
    void InitAwake();
}

public interface Startable
{
    void InitStart();
}
