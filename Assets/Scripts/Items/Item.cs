using UnityEngine;

public class Item : MonoBehaviour {


    // item basics
    public string item_type = "physical";
    public string item_name = "heal_potion";
    public string item_description = "this is a heal potion";


    // UI
    public bool is_showed = false;
    private SpriteRenderer sprite_renderer;

    // unity functions
    protected void Start()
    {
        // on récupère le sprite renderer
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    protected void Update()
    {
        // on met à jour l'affichage
        sprite_renderer.enabled = is_showed;
    }

    // functions
    public void changeShow(bool is_showed)
    {
        // on met à jour l'affichage
        this.is_showed = is_showed;
    }

}