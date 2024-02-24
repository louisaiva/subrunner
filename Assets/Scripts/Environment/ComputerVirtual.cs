using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class ComputerVirtual : Computer
{

    // UNITY FUNCTIONS
    void Start()
    {
        base.Start();
        anims = new ComputerVirtualAnims();
    }


    public override void succeedTurnOn()
    {
        base.succeedTurnOn();

        // on allume les lumières
        transform.Find("screen_light").gameObject.SetActive(true);
        transform.Find("kb_light").gameObject.SetActive(true);
    }

    public override void turnOff()
    {
        base.turnOff();

        // on éteint les lumières
        transform.Find("screen_light").gameObject.SetActive(false);
        transform.Find("kb_light").gameObject.SetActive(false);
    }
}

public class ComputerVirtualAnims : ComputerAnims
{
    // ANIMATIONS
    public ComputerVirtualAnims()
    {
        idle_on = "new" + idle_on;
        idle_off = "new" + idle_off;
        onin = "new" + onin;
        hackin = "new" + hackin;
    }
}