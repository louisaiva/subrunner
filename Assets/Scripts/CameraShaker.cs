using UnityEngine;
using System.Collections.Generic;

public class CameraShaker : MonoBehaviour
{
    protected AnimationHandler anim_handler;
    [SerializeField] protected float shake_duration = 0.5f;

    [SerializeField] private List<string> shake_anims = new List<string>() { "camera_shake01", "camera_shake02"};


    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
    }

    public void shake()
    {
        CancelInvoke("stopShaking"); // on annule l'invocation de "stopShaking" si elle existe

        // on joue l'animation
        anim_handler.ChangeAnim(shake_anims[Random.Range(0, shake_anims.Count)], shake_duration);

        Invoke("stopShaking", shake_duration);
    }

    public void stopShaking()
    {
        // on joue l'animation
        anim_handler.ChangeAnim("camera_idle");
    }
}