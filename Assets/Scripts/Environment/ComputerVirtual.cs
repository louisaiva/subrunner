using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class ComputerVirtual : Computer
{

    // UNITY FUNCTIONS
    new void Start()
    {
        os_names = new List<string> { "bee F14",
                                            "bee - 16.04.1 LTS",
                                            "bee - 14.04.1 LTS",
                                            "bee F18",
                                            "bee Y1",
                                            "bee SWARM"};

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
        // on éteint les lumières
        transform.Find("screen_light").gameObject.SetActive(false);
        transform.Find("kb_light").gameObject.SetActive(false);
    
        base.turnOff();
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
        offin = "new" + offin;
        hackin = "new" + hackin;
    }
}