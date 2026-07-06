using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
    public bool is_running = false;

    private void Update()
    {
        if (!is_running) { return; }
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        World.Instance.data.speedrun_clock += UnityEngine.Time.deltaTime;
    }
    public void StartTimer() { is_running = true; }
    public void StopTimer() { is_running = false; }
    public float Time => World.Instance?.data?.speedrun_clock ?? 0f;
}