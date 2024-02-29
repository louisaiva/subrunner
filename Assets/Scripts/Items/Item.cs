using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : MonoBehaviour, I_Interactable
{

    // item basics
    public string item_type = "item"; // is it a drink ? is it a glasses ? is it a hack ?
    public string category = "object"; // OBJECT / FOOD / DRINK / GLASSES / HACK
    public string action_type = "active"; // is it a capacity item ?
    public string item_name = "heal_potion"; // name of the item
    public string item_description = "this is a heal potion";

    // capacity
    public List<string> capacities = new List<string>();
    public Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    public int capacity_level = 1;

    // perso
    public GameObject perso;

    // interactable
    public Transform interact_tuto_label { get; set; }

    // UNITY FUNCTIONS

    protected void Awake()
    {

        // on récupère le perso
        perso = GameObject.Find("/perso");
        

        if (capacities.Contains("dash"))
        {
            // print("adding dash cooldown to " + gameObject.name);
            // on ajoute le cooldown
            cooldowns.Add("dash", 1f);
        }
        if (capacities.Contains("hit"))
        {
            // print("adding hit cooldown to " + gameObject.name);
            // on ajoute le cooldown
            cooldowns.Add("hit", 0.6f);
        }

    }

    protected void Start()
    {
        // on récupère le label
        interact_tuto_label = transform.Find("interact_tuto_label");
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
        interact_tuto_label.gameObject.SetActive(false);
    }
    // end of life
    public void destruct()
    {
        Destroy(gameObject);
    }

}