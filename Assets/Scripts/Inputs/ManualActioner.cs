using UnityEngine;

public class ManualActioner : MonoBehaviour
{
    public string action_name;
    public Color color;

    private ActionSwitcher _switcher;
    private ActionSwitcher switcher
    {
        get
        {
            if (_switcher != null) { return _switcher; }
            _switcher = GetComponent<ActionSwitcher>();
            return _switcher;
        }
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(action_name)) { return; }
        switcher.SwitchAction(action_name, color);
    }
}