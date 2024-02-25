using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class Server : InteractChest
{

    // unity functions
    protected override void Awake()
    {
        anims = new ServerAnims();
        randomize_cat = "hack_file";
        base.Awake();
    }

}

public class ServerAnims : ChestAnims
{
    // constructor
    public ServerAnims()
    {
        idle_open = "server_idle_open";
        idle_closed = "server_idle_on";
        openin = "server_openin";
        closin = "server_closin";
    }

}