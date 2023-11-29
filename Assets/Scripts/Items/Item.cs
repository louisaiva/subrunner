using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : MonoBehaviour
{


    // item basics
    public string item_type = "physical";
    public string action_type = "active";
    public string item_name = "heal_potion";
    public string item_description = "this is a heal potion";


    // UI
    public bool is_showed = false;
    private SpriteRenderer sprite_renderer;

    // on the ground
    public bool is_on_ground = false;
    public float scale_on_ground = 0.5f;
    public GameObject perso;

    // unity functions
    protected void Start()
    {
        // on récupère le sprite renderer
        sprite_renderer = GetComponent<SpriteRenderer>();

        // on récupère le perso
        perso = GameObject.Find("/perso");
    }

    protected void Update()
    {
        // on met à jour l'affichage
        sprite_renderer.enabled = is_showed;

        // on check les events
        Events();
    }

    void Events()
    {
        
        // on vérifie si on se fait cliquer dessus
        if (Input.GetMouseButtonDown(0) && is_showed && GetComponent<BoxCollider2D>() != null)
        {
            // on récupère la position de la souris
            Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
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
            }
        }
    }

    // functions
    public void changeShow(bool is_showed)
    {
        // on met à jour l'affichage
        this.is_showed = is_showed;
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
}