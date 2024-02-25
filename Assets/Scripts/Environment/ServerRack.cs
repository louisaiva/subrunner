using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ServerRack : MonoBehaviour, I_Interactable
{

    // public bool is_interacting { get; set; } // est en train d'interagir

    [Header("SERVERS")]
    [SerializeField] private List<Server> servers = new List<Server>();
    int current_server = -1;

    // UNITY FUNCTIONS
    void Awake()
    {
        // on récup les serveurs
        foreach (Transform child in transform.Find("rack"))
        {
            if (child.GetComponent<Server>() != null)
            {
                servers.Add(child.GetComponent<Server>());
            }
        }
    }


    // INTERACTIONS
    public bool isInteractable()
    {
        // on regarde si on a un server non ouvert
        foreach (Server server in servers)
        {
            if (!server.is_opened())
            {
                return true;
            }
        }
        return false;
    }

    public void interact(GameObject target)
    {
        if (current_server == -1)
        {
            // on ouvre le premier server
            current_server = 0;
            servers[current_server].interact(target);
        }
        else
        {
            // on ferme le server actuel
            servers[current_server].stopInteract();

            // on ouvre le server suivant
            current_server++;
            if (current_server >= servers.Count)
            {
                current_server = 0;
            }
            servers[current_server].interact(target);
        }
    }

    public void stopInteract()
    {
        if (current_server != -1)
        {
            servers[current_server].stopInteract();
            current_server = -1;
        }
    }

}
