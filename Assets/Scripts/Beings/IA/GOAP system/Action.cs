using UnityEngine;

public class Action : MonoBehaviour
{
    [Header("Action")]
    public float cost = 1f; // Cost of the action, lower means better
    public bool done = false; // If the action is done or not
    protected IA ia;

    [Header("Debug")]
    public bool debug = false; // If the action should be debugged or not

    // AWAKE
    protected virtual void Awake()
    {
        // Get the IA component from the parent Being
        ia = transform.parent.parent.parent.GetComponent<IA>();
        if (ia == null)
        {
            Debug.LogError("(Action) " + name + " must be attached to a IA!");
        }
    }

    // UPDATE
    public virtual void UpdateAction() { }

    // DOING / SUCCEEDING
    public virtual void Do() { }
    protected virtual void succeed(bool mark_as_done = true)
    {
        if (!mark_as_done)
        {
            if (debug) { Debug.Log("(Action) " + name + " is not done but finished"); }
            return; // we don't mark the action as done
        }

        // Mark the action as done
        done = true;
        if (debug) { Debug.Log("(Action) " + name + " is done!"); }
    }


    // QUITTING (not succed sadly ://)
    public void Quit()
    {
        // this is called when the goal ClearPlan(). so basically the action will be deleted after this
        succeed(mark_as_done:false);
    }
}