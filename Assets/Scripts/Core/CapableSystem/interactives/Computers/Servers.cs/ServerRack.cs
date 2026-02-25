using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerRack : Computer
{
    protected override void Start()
    {
        base.Start();

        // we randomly activate our servers
        Transform servers_parent = transform.Find("servers");
        for (int i = 0; i < servers_parent.childCount; i++)
        {
            Transform server = servers_parent.GetChild(i);
            server.gameObject.SetActive(Random.value > 0.5f);
        }
    }

}