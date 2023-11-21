using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Being : MonoBehaviour
{

    // a BEING is a character that can move, attack, etc.

    // VIE
    private float vie = 100f;
    private int max_vie = 100;
    private float regen_vie = 0.1f; // vie par seconde

    // DEPLACEMENT
    public Vector2 inputs;
    public float vitesse = 3f; // vitesse de déplacement
    private Rect feet_collider; // collider des pieds (pour les collisions)
    private float offset_perso_y_to_feet = 0.45f;

    // GAMEOBJECTS
    public Animator animator;

    // unity functions
    protected void Start()
    {

        // on récupère les composants
        animator = GetComponent<Animator>();

        // on crée notre collider_box des pieds
        feet_collider = Rect.zero;
        feet_collider.size = new Vector2(0.4f, 0.1f);
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        inputs = new Vector2(0.1f,0f);
    }

    public virtual void Events()
    {

        // circular input
        // -> fait gauche droite etc

        if (inputs.x >= 0f && inputs.x < 1f){
            inputs.x *= 1.01f;

            if (inputs.x > 1f){
                inputs.x = 1f;
            }
        }
        else if (inputs.x >= 1f){
            inputs.x = -0.1f;
        }
        else if (inputs.x < 0f && inputs.x > -1f){
            inputs.x *= 1.01f;

            if (inputs.x < -1f){
                inputs.x = -1f;
            }
        }
        else if (inputs.x <= -1f){
            inputs.x = 0.1f;
        }
    }

    protected void Update()
    {
        // régèn de la vie
        if (vie < max_vie)
        {
            vie += regen_vie * Time.deltaTime;
        }

        // on récupère les inputs
        Events();

        // maj des animations
        maj_animations(inputs);

        // déplacement
        move_perso(inputs);
    }


    // DEPLACEMENT
    protected void move_perso(Vector2 direction)
    {

        // ******************************
        // * MOUVEMENT EN X
        // ******************************

        // on calcule le mouvement sur X
        float x_movement = direction.normalized.x * vitesse * Time.deltaTime;

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
        float y_movement = direction.normalized.y * vitesse * Time.deltaTime;

        // on maj notre collider_box
        feet_collider.x += x_movement;

        // on lance le raycast
        raycast_direction = new Vector2(0f, (y_movement > 0f) ? 1f : -1f);
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

    protected void maj_animations(Vector2 direction)
    {

        // sens du sprite
        if (direction.x != 0f) { GetComponent<SpriteRenderer>().flipX = (direction.x > 0f); }

        // animation de déplacement
        var input_tresh = 0.1f;
        if (Mathf.Abs(direction.x) > input_tresh || Mathf.Abs(direction.y) > input_tresh)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

    }

}