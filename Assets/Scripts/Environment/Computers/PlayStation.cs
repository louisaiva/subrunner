using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class PlayStation : Computer
{

    // UNITY FUNCTIONS
    new void Start()
    {
        os_names = new List<string> { "GS X",
                                    "GS 4.2",
                                    "GS 4.3",
                                    "GS 4.4",
                                    "GS 5.01",
                                    "GS 5.03",
                                    "GS 5.05",
                                    "GS Pro 5.1",
                                    "GS 2X"
                                    };

        base.Start();
        anims = new PlayStationAnims();
    }


    public override void succeedTurnOn()
    {
        base.succeedTurnOn();

        // on allume les lumières
        transform.Find("screen_light").gameObject.SetActive(true);
    }

    public override void turnOff()
    {
        // on éteint les lumières
        transform.Find("screen_light").gameObject.SetActive(false);

        base.turnOff();
    }
}

public class PlayStationAnims : ComputerAnims
{
    // ANIMATIONS
    public PlayStationAnims()
    {
        idle_on = "playstation_idle_on";
        idle_off = "playstation_idle_off";
        onin = "playstation_powering_on";
        offin = "playstation_powering_off";
        hackin = "playstation_hacked";
    }
}