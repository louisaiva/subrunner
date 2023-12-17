using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    Animator animator;
    public string current_anim;

    // forcing one animation to play till the end
    public bool is_forcing = false;

    // UNITY FUNCTIONS

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // MAIN FUNCTIONS

    public bool ChangeAnim(string next_anim, float duration =0f)
    {
        if (current_anim == next_anim || is_forcing) { return false; }
        
        animator.Play(next_anim);

        if (duration != 0f)
        {
            // on change la vitesse de l'animation
            float speed = animator.GetCurrentAnimatorStateInfo(0).length / duration;
            animator.speed = speed;
        }
        else
        {
            // on remet la vitesse de l'animation à 1
            animator.speed = 1f;
        }
        
        current_anim = next_anim;
        
        return true;
    }

    public bool ChangeAnimTilEnd(string next_anim, float duration =0f)
    {
        // print("jveux attaquer !!");

        bool changed = ChangeAnim(next_anim, duration);
        if (!changed) { return false; }

        // si on a changé d'animation, on force l'animation à se jouer jusqu'à la fin
        is_forcing = true;
        Invoke("ForceTilEnd", 0.05f);
        // ForceTilEnd();
        return true;
    }

    public void StopForcing()
    {
        is_forcing = false;
    }

    public void ForceTilEnd(){

        // on regarde la longueur de l'animation
        float anim_length = GetCurrentAnimLength();

        // on regarde à quel moment on est dans l'animation
        float anim_time = GetCurrentAnimTime();

        // on calcule le temps restant
        float remaining_time = anim_length - anim_time;

        // on force l'animation à se jouer jusqu'à la fin
        is_forcing = true;

        // on arrête le forcing à la fin de l'animation
        Invoke("StopForcing", remaining_time-0.05f);
    }

    public void ForcedChangeAnim(string next_anim, float duration =0f)
    {
        // ! attention ne pas utiliser h24
        // ! seulement pour la mort
        // ! overpass tous les checks
        // * utiliser ChangeAnim or ChangeAnimTilEnd à la place

        StopForcing();
        ChangeAnim(next_anim, duration);
    }

    // SWAP FUNCTIONS

    public void ForceSwapAnimTilEnd(string next_anim, float duration =0f)
    {
        // swap l'animation en commençant par la position de l'animation actuelle
        StopForcing();

        // on regarde à quel moment on est dans l'animation
        float anim_time = GetCurrentAnimTime();
        float next_anim_time = 1f - anim_time;

        print("swap anim at time : " + anim_time + " next_anim_time : " + next_anim_time);

        // on change l'animation
        animator.Play(next_anim, -1, next_anim_time);
        current_anim = next_anim;

        // on force l'animation à se jouer jusqu'à la fin
        ForceTilEnd();

    }

    // GETTERS

    public float GetCurrentAnimLength()
    {
        // print("is it the attacking state ? " + (animator.GetCurrentAnimatorStateInfo(0).IsName("zombo_attack_RL")? "yes":"no") +" length : " + animator.GetCurrentAnimatorStateInfo(0).length);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public float GetCurrentAnimTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public string GetCurrentAnimName()
    {
        return current_anim;
    }

    public bool IsForcing()
    {
        return is_forcing;
    }

}