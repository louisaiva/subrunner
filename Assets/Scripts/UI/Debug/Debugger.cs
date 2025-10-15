using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    // [SerializeField] private TextMeshProUGUI debug_name;
    // [SerializeField] private TextMeshProUGUI debug_content;
    // [SerializeField] private TextMeshProUGUI debug_content;

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

        debug_content.text = debug_name + debuggable.GetDebugText();
    }
    
    // SET DEBUGGABLE
    public void SetDebuggable(Debuggable debuggable)
    {
        this.debuggable = debuggable;
    }
}