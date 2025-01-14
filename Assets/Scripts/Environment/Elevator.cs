using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
public class Elevator : MonoBehaviour, I_Interactable
{


    // ANIMATION
    protected AnimationHandler anim_handler;
    protected ElevatorAnims anims = new ElevatorAnims();



    // INTERACTABLE
    Transform I_Interactable.interact_tuto_label { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void I_Interactable.interact(GameObject target)
    {
        throw new System.NotImplementedException();
    }

    bool I_Interactable.isInteractable()
    {
        throw new System.NotImplementedException();
    }

    void I_Interactable.OnPlayerInteractRangeEnter()
    {
        throw new System.NotImplementedException();
    }

    void I_Interactable.OnPlayerInteractRangeExit()
    {
        throw new System.NotImplementedException();
    }

    void I_Interactable.stopInteract()
    {
        throw new System.NotImplementedException();
    }
}

public class ElevatorAnims
{
}