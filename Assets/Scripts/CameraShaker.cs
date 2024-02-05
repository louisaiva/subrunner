using UnityEngine;
using System.Collections.Generic;

public class CameraShaker : MonoBehaviour
{
    protected AnimationHandler anim_handler;
    [SerializeField] protected float shake_duration = 0.5f;

    [SerializeField] private List<string> basic_shake_anims = new List<string>() { "camera_shake01", "camera_shake02" };
    [SerializeField] private List<string> big_shake_anims = new List<string>() { "camera_shake03", "camera_shake04" };


    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
    }

    public void shake(float magnitude=1f)
    {
        CancelInvoke("stopShaking"); // on annule l'invocation de "stopShaking" si elle existe

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
    }
}