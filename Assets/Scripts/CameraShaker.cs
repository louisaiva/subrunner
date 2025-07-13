using UnityEngine;
using System.Collections.Generic;

public class CameraShaker : Singleton<CameraShaker>
{
    protected AnimationHandler anim_handler;

    [Header("Shake parameters")]
    [SerializeField] protected float shake_duration = 0.5f;
    [SerializeField] protected float shake_magnitude = 1f;

    [Header("Shake animations")]
    [SerializeField] private List<string> basic_shake_anims = new List<string>() { "camera_shake01", "camera_shake02" };
    [SerializeField] private List<string> big_shake_anims = new List<string>() { "camera_shake03", "camera_shake04" };

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
    }

    public void shake(float magnitude=1f)
    {
        CancelInvoke("stopShaking"); // on annule l'invocation de "stopShaking" si elle existe

        if (log) { Debug.Log("(CameraShaker) Shaking with magnitude: " + magnitude); }

        // on joue l'animation
        if (magnitude > 1f)
        {
            anim_handler.ChangeAnim(big_shake_anims[Random.Range(0, big_shake_anims.Count)], shake_duration);
        }
        else
        {
            anim_handler.ChangeAnim(basic_shake_anims[Random.Range(0, basic_shake_anims.Count)], shake_duration);
        }

        Invoke("stopShaking", shake_duration);
    }

    public void stopShaking()
    {
        // on joue l'animation
        anim_handler.ChangeAnim("camera_idle");
        if (log) { Debug.Log("(CameraShaker) Stopped shaking"); }
    }
}