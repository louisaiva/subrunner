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
        clock_text.text = prefix + World.Instance.Timer.GetColoredHMS_Time(color, Color.white);
    }
}