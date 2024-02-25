using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ServerRack : MonoBehaviour, I_Interactable
{

    [Header("SERVERS")]
    [SerializeField] private bool randomize_on_start = true;
    [SerializeField] private List<Server> servers = new List<Server>();
    [SerializeField] private int current_server = -1;

    [SerializeField] private GameObject server_prefab;
    [SerializeField] private List<Vector3> server_positions = new List<Vector3>();


    // UNITY FUNCTIONS
    
    void Start()
    {

        // on récupère le prefab du server
        server_prefab = Resources.Load("prefabs/objects/server") as GameObject;

        // on récupère les positions des serveurs
        server_positions = new List<Vector3>() {
            new Vector3(0, 0.125f, 0f),
            new Vector3(0, 0.25f+2/24f, 0f),
            new Vector3(0, 0.5f+1/24f, 0f),
            new Vector3(0, 0.75f, 0f)
        };	

        // on randomize les serveurs
        if (randomize_on_start)
        {
            randomize();
        }
        else
        {
            updateServers();
        }
    }


    // servers
    void updateServers()
    {
        // on clear la liste
        servers.Clear();

        // on récup les serveurs
        foreach (Transform child in transform.Find("rack"))
        {
            if (child.GetComponent<Server>() != null)
            {
                servers.Add(child.GetComponent<Server>());
            }
        }
    }

    void randomize()
    {
        // on clear la liste
        servers.Clear();

        // on clear les serveurs
        foreach (Transform child in transform.Find("rack"))
        {
            if (child.GetComponent<Server>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // on randomize les serveurs
        foreach (Vector3 pos in server_positions)
        {
            if (Random.Range(0, 2) == 0)
            {
                GameObject server = Instantiate(server_prefab, transform.Find("rack").position + pos, Quaternion.identity);
                server.transform.parent = transform.Find("rack");
                servers.Add(server.GetComponent<Server>());
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
