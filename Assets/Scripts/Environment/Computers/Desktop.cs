using UnityEngine;

public class Desktop : MonoBehaviour, I_Interactable
{
    
    private bool on = false;
    private Animator animator;



    private void Start()
    {
        animator = GetComponent<Animator>();


        if (interact_tuto_label == null)
        {
            interact_tuto_label = transform.Find("interact_tuto_label");
        }
    }


    // I_Interactable
    public Transform interact_tuto_label { get; set; }

    public bool isInteractable()
    {
        return true;
    }

    public void interact(GameObject target)
    {
        if (on)
        {
            animator.SetBool("on", false);
            on = false;
        }
        else
        {
            animator.SetBool("on", true);
            on = true;
        }
    }

    public void stopInteract() { }

    public void OnPlayerInteractRangeEnter()
    {
        // on affiche le label
        interact_tuto_label?.gameObject.SetActive(true);
    }

    public void OnPlayerInteractRangeExit()
    {
        // on cache le label
        interact_tuto_label?.gameObject.SetActive(false);
    }
}