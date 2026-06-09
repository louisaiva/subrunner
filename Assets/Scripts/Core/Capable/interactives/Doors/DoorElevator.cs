using UnityEngine;
using System.Collections;


public class DoorElevator : Door
{

    [Header("Automatic closing")]
    public float auto_close_delay = 10f;
    public float auto_close_timer = 0f;

    // ON INTERACT
    public override void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we react to the interaction
        if (!Opener.Able) { return; }

        // on ouvre la porte
        Open();

        // on reset le timer de fermeture automatique
        auto_close_timer = auto_close_delay;
    }

    protected override void Update()
    {
        base.Update();

        // on update le timer de fermeture automatique
        if (auto_close_timer > 0f)
        {
            auto_close_timer -= Time.deltaTime;
            if (auto_close_timer <= 0f)
            {
                auto_close_timer = 0f;
                if (Closer.Able) { Close(); }
            }
        }
    }
}