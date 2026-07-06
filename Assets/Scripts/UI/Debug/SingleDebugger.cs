using TMPro;
using UnityEngine;

public class SingleDebugger : MonoBehaviour, Debugger
{
    private TextMeshProUGUI debug_content;
    private Debuggable debuggable;
    private string debug_name;

    // START
    private void Awake()
    {
        debug_content = GetComponent<TextMeshProUGUI>();
        debug_name = gameObject.name.ToUpper();
        debug_name = string.Join("-", debug_name.ToCharArray()) + "\n";
    }

    // UPDATE
    void Update()
    {
        if (debuggable == null) { debug_content.text = "/!\\ no info ;-; /!\\"; return; }
        try
        {
            debug_content.text = debug_name + debuggable.GetDebugText();
        }
        catch
        {
            debuggable = null;
            debug_content.text = "/!\\ error getting info ;-; /!\\";
        }
    }

    // SET DEBUGGABLE
    public void SetDebuggable(Debuggable debuggable)
    {
        this.debuggable = debuggable;
    }
}