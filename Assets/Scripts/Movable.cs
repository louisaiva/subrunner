using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// a MOVABLE is a character that can move with FORCES. that's it.
public class Movable : Capable
{


    [Header("MOVABLE")]
    public float weight = 1f; // poids du movable (pour le knockback)

    // FORCES
    public List<Force> forces = new List<Force>();

    // DEPLACEMENT
    // private Rect feet_collider; // collider des pieds (pour les collisions)
    private BoxCollider2D feet;
    // private float offset_movable_y_to_feet = 0f; // offset entre le movable et le collider des pieds
    public LayerMask world_layers; // layers du monde
    public LayerMask ghost_layers; // layers du monde (ghost)
    
    // CURRENT VELOCITY
    protected Vector3 last_position = Vector3.zero;
    public Vector2 velocity = Vector2.zero;

    // unity functions
    protected override void Start()
    {
        base.Start();

        // on ajoute la capacité de déplacement
        AddCapacity("walk");
        AddCapacity("run");

        // on récupère notre collider_box des pieds
        if (feet == null)
        {
            feet = transform.Find("run").GetComponent<BoxCollider2D>();
        }

        // on défini les layers du monde
        world_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling", "Doors", "Chests", "Computers", "Decoratives", "Interactives");
        ghost_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling");

    }


    protected override void Update()
    {
        base.Update();

        /* if (feet == null)
        {
            feet = transform.Find("run").GetComponent<BoxCollider2D>();
            if (feet == null) {return;}
        } */

        // on sauvegarde la position du movable
        last_position = transform.position;
        
        // update forces
        updateForces();

        // on calcule la current velocity
        Vector3 vel = (transform.position - last_position) / Time.deltaTime;
        velocity = new Vector2(vel.x, vel.y);

    }


    // FORCES
    public void addForce(Force force)
    {
        // on ajoute une force
        forces.Add(force);
    }

    public void clearForces()
    {
        // on supprime toutes les forces
        forces.Clear();
    }

    protected void updateForces()
    {
        // on parcourt les forces
        for (int i = 0; i < forces.Count; i++)
        {
            // on récupère la force
            Force force = forces[i];

            // on applique la force si elle est assez forte
            if (force.magnitude > 0.5f)
            {

                // on applique la force
                apply_force(force.direction * force.magnitude * Time.deltaTime);

                // on diminue la force
                force.magnitude = Mathf.Lerp(force.magnitude, 0, Time.deltaTime * 10f * force.attenuation);

                // on met à jour la force
                forces[i] = force;
            }
            else
            {
                // on supprime la force si elle est trop faible
                forces.RemoveAt(i);
                i--;
            }
        }
    }

    protected void apply_force(Vector2 force)
    {
        // on déplace le movable
        int moved_code = move(force);


        if (this is Perso)
        {

            if (force.magnitude < 0.05f) { return; }

            // on regarde si on a collisionné
            if (moved_code == 0 || moved_code == 1) { return; }

            if (moved_code == 2 || moved_code == 4 || moved_code == -1)
            {
                // on a collisionné fort
                Camera.main.GetComponent<CameraShaker>().shake(force.magnitude);
                // Debug.Log("shake : " + force.magnitude);
            }
            else if (moved_code == 3 || moved_code == 5)
            {
                // on a collisionné moins fort
                Camera.main.GetComponent<CameraShaker>().shake(force.magnitude / 2f);
                // Debug.Log("shake w glissement : " + force.magnitude);
            }
        }
    }



    // DEPLACEMENT
    protected int move(Vector2 movement)
    {
        // fonction mère de toutes les fonctions de déplacement :
        // applique le mouvement au movable en fonction des collisions

        if (movement == Vector2.zero) { return 0; }

        bool hit_wall_x = false;
        bool glissed_x = false;
        bool hit_wall_y = false;
        bool glissed_y = false;

        // codes de retour
        // 0 : pas de mouvement de base
        // 1 : mouvement effectué sans collision
        // 2 : collision sur x
        // 3 : collision sur x + glissement en x
        // 4 : collision sur y
        // 5 : collision sur y + glissement en y
        // -1 : collision sur x et y
        // -2 : collision sur x et y + glissement en x et y

        Physics2D.queriesHitTriggers = false;

        // on récupère le AABB des pieds
        Bounds feet_aabb = feet.bounds;

        // on sauvegarde le mouvement
        Vector2 movement_save = movement;

        // ******************************
        // * MOUVEMENT EN X
        // ******************************

        if (movement.x != 0f)
        {

            // on lance le raycast
            Vector2 raycast_direction = new Vector2((movement.x > 0f) ? 1f : -1f, 0f);
            RaycastHit2D hit = Physics2D.BoxCast(feet_aabb.center, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.x), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);

            // on prépare un mouvement subsidiare en y
            float subsidiar_movement_y = 0f;

            // on vérifie si le mouvement touche un collider
            if (hit.collider != null)
            {
                float hit_treshold = 0.05f;
                if (hit.distance < hit_treshold)
                {
                    // si on collisionne de trop proche,
                    // il y a plusieurs cas

                    // soit on se trouve contre un mur penché (et on peut se déplacer en y)

                    // on se trouve soit contre un mur (et on ne peut pas se déplacer en y)
                    // soit à la jonction de 2 murs potentiellement penchés (et on peut se déplacer en y)

                    // on envoie 2 raycasts en direction du mur sur x
                    // avec 2 boxs de y différents (un en haut et un en bas)

                    float y_offset = feet_aabb.size.y;


                    // 1er raycast
                    Vector2 center_hit_down = new Vector2(feet_aabb.center.x, feet_aabb.center.y - y_offset);
                    RaycastHit2D hit_down = Physics2D.BoxCast(center_hit_down, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.x), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);
                    
                    // 2eme raycast
                    Vector2 center_hit_up = new Vector2(feet_aabb.center.x, feet_aabb.center.y + y_offset);
                    RaycastHit2D hit_up = Physics2D.BoxCast(center_hit_up, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.x), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);

                    // on regarde si l'un des 2 raycast a touché un collider
                    // print("coll X - D : " + hit_down.normal + " / U : " + hit_up.normal);

                    // on regarde si sur l'axe y on peut se déplacer
                    if (hit_down.collider == null && movement.y <= 0f)
                    {
                        // on peut se déplacer vers le bas
                        subsidiar_movement_y = -y_offset/2f;
                        movement.x/=2f;
                    }
                    else if (hit_up.collider == null && movement.y >= 0f)
                    {
                        // on peut se déplacer vers le haut
                        subsidiar_movement_y = y_offset/2f;
                        movement.x /= 2f;
                    }

                    // on annule le mouvement en x
                    if (subsidiar_movement_y == 0f)
                    {
                        movement.x = 0f;
                    }

                }
                else
                {
                    // on ajuste le vecteur de déplacement
                    movement.x = hit.distance + hit.normal.x * hit_treshold;
                }
            }

            if (movement.x == 0f) { hit_wall_x = true; }
            if (subsidiar_movement_y != 0f) { glissed_x = true; }


            // on déplace le movable
            transform.position += new Vector3(movement.x, subsidiar_movement_y, 0f);

            // on maj notre collider_box
            /* feet_aabb.x += movement.x;
            feet_aabb.y += subsidiar_movement_y; */
            // pas besoin notre collider est un child de notre movable
        }

        // ******************************
        // * MOUVEMENT EN Y
        // ******************************

        if (movement.y != 0f)
        {

            // on lance le raycast
            Vector2 raycast_direction = new Vector2(0f, (movement.y > 0f) ? 1f : -1f);
            RaycastHit2D hit = Physics2D.BoxCast(feet_aabb.center, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.y), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);

            // on prépare un mouvement subsidiare en x
            float subsidiar_movement_x = 0f;

            // on vérifie si le mouvement touche un collider
            if (hit.collider != null)
            {
                float hit_treshold = 0.02f;
                if (hit.distance < hit_treshold)
                {
                    // si on collisionne de trop proche,
                    // il y a plusieurs cas

                    // soit on se trouve contre un mur penché (et on peut se déplacer en x)

                    // on se trouve soit contre un mur (et on ne peut pas se déplacer en x)
                    // soit à la jonction de 2 murs potentiellement penchés (et on peut se déplacer en x)

                    // on envoie 2 raycasts en direction du mur sur x
                    // avec 2 boxs de x différents (un à gauche et un à droite)

                    float x_offset = feet_aabb.size.x;


                    // 1er raycast
                    Vector2 center_hit_L = new Vector2(feet_aabb.center.x - x_offset, feet_aabb.center.y);
                    RaycastHit2D hit_L = Physics2D.BoxCast(center_hit_L, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.y), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);

                    // 2eme raycast
                    Vector2 center_hit_R = new Vector2(feet_aabb.center.x + x_offset, feet_aabb.center.y);
                    RaycastHit2D hit_R = Physics2D.BoxCast(center_hit_R, feet_aabb.size, 0f, raycast_direction, Mathf.Abs(movement.y), HasEffect(Effect.Ghost) ? ghost_layers : world_layers);

                    // on regarde si l'un des 2 raycast a touché un collider
                    // print("coll Y - L : " + hit_L.normal + " / R : " + hit_R.normal);

                    // on regarde si sur l'axe y on peut se déplacer
                    if (hit_L.collider == null && movement_save.x <= 0f)
                    {
                        // on peut se déplacer vers le bas
                        subsidiar_movement_x = -x_offset / 8f;
                        movement.y /= 8f;
                    }
                    else if (hit_R.collider == null && movement_save.x >= 0f)
                    {
                        // on peut se déplacer vers le haut
                        subsidiar_movement_x = x_offset / 8f;
                        movement.y /= 8f;
                    }

                    // on annule le mouvement en x
                    if (subsidiar_movement_x == 0f)
                    {
                        movement.y = 0f;
                    }

                }
                else
                {
                    // on ajuste le vecteur de déplacement
                    movement.y = hit.distance + hit.normal.y * hit_treshold;
                }
            }

            if (movement.y == 0f) { hit_wall_y = true; }
            if (subsidiar_movement_x != 0f) { glissed_y = true; }

            // on déplace le movable
            transform.position += new Vector3(subsidiar_movement_x, movement.y, 0f);
        }

        // on regarde si on a collisionné
        if (hit_wall_x && hit_wall_y)
        {
            if (glissed_x && glissed_y) { return -2; }
            return -1;
        }
        else if (hit_wall_x)
        {
            if (glissed_x) { return 3; }
            return 2;
        }
        else if (hit_wall_y)
        {
            if (glissed_y) { return 5; }
            return 4;
        }
        return 1;

    }
    public void MOVE(Vector3 position)
    {
        // on FORCE le déplacement du movable
        // ! à utiliser seulement pour des animations
        // ! ne prend pas en compte les collisions
        // ! -> obliger le Being à être en knock out pour utiliser cette fonction

        if (!HasEffect(Effect.Stunned)) { return; }

        // on déplace le movable
        // transform.position += new Vector3(movement.x, movement.y, 0f);
        transform.position = position;
    }


    // GETTERS
    public float getHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }

    public Force getForce(int id)
    {
        return forces.Find(f => f.id == id);
    }

    public List<Force> getForces(List<int> ids)
    {
        return forces.FindAll(f => ids.Contains(f.id));
    }

    // gizmos
    protected virtual void OnDrawGizmos()
    {
        // on dessine le collider des pieds
        if (feet == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(feet.bounds.center, feet.bounds.size);
    }

}


public class Force
{
    public static int id_counter = 0;
    public int id;
    public Vector2 direction;
    public float magnitude;
    public float attenuation;

    public static Force Zero = new Force(Vector2.zero, 0f, 1f);

    public Force(Vector2 direction, float magnitude, float attenuation=1f)
    {
        this.direction = direction;
        this.magnitude = magnitude;
        this.attenuation = attenuation;

        // on génère un id unique
        this.id = id_counter;
        id_counter++;
    }

}