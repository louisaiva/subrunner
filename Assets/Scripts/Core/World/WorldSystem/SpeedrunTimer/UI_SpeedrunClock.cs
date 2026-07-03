using UnityEngine;

public class UI_SpeedrunClock : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI clock_text;

    private void Update()
    {
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        clock_text.enabled = GameManager.State != GameState.Cinematic;
        float time = World.Instance.Timer.Time;
        clock_text.text = time.ToString("F2");
    }
}