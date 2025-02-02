using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class DoorHackableLR : DoorHackable
{

    // UNITY FUNCTIONS
    protected new void Start()
    {
        // on change les bonnes animations
        // anims.addGlobalModif("ss");

        base.Start();

        // on change la direction de la porte
        // door_axis = "horizontal";
    }


    /* protected override void success_open()
    {
        base.success_open();

        // on modifie le sorting layer pour le mettre au sol et l'order à 1
        GetComponent<SpriteRenderer>().sortingLayerName = "ground";
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    protected override void close()
    {
        base.close();

        // on modifie le sorting layer pour le mettre dans le main
        GetComponent<SpriteRenderer>().sortingLayerName = "main";
        GetComponent<SpriteRenderer>().sortingOrder = 0;
    } */
}