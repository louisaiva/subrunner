using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;


public class ChestDecoy : Chest
{

    /* [Header("Hackable")]
    [SerializeField] private int securityLevel = 1;
    public List<Hack> RunningHacks { get; private set; } = new List<Hack>();
    public int SecurityLevel => securityLevel;

    // TARGETED
    public SpriteRenderer spriteRenderer { get; private set; }
    public Material TargetMaterial { get; private set; }
    public Material DefaultMaterial { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        DefaultMaterial = spriteRenderer.material;
        TargetMaterial = Resources.Load<Material>("materials/targeted/hack_door");
    }


    // HACKABLE
    public bool IsVulnerableTo(Exploit exploit)
    {
        if (exploit == Exploit.Nmap) { return true; }
        if (exploit.name == "trojan") { return true; }
        return false;
    }
    public void OnHackStarted(Hack hack)
    {
        // Handle the hack start event
        if (debug) { Debug.Log($"(ChestDecoy) Hack started on {name} with exploit {hack.name}"); }

        // we show the hacked animation
        // anim_player.Play("hacked");

        RunningHacks.Add(hack);
    }
    public void OnHackFailed(Hack hack)
    {
        // Handle the hack failure event
        if (debug) { Debug.Log($"(ChestDecoy) Hack failed on {name} with exploit {hack.name}"); }
        
        if (Controller.Instance.Capable == this)
        {
            Controller.Instance.ResetCapableTarget();
        }
    }
    public void OnHackSucceeded(Hack hack)
    {
        if (debug) { Debug.Log($"(ChestDecoy) Hack completed on {name} with exploit {hack.name}"); }

        // if the hack was successful we unlock the door
        if (hack.name == "trojan")
        {
            // we make the perso target the chest
            Controller.Instance.ChangeCapableTarget(this, (hack.program as Exploit).end_timer);
        }
    }
    public void OnHackDone(Hack hack)
    {
        if (RunningHacks.Contains(hack)) { RunningHacks.Remove(hack); } // if the zombo is dead we may have already removed the hack
    } */

}