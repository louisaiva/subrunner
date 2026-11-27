using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Cmd : MonoBehaviour
{
    [Header("Static lines"), Tooltip("These are the bones of the cmd, they will be constructed one per one in Enable()")]
    public List<UI_CmdLine> lines_prefabs = new List<UI_CmdLine>();
    
    [Header("Current lines"), Tooltip("Current lines")]
    public List<UI_CmdLine> lines = new List<UI_CmdLine>();
    private List<UI_CmdLine> prefabs_pile = new List<UI_CmdLine>();

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // ENABLE - DISABLE
    public void Enable()
    {
        if (log) { Debug.Log($"(UI_CMD) Enabling cmd"); }
        if (writing_coroutine != null) { return; }

        prefabs_pile.Clear();
        prefabs_pile = new List<UI_CmdLine>(lines_prefabs);
        writing_coroutine = StartCoroutine(WriteLines());
    }
    public void Disable()
    {
        if (log) { Debug.Log($"(UI_CMD) Disabling cmd"); }
        foreach (UI_CmdLine line in lines)
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
        writing_coroutine = null;
    }


    // writing coroutine
    private Coroutine writing_coroutine = null;
    private IEnumerator WriteLines()
    {
        while (prefabs_pile.Count > 0)
        {
            UI_CmdLine line = lines.LastOrDefault();

            if (is_line_still_running(line))
            {
                yield return new WaitForSecondsRealtime(0.1f);
                continue;
            }
            
            // we create a new line
            line = Instantiate(prefabs_pile[0], transform);
            lines.Add(line);
            prefabs_pile.RemoveAt(0);
        }
        writing_coroutine = null;
    }
    private bool is_line_still_running(UI_CmdLine last_line)
    {
        if (last_line == null) { return false; }
        if (last_line.IsDone) { return false; }
        return true;
    }

}