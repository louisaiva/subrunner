using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface Awakable { void InitAwake(); }
public interface Startable { void InitStart(); }


/// <summary>
/// this class is for calling the InitAwake() method on some specific Awakable
/// that needs to be called on the awake (like few singletons, etc)
/// useful for awaking monobehaviours that needs to be disabled in the inspector (ui)
/// </summary>
public class Awaker : MonoBehaviour
{
    [Header("Awakables")]
    public List<GameObject> objectsToAwake;
    public bool awake_all_awakables_in_scene = true;
    private List<Awakable> all_awakables = new List<Awakable>();

    [Header("Startables")]
    public List<GameObject> objectsToStart;
    public bool start_all_startables_in_scene = true;
    private List<Startable> all_startables = new List<Startable>();

    // AWAKE - CALL INITAWAKE
    private void Awake()
    {
        FindAllAwakablesAndStartablesInScene();

        // 1 - we prepare the lists of components to awake
        List<Awakable> awakables = new List<Awakable>();

        // 2 - we add objects To Awake to this list
        if (awake_all_awakables_in_scene) { awakables.AddRange(all_awakables); }
        else
        {

            // else we add chosen awakbles only
            for (int i = 0; i < objectsToAwake.Count; i++)
            {
                GameObject obj = objectsToAwake[i];
                if (obj == null) { continue; }
                Awakable awakable = obj.GetComponent<Awakable>();
                if (awakable == null) { continue; }
                awakables.Add(awakable);
            }

            // + we add other components we want to add
            UI_SlottableMixer[] mixers = FindObjectsByType<UI_SlottableMixer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            awakables.AddRange(mixers);
        }

        // 3 - we init awake all those awakable
        for (int i = 0; i < awakables.Count; i++)
        {
            Awakable awakable = awakables[i];
            if (awakable == null) { continue; }
            awakable.InitAwake();
        }
    }

    // START - CALL INITSTART
    private void Start()
    {
        // same thing pretty much
        List<Startable> startables = new List<Startable>();
        if (start_all_startables_in_scene) { startables.AddRange(all_startables); }
        else
        {
            for (int i = 0; i < objectsToStart.Count; i++)
            {
                GameObject obj = objectsToStart[i];
                if (obj == null) { continue; }
                Startable startable = obj.GetComponent<Startable>();
                if (startable == null) { continue; }
                startables.Add(startable);
            }
        }

        for (int i = 0; i < startables.Count; i++)
        {
            Startable startable = startables[i];
            if (startable == null) { continue; }
            startable.InitStart();
        }
    }


    // helpers methods
    private void FindAllAwakablesAndStartablesInScene()
    {
        // Source - https://stackoverflow.com/a/65495834
        // Posted by derHugo, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-02-17, License - CC BY-SA 4.0

        MonoBehaviour[] mono_behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        all_startables = mono_behaviours.OfType<Startable>().ToList();
        all_awakables = mono_behaviours.OfType<Awakable>().ToList();
    }

}
