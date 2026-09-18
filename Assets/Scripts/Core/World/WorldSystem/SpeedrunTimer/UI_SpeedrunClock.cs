using UnityEngine;

public class UI_SpeedrunClock : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI clock_text;

    private void Update()
    {
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        clock_text.enabled = GameManager.State != GameState.Cinematic;
        clock_text.text = convert_sec_to_hms(World.Instance.Timer.Time);
    }

    private string convert_sec_to_hms(float sec)
    {
        int min = (int)sec / 60;
        if (min <= 0) { return sec.ToString("F2"); } // only seconds
        
        // we have some minutes !
        sec -= min*60;
        int hour = min / 60 ;
        if (hour <= 0) { return $"{min}min {sec:F2}s"; }

        // we have some hours !
        min -= hour*60;
        return $"{hour}h {min}min {sec:F2}s";
    }
}