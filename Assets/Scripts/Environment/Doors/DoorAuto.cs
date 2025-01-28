using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class DoorAuto : Door
{

    // [SerializeField] private float openin_duration = 1f;

    [Header("OPENING")]
    [SerializeField] protected float auto_openin_radius = 2f;

    protected new void Update()
    {
        base.Update();

        if (is_moving) { return;}


        // on check si le perso est dans le rayon d'ouverture
        if (!is_open && Vector2.Distance(transform.position, perso.transform.position) < auto_openin_radius)
        {
            // if (!perso.GetComponent<Perso>().Can("badge")) { return; }
            open();
        }
        else if (is_open && Vector2.Distance(transform.position, perso.transform.position) > auto_openin_radius)
        {
            close();
        }
    }

}