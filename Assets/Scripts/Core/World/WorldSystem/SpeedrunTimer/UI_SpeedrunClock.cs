using UnityEngine;

public class UI_SpeedrunClock : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI clock_text;

    private void Update()
    {
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        clock_text.enabled = GameManager.State != GameState.Cinematic;
        clock_text.text = World.Instance.Timer.GetColoredHMS_Time(Color.white, Color.grey);
    }

}