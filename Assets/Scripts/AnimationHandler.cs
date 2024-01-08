using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    Animator animator;
    public string current_anim;

    // forcing one animation to play till the end
    public bool is_forcing = false;
    public bool debug = false;

    // states
    Dictionary<string,float> clips_length = new Dictionary<string, float>();

    // UNITY FUNCTIONS

    void Start()
    {
        animator = GetComponent<Animator>();

        // debug
        string s="";

        // on récup les states
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            clips_length.Add(clip.name, clip.length);
            s += clip.name + " " + clip.length + "\n";
        }
        if (debug) { print("states : " + s); }
    }

    void Update()
    {
        // if (debug) { print("anim : " + current_anim + " / length : " + GetCurrentAnimLength() + " / time : "+ GetCurrentAnimTime()); }
    }
    // MAIN FUNCTIONS

    public bool ChangeAnim(string next_anim, float duration =0f)
    {
        // if (current_anim == next_anim || is_forcing) { return false; }
        if (is_forcing) { return false; }

        if (duration != 0f)
        {
            // on change la vitesse de l'animation
            float speed = clips_length[next_anim] / duration;
            animator.speed = speed;
        }
        else
        {
            // on remet la vitesse de l'animation à 1
            animator.speed = 1f;
        }
        
        if (debug) { print("changing anim to " + next_anim + " for " + duration + " seconds, with animator speed set to" + animator.speed ); }
        
        current_anim = next_anim;
        animator.Play(next_anim);
        
        return true;
    }

    public bool ChangeAnimTilEnd(string next_anim, float duration =0f)
    {
        bool changed = ChangeAnim(next_anim, duration);
        if (!changed) { return false; }

        // on force l'animation à se jouer jusqu'à la fin
        ForceTilEnd(clips_length[next_anim]);

        return true;
    }

    public void StopForcing()
    {
        if (debug) { print("stop forcing"); }
        is_forcing = false;
    }

    public void ForceTilEnd(){
        
        CancelInvoke("StopForcing");

        // on regarde la longueur de l'animation
        float anim_length = GetCurrentAnimLength();

        // on regarde à quel moment on est dans l'animation
        float anim_time = GetCurrentAnimTime();

        // on calcule le temps restant
        float remaining_time = anim_length - anim_time;
        if (debug) {print("remaining time : " + remaining_time + " anim_length : " + anim_length + " anim_time : " + anim_time);}

        // on force l'animation à se jouer jusqu'à la fin
        is_forcing = true;

        // on arrête le forcing à la fin de l'animation
        Invoke("StopForcing", remaining_time);
    }

    public void ForceTilEnd(float duration)
    {
        CancelInvoke("StopForcing");

        // on force l'animation à se jouer jusqu'à la fin
        is_forcing = true;

        // on arrête le forcing à la fin de l'animation
        Invoke("StopForcing", duration);
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

    public float GetAnimLength(string anim)
    {
        return clips_length[anim];
    }

    public float GetCurrentAnimLength()
    {
        if (current_anim == null || current_anim == "") { return 0f; }
        return clips_length[current_anim];
        // return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public float GetCurrentNormalizedCumulatedAnimTime()
    {
        // if (debug) {print("normalized time : " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime + " / anim length : " + GetCurrentAnimLength());}

        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public float GetCurrentNormalizedAnimTime()
    {
        float cumulated_time = GetCurrentNormalizedCumulatedAnimTime();

        // on transforme le temps cumulé en temps unique
        float unique_time = cumulated_time % 1f;
        // if (debug) { print("unique time : " + unique_time + " / anim length : " + GetCurrentAnimLength()); }
        return unique_time;
    }
    
    public float GetCurrentAnimTime()
    {
        return GetCurrentNormalizedAnimTime() * GetCurrentAnimLength();
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