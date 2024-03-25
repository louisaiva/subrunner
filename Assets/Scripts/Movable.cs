using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(AudioManager))]
public class Movable : MonoBehaviour
{


    [Header("MOVABLE")]
    // a MOVABLE is a character that can ONLY move with FORCES. that's it.

    public float weight = 1f; // poids du movable (pour le knockback)

    // FORCES
    public List<Force> forces = new List<Force>();

    // DEPLACEMENT
    private Rect feet_collider; // collider des pieds (pour les collisions)
    private float offset_movable_y_to_feet = 0f; // offset entre le movable et le collider des pieds
    public LayerMask world_layers; // layers du monde
    public LayerMask ghost_layers; // layers du monde (ghost)
    
    // COLLISIONS
    // protected List<Collider2D> ghosting_colliders = new List<Collider2D>(); // colliders qui ne sont pas pris en compte dans les collisions


    // CURRENT VELOCITY
    protected Vector3 last_position = Vector3.zero;
    public Vector2 velocity = Vector2.zero;

    // SOUND
    protected AudioManager audio_manager;
    protected BeingSounds sounds = new BeingSounds();

    // CAPACITES
    protected Dictionary<string, bool> capacities = new Dictionary<string, bool>();
    protected Dictionary<string, float> capacities_cooldowns = new Dictionary<string, float>();
    protected Dictionary<string, float> capacities_cooldowns_base = new Dictionary<string, float>();
    protected Dictionary<string, float> capacities_ttl = new Dictionary<string, float>();

    // unity functions
    protected virtual void Start()
    {

        // on récup le audio manager
        audio_manager = GetComponent<AudioManager>();
        

        // on crée notre collider_box des pieds
        feet_collider = Rect.zero;
        feet_collider.size = new Vector2(0.4f, 0.1f);
        float y_center = transform.position.y - offset_movable_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        // on défini les layers du monde
        world_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling", "Doors", "Chests", "Computers", "Decoratives", "Interactives");
        ghost_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling");
    }


    protected virtual void Update()
    {
        // on sauvegarde la position du movable
        last_position = transform.position;

        // update capacities cooldowns
        UpdateCapacities();
        
        // update forces
        update_forces();

        // on calcule la current velocity
        Vector3 vel = (transform.position - last_position) / Time.deltaTime;
        velocity = new Vector2(vel.x, vel.y);
    }


    // CAPACITES
    protected virtual void UpdateCapacities()
    {
        // update capacities cooldowns
        for (int i = 0; i < capacities_cooldowns.Count; i++)
        {
            KeyValuePair<string, float> entry = capacities_cooldowns.ElementAt(i);
            if (entry.Value > 0f)
            {
                // on désactive la capacité
                if (capacities[entry.Key] == true)
                {
                    capacities[entry.Key] = false;
                }

                // on diminue le cooldown
                capacities_cooldowns[entry.Key] -= Time.deltaTime;

                // on remet la capacité si le cooldown est fini
                if (capacities_cooldowns[entry.Key] < 0f)
                {
                    capacities_cooldowns[entry.Key] = 0f;
                    capacities[entry.Key] = true;
                }
            }
        }

        // update capacities ttl
        for (int i = 0; i < capacities_ttl.Count; i++)
        {
            KeyValuePair<string, float> entry = capacities_ttl.ElementAt(i);
            if (entry.Value > 0f)
            {
                // on diminue le ttl
                capacities_ttl[entry.Key] -= Time.deltaTime;

                // on enlève la capacité si le ttl est fini
                if (capacities_ttl[entry.Key] < 0f)
                {
                    removeCapacity(entry.Key);
                    capacities_ttl.Remove(entry.Key);
                }
            }
        }
    }

    public void removeCapacity(string capacity)
    {
        if (!capacities.ContainsKey(capacity)) { return; }
        
        // on enlève la capacité
        capacities.Remove(capacity);

        if (capacities_cooldowns.ContainsKey(capacity))
        {
            // on enlève le cooldown
            capacities_cooldowns.Remove(capacity);
        }

        if (capacities_cooldowns_base.ContainsKey(capacity))
        {
            // on enlève le cooldown de base
            capacities_cooldowns_base.Remove(capacity);
        }
    }

    public void addCapacity(string capacity, float cooldown=0f)
    {
        if (!capacities.ContainsKey(capacity))
        {
            // on ajoute la capacité
            capacities.Add(capacity, true);
        }
        else
        {
            // on active la capacité
            capacities[capacity] = true;
        }

        if (cooldown == 0f) { return; }


        // on ajoute les cooldowns
        if (!capacities_cooldowns.ContainsKey(capacity))
        {
            capacities_cooldowns.Add(capacity, 0f);
        }
        if (!capacities_cooldowns_base.ContainsKey(capacity))
        {
            capacities_cooldowns_base.Add(capacity, cooldown);
        }
        else
        {
            capacities_cooldowns_base[capacity] = cooldown;
        }
    }

    public void addCapacityCooldown(string capacity, float cooldown)
    {
        // on ajoute un cooldown à une capacité
        if (!capacities_cooldowns_base.ContainsKey(capacity)) { return; }

        // on met le cooldown à jour
        capacities_cooldowns[capacity] = cooldown;
    }

    public void addEphemeralCapacity(string capacity, float duration, float cooldown=0f)
    {
        // on ajoute une capacité éphémère
        addCapacity(capacity, cooldown);
        capacities_ttl[capacity] = duration;
    }

    protected void startCapacityCooldown(string capacity)
    {
        if (!capacities_cooldowns_base.ContainsKey(capacity)) { return; }

        // on met le cooldown à jour
        capacities_cooldowns[capacity] = capacities_cooldowns_base[capacity];
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

    protected void update_forces()
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
        int moved_code = movable(force);


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
    protected int movable(Vector2 movement)
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

        // on maj notre collider_box        
        float y_center = transform.position.y - offset_movable_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        // on sauvegarde le mouvement
        Vector2 movement_save = movement;

        // ******************************
        // * MOUVEMENT EN X
        // ******************************

        if (movement.x != 0f)
        {

            // on lance le raycast
            Vector2 raycast_direction = new Vector2((movement.x > 0f) ? 1f : -1f, 0f);
            RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), hasCapacity("ghost") ? ghost_layers : world_layers);

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

                    float y_offset = feet_collider.size.y;


                    // 1er raycast
                    Vector2 center_hit_down = new Vector2(feet_collider.center.x, feet_collider.center.y - y_offset);
                    RaycastHit2D hit_down = Physics2D.BoxCast(center_hit_down, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), hasCapacity("ghost") ? ghost_layers : world_layers);
                    
                    // 2eme raycast
                    Vector2 center_hit_up = new Vector2(feet_collider.center.x, feet_collider.center.y + y_offset);
                    RaycastHit2D hit_up = Physics2D.BoxCast(center_hit_up, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), hasCapacity("ghost") ? ghost_layers : world_layers);

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
            feet_collider.x += movement.x;
            feet_collider.y += subsidiar_movement_y;
        }

        // ******************************
        // * MOUVEMENT EN Y
        // ******************************

        if (movement.y != 0f)
        {

            // on lance le raycast
            Vector2 raycast_direction = new Vector2(0f, (movement.y > 0f) ? 1f : -1f);
            RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), hasCapacity("ghost") ? ghost_layers : world_layers);

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

                    float x_offset = feet_collider.size.x;


                    // 1er raycast
                    Vector2 center_hit_L = new Vector2(feet_collider.center.x - x_offset, feet_collider.center.y);
                    RaycastHit2D hit_L = Physics2D.BoxCast(center_hit_L, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), hasCapacity("ghost") ? ghost_layers : world_layers);

                    // 2eme raycast
                    Vector2 center_hit_R = new Vector2(feet_collider.center.x + x_offset, feet_collider.center.y);
                    RaycastHit2D hit_R = Physics2D.BoxCast(center_hit_R, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), hasCapacity("ghost") ? ghost_layers : world_layers);

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

        if (!hasCapacity("knocked_out")) { return; }

        // on déplace le movable
        // transform.position += new Vector3(movement.x, movement.y, 0f);
        transform.position = position;
    }


    // GETTERS
    public float getHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }

    public bool hasCapacity(string capacity)
    {
        return capacities.ContainsKey(capacity) && capacities[capacity];
    }

    public Force getForce(int id)
    {
        return forces.Find(f => f.id == id);
    }

    public List<Force> getForces(List<int> ids)
    {
        return forces.FindAll(f => ids.Contains(f.id));
    }

    public List<string> getCapacities()
    {
        return capacities.Keys.ToList();
    }
    public Dictionary<string, float> getCooldownsBase()
    {
        return capacities_cooldowns_base;
    }



    // PRINTERS
    public void showCapacities()
    {
        string capacities_str = "(Being - "+gameObject.name + ") capacities : \n";
        foreach (KeyValuePair<string, bool> entry in capacities)
        {
            capacities_str += entry.Key + " : " + entry.Value + "\n";
        }
        Debug.Log(capacities_str);
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