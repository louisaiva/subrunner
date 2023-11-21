using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : MonoBehaviour
{

    // VIE
    private float vie = 100f;
    private int max_vie = 100;
    private float regen_vie = 0.1f; // vie par seconde

    // EXPLOITS
    private float exploits = 3f; // exploits = mana (lance des sorts de hacks)
    private int max_exploits = 3;
    private float regen_exploits = 0.1f; // exploits par seconde

    // DEPLACEMENT
    public float vitesse = 5f; // vitesse de déplacement
    private Rect feet_collider; // collider des pieds (pour les collisions)
    private float offset_perso_y_to_feet = 0.45f;
    public Vector2 inputs;

    // GAMEOBJECTS
    public GameObject world; // le monde : pour les collisions
    public Animator animator;
    public Collider2D collider_feet;


    /*


    */

    // unity functions
    void Start()
    {
        // on récupère les composants
        animator = GetComponent<Animator>();
        collider_feet = GetComponent<Collider2D>();

        // on crée notre collider_box des pieds
        feet_collider = Rect.zero;
        feet_collider.size = new Vector2(0.4f, 0.1f);
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);
    }
    
    void Events()
    {
        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // update de d'habitude
    void Update()
    {


        // régèn de la vie et des exploits
        if (vie < max_vie)
        {
            vie += regen_vie * Time.deltaTime;
        }
        if (exploits < max_exploits)
        {
            exploits += regen_exploits * Time.deltaTime;
        }

        // events
        Events();

        // sens du sprite
        if (inputs.x != 0f) { GetComponent<SpriteRenderer>().flipX = (inputs.x > 0f); }

        // animation de déplacement
        var input_tresh = 0.1f;
        if (Mathf.Abs(inputs.x) > input_tresh || Mathf.Abs(inputs.y) > input_tresh)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        // déplacement
        move_perso();
    }

    // DEPLACEMENT
    private void move_perso()
    {

        // ******************************
        // * MOUVEMENT EN X
        // ******************************

        // on calcule le mouvement sur X
        float x_movement = inputs.normalized.x * vitesse * Time.deltaTime;

        // on maj notre collider_box        
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        // on lance le raycast
        Vector2 raycast_direction = new Vector2((x_movement > 0f) ? 1f : -1f, 0f);
        RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(x_movement));

        // on vérifie si le mouvement touche un collider
        if (hit.collider != null)
        {

            float hit_treshold = 0.05f;
            if (hit.distance < hit_treshold && hit.normal.x * x_movement < 0f)
            {
                // on annule le movement parce qu'on est trop proche du collider
                // et qu'on essaie de renter dedans
                x_movement = 0f;
            }
            else
            {
                // on ajuste le vecteur de déplacement
                x_movement = hit.distance + hit.normal.x * hit_treshold;
            }
            
        }

        // on déplace le perso
        transform.position += new Vector3(x_movement, 0f, 0f);




        // ******************************
        // * MOUVEMENT EN Y
        // ******************************

        // on calcule le mouvement sur Y
        float y_movement = inputs.normalized.y * vitesse * Time.deltaTime;

        // on maj notre collider_box
        feet_collider.x += x_movement;

        // on lance le raycast
        raycast_direction = new Vector2(0f,(y_movement > 0f) ? 1f : -1f);
        hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(y_movement));

        // on vérifie si le mouvement touche un collider
        if (hit.collider != null)
        {
            float hit_treshold = 0.02f;
            if (hit.distance < hit_treshold && hit.normal.y * y_movement < 0f)
            {
                // on annule le movement parce qu'on est trop proche du collider
                // et qu'on essaie de renter dedans
                y_movement = 0f;
            }
            else
            {
                // on ajuste le vecteur de déplacement
                y_movement = hit.distance + hit.normal.y * hit_treshold;
            }

        }

        // on déplace le perso
        transform.position += new Vector3(0f, y_movement, 0f);

    }

}
