using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using TMPro;


[RequireComponent(typeof(AnimationHandler), typeof(BoxCollider2D))]
public class ManholeSewer : OldChest, I_Interactable
{

    [Header("MANHOLE SEWER")]
    [SerializeField] private GameObject zombie_prefab;
    [SerializeField] private Vector2 zombie_spawn_offset = new Vector2(0, 0);
    [SerializeField] private bool closable = false;
    [SerializeField] private GameObject perso;
    [SerializeField] private BoxCollider2D boxcol;

    public Transform interact_tuto_label { get; set; }

    // unity functions
    protected override void Awake()
    {
        // on met les bonnes animations
        anims = new ManholeSewerAnims();

        // on récupère le prefab du zombie
        zombie_prefab = Resources.Load("prefabs/beings/enemies/zombo") as GameObject;

        if (zombie_prefab == null)
        {
            Debug.LogWarning("ManholeSewer: zombie_prefab not found");
        }

        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le collider
        boxcol = transform.Find("detector").GetComponent<BoxCollider2D>();

        // on récup_re l'interract tuto label
        interact_tuto_label = transform.Find("interact_tuto_label");

        base.Awake();
    }

    void Update()
    {
        // Debug.Log("(manhole) lookin for player at position" + perso.transform.position);
        // we check if we collide with the perso
        if (/* !was_triggered && */ !is_open && !is_moving && perso != null && boxcol.bounds.Contains(perso.transform.position))
        {
            Debug.Log("MANHOLE TRIGERRED by " + perso.gameObject.name);
            open();
            Invoke("autoriseClosing", 10f);
        }
    }

    protected override void success_open()
    {
        base.success_open();
        
        // on libère un zombie à l'emplacement du manhole
        GameObject zombie = Instantiate(zombie_prefab, transform.position + new Vector3(zombie_spawn_offset.x, zombie_spawn_offset.y, 0), Quaternion.identity);
    }
    
    protected void autoriseClosing()
    {
        closable = true;
    }

    // INTERACTION
    public bool isInteractable()
    {
        return closable && !is_moving && is_open;
    }

    public void interact(GameObject target)
    {
        if (!isInteractable()) { return; }
        closable = false;
        close();
    }

    public void stopInteract()
    {
        OnPlayerInteractRangeExit();
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

public class ManholeSewerAnims : ChestAnims
{
    public ManholeSewerAnims()
    {
        openin = "manhole_sewer_openin";
        closin = "manhole_sewer_closin";
        idle_open = "manhole_sewer_idle_open";
        idle_closed = "manhole_sewer_idle_closed";
    }    

}