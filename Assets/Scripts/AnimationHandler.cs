using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    Animator animator;
    private string current_anim;

    // forcing one animation to play till the end
    private bool is_forcing = false;

    // UNITY FUNCTIONS

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // MAIN FUNCTIONS

    public bool ChangeAnim(string next_anim)
    {
        if (current_anim == next_anim || is_forcing) { return false; }
        
        animator.Play(next_anim);
        current_anim = next_anim;
        
        return true;
    }

    public bool ChangeAnimTilEnd(string next_anim)
    {
        // print("jveux attaquer !!");

        bool changed = ChangeAnim(next_anim);
        if (!changed) { return false; }

        // si on a changé d'animation, on force l'animation à se jouer jusqu'à la fin
        is_forcing = true;
        Invoke("StopForcing", 0.35f);
        return true;
    }

    public void StopForcing()
    {
        is_forcing = false;
    }

    public void ForcedChangeAnim(string next_anim)
    {
        // ! attention ne pas utiliser h24
        // ! seulement pour la mort
        // ! overpass tous les checks
        // * utiliser ChangeAnim or ChangeAnimTilEnd à la place

        StopForcing();
        ChangeAnim(next_anim);
    }

    // GETTERS

    public float GetCurrentAnimLength()
    {
        print("is it the attacking state ? " + (animator.GetCurrentAnimatorStateInfo(0).IsName("zombo_attack_RL")? "yes":"no") +" length : " + animator.GetCurrentAnimatorStateInfo(0).length);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

}