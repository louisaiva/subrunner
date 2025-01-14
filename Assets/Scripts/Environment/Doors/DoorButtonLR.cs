using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class DoorButtonLR : DoorLR, I_Buttonable
{

    // BUTTONS
    public void buttonDown()
    {
        open();
    }
    public void buttonUp()
    {
        close();
    }
}