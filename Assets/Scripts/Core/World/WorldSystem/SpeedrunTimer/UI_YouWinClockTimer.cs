using UnityEngine;

public class UI_YouWinClockTimer : MonoBehaviour
{
    [SerializeField] private string prefix = "Demo completed in ";
    [SerializeField] private TMPro.TextMeshProUGUI clock_text;
    [SerializeField] private Color color;

    private void OnEnable()
    {
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        float time = World.Instance.Timer.Time;
        clock_text.text = prefix + time.ToString("F2").AddColor(color) + " seconds";
    }
}