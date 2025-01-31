using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : Movable, I_Interactable
{

    // item basics
    public string item_type = "item"; // is it a drink ? is it a glasses ? is it a hack ?
    public string category = "object"; // OBJECT / FOOD / DRINK / GLASSES / FILEHOLDER
    public string action_type = "active"; // is it a capacity item ?
    public string item_name = "heal_potion"; // name of the item
    public string item_description = "this is a heal potion";

    public string item_capacities = ""; // separated by a / : "dash/hit" and cooldowns by a : : "dash:0.7/hit:0.6"

    public int level = 1; // level of the item

    /* // capacity
    public List<string> capacities = new List<string>();
    public Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    public int capacity_level = 1;
    */

    // perso
    public GameObject perso;

    // interactable
    public Transform interact_tuto_label { get; set; }

    // UNITY FUNCTIONS
    public new virtual void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");


        base.Start();
        // world_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling", "Doors", "Computers", "Decoratives", "Interactives");

        // on ajoute les capacités
        foreach (string capacity in item_capacities.Split('/'))
        {
            // on récupère le nom et le cooldown
            string[] capacity_and_cooldown = capacity.Split(':');
            string capacity_name = capacity_and_cooldown[0];


            // todo !! maintenant on fait plus comme ça y'a pas besoin d'ajouter les capacity au Being,
            // todo ça se fait automatiquement via Can(capacity) qui vérifie les capacités de l'item
            /* if (capacity_and_cooldown.Length > 1)
            {
                try 
                {
                    float.Parse(capacity_and_cooldown[1]);
                }
                catch
                {
                    Debug.LogError("Item " + item_name + " has a wrong cooldown for capacity " + capacity_name + " to " + item_name);
                    continue;
                }
                float capacity_cooldown = float.Parse(capacity_and_cooldown[1]);
                // Debug.Log("(Item) successfully adding " + capacity_name + " with cooldown " + capacity_cooldown + " to " + item_name);
                // on ajoute le cooldown
                AddCapacity(capacity_name, capacity_cooldown);
            }
            else
            {
                AddCapacity(capacity_name);
                // Debug.Log("(Item) adding " + capacity_name + " to " + item_name);
            } */
        }

        // on récupère le label
        interact_tuto_label = transform.Find("interact_tuto_label");
        interact_tuto_label.Find("hc/text").GetComponent<TMPro.TextMeshPro>().text = item_name;

    }

    // interactions
    public bool isInteractable()
    {
        return true;
    }

    public void interact(GameObject target)
    {
        // on se fait ramasser par le perso
        perso.GetComponent<Perso>().grab(this);
    }

    public void stopInteract()
    {
        OnPlayerInteractRangeExit();
    }

    public void OnPlayerInteractRangeEnter()
    {
        if (interact_tuto_label == null)
        {
            interact_tuto_label = transform.Find("interact_tuto_label");
            if (interact_tuto_label == null) { return; }
        }

        // on affiche le label
        interact_tuto_label.gameObject.SetActive(true);
    }

    public void OnPlayerInteractRangeExit()
    {
        // on cache le label
        if (interact_tuto_label == null) { return; }
        interact_tuto_label.gameObject.SetActive(false);
    }


    // forces
    /* public void drop(Vector2 direction)
    {

    } */


    // end of life
    public void destruct()
    {
        Destroy(gameObject);
    }

}