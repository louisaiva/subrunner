using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AnimationHandler))]
public class Being : MonoBehaviour
{

    // a BEING is a character that can move, attack, etc.

    // VIE
    public float vie = 100f;
    public int max_vie = 100;
    private float regen_vie = 0.01f; // pourcentage de vie_max par seconde
    public float weight = 1f; // poids du perso (pour le knockback)

    // HURTED
    // private float last_hurted_time = 0f; // temps de la dernière attaque subie
    public GameObject xp_provider;
    private int xp_gift = 10;

    // DEPLACEMENT
    public Vector2 inputs;
    public float vitesse = 3f; // vitesse de déplacement
    private Rect feet_collider; // collider des pieds (pour les collisions)
    private float offset_perso_y_to_feet = 0.45f; // offset entre le perso et le collider des pieds
    public LayerMask world_layers; // layers du monde
    private bool isMoving = false;

    // ANIMATIONS
    protected AnimationHandler anim_handler;
    protected Vector2 lookin_at = new Vector2(0f, -1f);
    protected float lookin_at_angle = 40f; // angle du regard du perso en degrés
    protected Anims anims = new Anims();



    // unity functions
    protected void Start()
    {

        // on récupère les composants
        // gameObject.AddComponent<AnimationHandler>();
        anim_handler = GetComponent<AnimationHandler>();

        // on crée notre collider_box des pieds
        feet_collider = Rect.zero;
        feet_collider.size = new Vector2(0.4f, 0.1f);
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        inputs = new Vector2(0.1f,0f);

        // on défini les layers du monde
        world_layers = LayerMask.GetMask("World","Doors");

        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/world/xp_provider");
    }

    public virtual void Events()
    {
        inputs = simulate_circular_input_on_x(inputs);
    }

    protected void Update()
    {
        // on vérifie si le perso est mort
        if (!isAlive()) { return; }

        // régèn de la vie
        if (vie < max_vie)
        {
            vie += regen_vie * max_vie * Time.deltaTime;
        }

        // on récupère les inputs
        Events();

        // on calcule la direction du regard
        calculate_lookin_at(inputs);

        // maj des animations
        maj_animations(inputs);

        // déplacement
        move_perso(inputs);
    }

    protected void OnDrawGizmosSelected()
    {

        // calculate lookin_at vector base position
        Vector3 lookin_at_pos = transform.position + new Vector3(lookin_at.x, lookin_at.y, 0f);
        
        // draw lookin_at angle
        Vector3[] points = new Vector3[3]
        {
            transform.position,
            transform.position + Quaternion.Euler(0f, 0f, lookin_at_angle/2f) * (lookin_at_pos - transform.position),
            transform.position + Quaternion.Euler(0f, 0f, -lookin_at_angle/2f) * (lookin_at_pos - transform.position)
        };

        Gizmos.color = Color.red;
        Gizmos.DrawLineStrip(points, true);
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
        RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(x_movement), world_layers);

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
        hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(y_movement), world_layers);

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

    protected Vector2 simulate_circular_input_on_x(Vector2 input_vecteur)
    {
        // circular input
        // -> fait gauche droite etc

        if (input_vecteur.x >= 0f && input_vecteur.x < 1f)
        {
            input_vecteur.x *= 1.01f;

            if (input_vecteur.x > 1f)
            {
                input_vecteur.x = 1f;
            }
        }
        else if (input_vecteur.x >= 1f)
        {
            input_vecteur.x = -0.1f;
        }
        else if (input_vecteur.x < 0f && input_vecteur.x > -1f)
        {
            input_vecteur.x *= 1.01f;

            if (input_vecteur.x < -1f)
            {
                input_vecteur.x = -1f;
            }
        }
        else if (input_vecteur.x <= -1f)
        {
            input_vecteur.x = 0.1f;
        }

        return input_vecteur;
    }

    protected void apply_force(Vector2 force)
    {

        // ******************************
        // * MOUVEMENT EN X
        // ******************************

        // on calcule le mouvement sur X
        float x_movement = force.x;

        // on maj notre collider_box        
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        // on lance le raycast
        Vector2 raycast_direction = new Vector2((x_movement > 0f) ? 1f : -1f, 0f);
        RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(x_movement), world_layers);

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
        float y_movement = force.y;

        // on maj notre collider_box
        feet_collider.x += x_movement;

        // on lance le raycast
        raycast_direction = new Vector2(0f, (y_movement > 0f) ? 1f : -1f);
        hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(y_movement), world_layers);

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

        // on applique une force au perso
        // transform.position += new Vector3(force.x, force.y, 0f);
    }

    // ANIMATIONS
    protected void maj_animations(Vector2 direction)
    {

        // animation de déplacement (en fonction des inputs)
        var input_tresh = 0.1f;
        isMoving = (direction.magnitude > input_tresh);

        // animation de direction (en fonction du regard)
        if (anims.has_extended_animation)
        {
            // on calcule la direction qu'on doit envoyer à l'animator
            // int anim_direction = 0;

            // si le vecteur direction pointe vers le bas, on envoie -1
            // s'il pointe vers le haut on envoie 1
            // sinon on envoie 0

            if (lookin_at.y < -Mathf.Abs(lookin_at.x))
            {
                // on regarde en bas
                if (isMoving)
                {
                    anim_handler.ChangeAnim(anims.run_down);
                }
                else
                {
                    anim_handler.ChangeAnim(anims.idle_down);
                }
            }
            else if (lookin_at.y > Mathf.Abs(lookin_at.x))
            {
                // on regarde en haut
                if (isMoving)
                {
                    anim_handler.ChangeAnim(anims.run_up);
                }
                else
                {
                    anim_handler.ChangeAnim(anims.idle_up);
                }
            }
            else
            {
                // on regarde à gauche ou à droite
                if (isMoving)
                {
                    anim_handler.ChangeAnim(anims.run_side);
                }
                else
                {
                    anim_handler.ChangeAnim(anims.idle_side);
                }
            }

            // on joue la bonne animation
            // anim_handler.ChangeAnim(anim_direction);
        }
        else
        {
            // on est obligé de regarder à gauche ou à droite (pas d'anim haut ou bas)
            if (isMoving)
            {
                anim_handler.ChangeAnim(anims.run_side);
            }
            else
            {
                anim_handler.ChangeAnim(anims.idle_side);
            }
        }

        // sens du sprite (en fonction du regard)
        // on change seulement si on ne subit pas de knockback
        if (anim_handler.GetCurrentAnimName() != anims.hurted)
        {
            if (Mathf.Abs(lookin_at.x) >= Mathf.Abs(lookin_at.y)) { GetComponent<SpriteRenderer>().flipX = (lookin_at.x > 0f); }
        }

    }

    protected void calculate_lookin_at(Vector2 direction)
    {
        // on calcule la direction du regard
        if (direction != Vector2.zero)
        {
            lookin_at = direction.normalized;
        }
    }


    // DAMAGE
    public void take_damage(float damage, Vector2 knockback)
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        vie -= damage;

        // play hurt animation
        anim_handler.ChangeAnimTilEnd(anims.hurted);

        // knockback
        apply_force(knockback);
        
        // change the flipX of the sprite if needed
        if (knockback.x != 0f)
        {
            GetComponent<SpriteRenderer>().flipX = (knockback.x < 0f);
        }

        // check if dead
        if (vie <= 0f)
        {
            die();
        }
    }

    protected virtual void die()
    {
        // play death animation
        anim_handler.ForcedChangeAnim(anims.die);

        // destroy object        
        Invoke("DestroyObject", 60f);

        // on donne de l'xp
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_gift, transform.position);
    }

    protected void DestroyObject()
    {
        if (!isAlive()){
            Destroy(gameObject);
        }
    }


    // GETTERS
    public bool isAlive()
    {
        return vie > 0f;
    }
}

public class Anims
{
    public bool has_extended_animation = false;


    // ANIMATIONS
    public string idle_down = "idle_D";
    public string idle_up = "idle_U";
    public string idle_side = "idle_RL";
    public string run_down = "runnin_D";
    public string run_up = "runnin_U";
    public string run_side = "runnin_RL";
    public string attack = "attack_RL";
    public string die = "die";
    public string hurted = "hurted_RL";

    public void init(string name)
    {
        if (name == "perso")
        {
            has_extended_animation = true;
        }

        idle_down = name + "_" + idle_down;
        idle_up = name + "_" + idle_up;
        idle_side = name + "_" + idle_side;
        run_down = name + "_" + run_down;
        run_up = name + "_" + run_up;
        run_side = name + "_" + run_side;
        attack = name + "_" + attack;
        die = name + "_" + die;
        hurted = name + "_" + hurted;
    }
}