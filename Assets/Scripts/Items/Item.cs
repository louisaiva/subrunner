using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : MonoBehaviour, I_Descriptable, I_Interactable
{


    // item basics
    public string item_type = "item"; // is it a drink ? is it a glasses ? is it a hack ?
    public string action_type = "active"; // is it a capacity item ?
    public string item_name = "heal_potion"; // name of the item
    public string item_description = "this is a heal potion";

    public bool legendary_item = false;


    // capacity
    public List<string> capacities = new List<string>();
    public Dictionary<string,float> cooldowns = new Dictionary<string,float>();
    public int capacity_level = 1;
 

    // UI
    public bool is_showed = false;
    private SpriteRenderer sprite_renderer;
    protected GameObject ui_bg;

    // on the ground
    public bool is_on_ground = false;
    public float scale_on_ground = 0.5f;
    public GameObject perso;

    // description
    public bool is_hoovered { get; set; }
    public GameObject description_ui;

    // cursor
    public CursorHandler cursor_handler;

    // unity functions
    protected void Awake()
    {
        // on récupère le sprite renderer
        sprite_renderer = GetComponent<SpriteRenderer>();

        // on récupère le ui_bg
        ui_bg = transform.Find("ui_bg").gameObject;

        // on met à jour le sprite du ui_bg en fonction de legendary_item
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/item_slots");
        if (legendary_item)
        {
            // on met un fond jaune
            ui_bg.GetComponent<SpriteRenderer>().sprite = sprites[2];
        }
        else
        {
            // on met un fond bleu de base
            ui_bg.GetComponent<SpriteRenderer>().sprite = sprites[0];
        }

        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le description_ui
        description_ui = GameObject.Find("/ui/hoover_description");

        // on récupère le cursor_handler
        cursor_handler = GameObject.Find("/utils").GetComponent<CursorHandler>();

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


        // on vérifie si on est sur le sol
        if (is_on_ground)
        {
            // on met à jour l'affichage
            changeShow(true);

            // on change le sorting layer pour devenir un objet du sol
            sprite_renderer.sortingLayerName = "main";
            sprite_renderer.sortingOrder = 0;

            // on change la scale
            transform.localScale = new Vector3(scale_on_ground, scale_on_ground, scale_on_ground);

            // on change le material
            sprite_renderer.material = Resources.Load<Material>("materials/sprite_lit_default");
        }
    }

    protected void Update()
    {
        // on met à jour l'affichage
        sprite_renderer.enabled = is_showed;

        // on met à jour l'affichage du ui_bg
        ui_bg.SetActive(is_showed && !is_on_ground);

        // on check les events
        Events();
    }

    void Events()
    {

        // on vérifie si on se fait survoler par la souris
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (is_showed)
        {
            if (GetComponent<BoxCollider2D>().OverlapPoint(mouse_pos))
            {
                if (!is_hoovered)
                {
                    if (!is_on_ground)
                    {
                        // on met à jour la description
                        description_ui.GetComponent<UI_HooverDescriptionHandler>().changeDescription(this);
                    }

                    // on met à jour le fait qu'on est survolé
                    is_hoovered = true;

                    // on change le cursor
                    cursor_handler.SetCursor("hand");
                }
            }
            else
            {
                if (is_hoovered)
                {
                    if (!is_on_ground)
                    {
                        // on met à jour l'affichage
                        description_ui.GetComponent<UI_HooverDescriptionHandler>().removeDescription(this);
                    }

                    // on met à jour le fait qu'on est survolé
                    is_hoovered = false;

                    // on change le cursor
                    cursor_handler.SetCursor("arrow");
                }
            }
        }

        // on vérifie si on se fait cliquer dessus
        if (Input.GetMouseButtonDown(0) && is_showed)
        {            
            // on regarde si ça overlap avec notre box collider
            if (GetComponent<BoxCollider2D>().OverlapPoint(mouse_pos))
            {
                if (is_on_ground)
                {
                    // on se fait ramasser par le perso
                    fromGroundToInv();
                    perso.GetComponent<Perso>().grab(this);
                }
                else
                {
                    // on se fait drop par notre inventaire
                    transform.parent.GetComponent<Inventory>().dropItem(this);
                }

                // on change le cursor
                cursor_handler.SetCursor("arrow");
            }
        }
    }

    // functions
    public void changeShow(bool is_showed)
    {
        // on met à jour l'affichage
        this.is_showed = is_showed;

        // on met à jour l'affichage du ui_bg
        ui_bg.SetActive(is_showed && !is_on_ground);

        // on met à jour is_hoovered
        if (!is_showed && is_hoovered)
        {
            is_hoovered = false;
        }
    }

    // transfer to ground
    public void fromInvToGround()
    {
        // on met à jour le fait qu'on est sur le sol
        is_on_ground = true;

        // on met à jour l'affichage
        changeShow(true);

        // on change le sorting layer pour devenir un objet du sol
        sprite_renderer.sortingLayerName = "main";
        sprite_renderer.sortingOrder = 0;

        // on change la scale
        // transform.localScale = new Vector3(scale_on_ground, scale_on_ground, scale_on_ground);

        // on change le material
        sprite_renderer.material = Resources.Load<Material>("materials/sprite_lit_default");
    }

    // transfer to inv
    public void fromGroundToInv()
    {
        // on met à jour le fait qu'on est sur le sol
        is_on_ground = false;

        // on met à jour l'affichage
        changeShow(false);

        // on change le sorting layer pour devenir un objet du sol
        sprite_renderer.sortingLayerName = "ui";
        sprite_renderer.sortingOrder = 1;

        // on change la scale
        // transform.localScale = new Vector3(1, 1, 1);

        // on change le material
        sprite_renderer.material = Resources.Load<Material>("materials/sprite_unlit_default");
    }

    // description
    public string getDescription()
    {

        string s = item_name + "\n\n" + item_description;
        return s;
    }

    public bool shouldDescriptionBeShown()
    {
        return is_showed && !is_on_ground;
    }

    // interactions
    public bool isInteractable()
    {
        return is_on_ground;
    }

    public void interact()
    {
        // on se fait ramasser par le perso
        fromGroundToInv();
        perso.GetComponent<Perso>().grab(this);
    }

    public void stopInteract() {}



    // end of life
    public void destruct()
    {
        if (!is_on_ground)
        {
            // on enlève l'item de l'inventaire
            transform.parent.GetComponent<Inventory>().removeItem(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}