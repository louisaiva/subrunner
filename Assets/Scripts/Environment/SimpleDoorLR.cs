using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class SimpleDoorLR : SimpleDoor
{

    // [SerializeField] private float openin_duration = 1f;

    // UNITY FUNCTIONS
    protected void Start()
    {
        // on change les bonnes animations
        anims.addGlobalModif("ss");

        base.Start();
    }

}