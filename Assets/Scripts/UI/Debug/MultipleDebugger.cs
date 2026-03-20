using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultipleDebugger : MonoBehaviour, Debugger
{
    [SerializeField] private GameObject debug_prefab;
    private TextMeshProUGUI debug_title;
    private MultipleDebuggable debuggable;
    private string debug_name;

    // instanciated debug lines
    private List<TextMeshProUGUI> debug_lines = new List<TextMeshProUGUI>();

    // START
    private void Awake()
    {
        debug_title = GetComponent<TextMeshProUGUI>();
        debug_name = gameObject.name.ToUpper();
        debug_name = string.Join("-", debug_name.ToCharArray()) + "\n";
    }

    // SET DEBUGGABLE
    public void SetDebuggable(Debuggable debuggable)
    {
        if (debuggable is not MultipleDebuggable mdebug) { return; }
        this.debuggable = mdebug;
    }

    // UPDATE
    private readonly List<string> lines = new List<string>();
    void Update()
    {
        // if no debuggable, we remove all debug lines except 1, say no info in this one and return
        if (debuggable == null)
        {
            remove_debug_lines(debug_lines.Count);
            add_debug_lines(1);
            debug_lines[0].text = "/!\\ no info ;-; /!\\";
            return;
        }

        // else we get the debug lines from the debuggable
        lines.Clear();
        try
        {
            lines.AddRange(debuggable.GetDebugLines());
        }
        catch
        {
            remove_debug_lines(debug_lines.Count);
            add_debug_lines(1);
            debug_lines[0].text = "/!\\ error getting info ;-; /!\\";
            return;
        }


        // else we have the lines !!

        // we match the number of lines we have with the number of lines we need
        if (lines.Count != debug_lines.Count)
        {
            if (lines.Count > debug_lines.Count) { add_debug_lines(lines.Count - debug_lines.Count); }
            else if (lines.Count < debug_lines.Count) { remove_debug_lines(debug_lines.Count - lines.Count); }
        }

        // then we update the text of each line
        for (int i = 0; i < lines.Count; i++)
        {
            debug_lines[i].text = lines[i];
        }
    }
   

    // ADD / REMOVE DEBUG LINES
    private void add_debug_lines(int count=1)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject debug_line = Instantiate(debug_prefab, transform);
            TextMeshProUGUI debug_line_text = debug_line.GetComponent<TextMeshProUGUI>();
            debug_lines.Add(debug_line_text);
        }
    }
    private void remove_debug_lines(int count=1)
    {
        for (int i = 0; i < count; i++)
        {
            if (debug_lines.Count == 0) { return; }
            TextMeshProUGUI debug_line = debug_lines[0];
            debug_lines.RemoveAt(0);
            Destroy(debug_line.gameObject);
        }
    }
}