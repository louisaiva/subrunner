using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class SimpleDoorLR : SimpleDoor
{

    // [SerializeField] private float openin_duration = 1f;

    // UNITY FUNCTIONS
    protected new void Start()
    {
        // on change les bonnes animations
        anims.addGlobalModif("ss");

        base.Start();

        // on change la direction de la porte
        door_axis = "horizontal";
    }

}