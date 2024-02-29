using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Stand : MonoBehaviour, I_Grabber, I_Interactable
{
    // can stack 1 GameObject
    [Header("Stand")]
    [SerializeField] protected Vector2 slot_offset = new Vector2(0, 0.66f);
    // [SerializeField] protected Vector2 ;
    public Transform interact_tuto_label { get; set; }

    private void Start()
    {
        // on récupère le label
        interact_tuto_label = transform.Find("interact_tuto_label");

        // on récupère les enfants et on désactive leur collider si ce ne sont pas des items
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] {"soft_light", "interact_tuto_label" }.Contains(child.name)) { continue; }

            // on place l'item sur le stand
            placeOnStand(child.gameObject);
        }
    }

    private void placeOnStand(GameObject target)
    {
        // on déplace l'item sur le stand
        // on désactive le collider
        if (target.GetComponent<Collider2D>())
        {
            target.GetComponent<Collider2D>().enabled = false;
        }
        
        // on a un item -> on déplace le collider de -offset/scale
        /* if (target.GetComponent<Collider2D>())
        {
            Vector2 comp_offset = -slot_offset / target.transform.localScale;

            target.GetComponent<Collider2D>().offset = comp_offset;

            if (target.GetComponent<BoxCollider2D>())
            {
                target.GetComponent<BoxCollider2D>().size /= target.transform.localScale;
            }
        } */

        if (target.transform.Find("glowing_particles"))
        {
            target.transform.Find("glowing_particles").GetComponent<GlowingItemEffect>().offset = -slot_offset;
        }

        // on déplace le gameobject de +offset pour le placer sur le stand
        target.transform.localPosition = new Vector3(slot_offset.x, slot_offset.y, 0);

    }

    // GRABBER
    public bool canGrab()
    {
        // on vérifie si on a déjà un item
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] {"soft_light","interact_tuto_label"}.Contains(child.name)) { continue; }
            return false;
        }
        return true;
    }

    public void grab(GameObject target)
    {
        // on déplace l'item sur le stand
        target.transform.SetParent(transform);
        placeOnStand(target);
    }



    // INTERACTIONS

    public bool isInteractable()
    {
        bool has_interactable = false;
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] {"soft_light","interact_tuto_label"}.Contains(child.name)) { continue; }

            if (child.GetComponent<I_Interactable>() != null)
            {
                has_interactable = true;
                break;
            }
        }

        return has_interactable;
    }

    public void interact(GameObject target)
    {
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] { "soft_light" }.Contains(child.name)) { continue; }

            if (child.GetComponent<I_Interactable>() != null)
            {
                child.GetComponent<I_Interactable>().interact(target);
                break;
            }
        }
    }

    public void stopInteract()
    {
        OnPlayerInteractRangeExit();

        if (!isInteractable()) { return; }
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas softlight
            if (new string[] { "soft_light" }.Contains(child.name)) { continue; }

            if (child.GetComponent<I_Interactable>() != null)
            {
                child.GetComponent<I_Interactable>().stopInteract();
                break;
            }
        }
    }

    public void OnPlayerInteractRangeEnter()
    {
        if (interact_tuto_label == null)
        {
            interact_tuto_label = transform.Find("interact_tuto_label");
            if (interact_tuto_label == null) { return; }
        }

        // on affiche le label
        interact_tuto_label.gameObject.SetActive(true);
    }

    public void OnPlayerInteractRangeExit()
    {
        // on cache le label
        interact_tuto_label.gameObject.SetActive(false);
    }
}