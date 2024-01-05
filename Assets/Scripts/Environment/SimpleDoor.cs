using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class SimpleDoor : Door
{

    // [SerializeField] private float openin_duration = 1f;

    // PERSO
    protected GameObject perso;

    [Header("OPENING")]
    [SerializeField] protected float auto_openin_radius = 2f;

    // UNITY FUNCTIONS
    protected new void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        base.Start();
    }

    protected void Update()
    {

        // on check si le perso est dans le rayon d'ouverture
        if (!is_open && !is_moving)
        {
            if (Vector2.Distance(transform.position, perso.transform.position) < auto_openin_radius)
            {
                open();
            }
        }
        else if (is_open && !is_moving)
        {
            if (Vector2.Distance(transform.position, perso.transform.position) > auto_openin_radius)
            {
                close();
            }
        }

    }

}