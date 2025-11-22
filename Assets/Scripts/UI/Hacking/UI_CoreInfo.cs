using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CoreInfo : MonoBehaviour
{
    [Header("UI_CoreInfo")]
    [SerializeField] private Image image;
    public Core core;

    [Header("Colors")]
    [SerializeField] private Color base_color = Color.white;
    [SerializeField] private Color used_color = Color.red;
    [SerializeField] private Color waiting_color = Color.blue;

    // START
    public void Init(Core core)
    {
        this.core = core;
        image.color = base_color;
        Update();
    }

    // UPDATE
    void Update()
    {
        if (core == null) { return; }
        if (core.isFree)
        {
            image.color = base_color;
        }
        else if (core.RunningProcess != null && core.RunningProcess.state == ProcessusState.Waiting)
        {
            image.color = waiting_color;
        }
        else
        {
            image.color = used_color;
        }
    }
}