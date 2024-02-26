using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(AnimationHandler))]
[RequireComponent(typeof(AudioManager))]
public class Being : MonoBehaviour
{


    [Header("BEING")]
    // a BEING is a character that can move, attack, etc.

    // VIE
    public float vie = 100f;
    public int max_vie = 100;
    public float regen_vie = 0f; // en point de vie par seconde
    public float weight = 1f; // poids du perso (pour le knockback)

    // HURTED
    // private float last_hurted_time = 0f; // temps de la dernière attaque subie
    public GameObject xp_provider;
    public GameObject floating_dmg_provider;
    protected int xp_gift = 10;

    // FORCES
    public List<Force> forces = new List<Force>();

    // DEPLACEMENT
    public Vector2 inputs;
    public float inputs_magnitude=1f;
    public float speed = 3f; // speed de déplacement
    public float running_speed = 5f; // speed de déplacement
    protected bool isRunning = false;
    private Rect feet_collider; // collider des pieds (pour les collisions)
    private float offset_perso_y_to_feet = 0f; // offset entre le perso et le collider des pieds
    public LayerMask world_layers; // layers du monde
    private bool isMoving = false;

    // CURRENT VELOCITY
    private Vector3 last_position = Vector3.zero;
    public Vector2 velocity = Vector2.zero;

    // ANIMATIONS
    protected AnimationHandler anim_handler;
    protected Vector2 lookin_at = new Vector2(0f, -1f);
    protected float lookin_at_angle = 40f; // angle du regard du perso en degrés
    protected BeingAnims anims = new BeingAnims();

    // SOUND
    protected AudioManager audio_manager;
    protected BeingSounds sounds = new BeingSounds();

    // CAPACITES
    protected Dictionary<string, bool> capacities = new Dictionary<string, bool>();
    protected Dictionary<string, float> capacities_cooldowns = new Dictionary<string, float>();
    protected Dictionary<string, float> capacities_cooldowns_base = new Dictionary<string, float>();

    // unity functions
    protected void Start()
    {

        // on initialise les capacités
        capacities.Add("life_regen", true);
        capacities.Add("walk", true);

        // on récupère les composants
        // gameObject.AddComponent<AnimationHandler>();
        anim_handler = GetComponent<AnimationHandler>();

        // on récup le audio manager
        audio_manager = GetComponent<AudioManager>();
        

        // on crée notre collider_box des pieds
        feet_collider = Rect.zero;
        feet_collider.size = new Vector2(0.4f, 0.1f);
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
        feet_collider.center = new Vector2(transform.position.x, y_center);

        inputs = new Vector2(0.1f,0f);

        // on défini les layers du monde
        world_layers = LayerMask.GetMask("Ground","Walls","Ceiling","Doors","Chests","Computers","Buttons","Decoratives", "Interactives");

        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/particles/xp_provider");

        // on récupère le provider de floating dmg
        floating_dmg_provider = GameObject.Find("/utils/dmgs_provider");
    }

    public virtual void Events()
    {

        // on vérifie qu'on est pas KO
        if (hasCapacity("knocked_out")) { return; }

        // life regen
        if (hasCapacity("life_regen"))
        {
            if (vie < max_vie)
            {
                vie += regen_vie * Time.deltaTime;
            }
        }

        // walk
        if (hasCapacity("walk") && !(this is Perso))
        {
            inputs = randomly_circulate(inputs);
        }
        
    }

    protected virtual void Update()
    {
        // on vérifie si le perso est mort
        if (!isAlive()) { return; }

        // on sauvegarde la position du perso
        last_position = transform.position;

        // update capacities cooldowns
        UpdateCapacities();

        // on récupère les inputs
        Events();

        // on calcule la direction du regard
        calculate_lookin_at(inputs);

        // maj des animations
        maj_animations(inputs);

        if (!hasCapacity("knocked_out"))
        {
            // déplacement
            if (isRunning)
            {
                run(inputs, inputs_magnitude);
            }
            else
            {
                walk(inputs, inputs_magnitude);
            }

            // update forces
            update_forces();
        }

        // on calcule la current velocity
        Vector3 vel = (transform.position - last_position) / Time.deltaTime;
        velocity = new Vector2(vel.x, vel.y);
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

    protected void startCapacityCooldown(string capacity)
    {
        if (!capacities_cooldowns_base.ContainsKey(capacity)) { return; }

        // on met le cooldown à jour
        capacities_cooldowns[capacity] = capacities_cooldowns_base[capacity];
    }

    protected void beInvicible(float duration = 0.5f)
    {
        // capacities["invicible"] = true;
        addCapacity("invicible");
        Invoke("stopInvicibility", duration);
    }
    protected void stopInvicibility()
    {
        // capacities["invicible"] = false;
        // capacities.Remove("invicible");
        removeCapacity("invicible");
    }

    // DEPLACEMENT
    protected void walk(Vector2 direction, float inputs_magnitude=1f)
    {

        // on calcule le mouvement sur X
        float x_movement = direction.normalized.x * speed * Time.deltaTime * inputs_magnitude;

        // on calcule le mouvement sur Y
        float y_movement = direction.normalized.y * speed * Time.deltaTime * inputs_magnitude;

        // on applique le mouvement au perso
        move_perso(new Vector2(x_movement, y_movement));

    }
    protected void run(Vector2 direction, float inputs_magnitude=1f)
    {

        // on calcule le mouvement sur X
        float x_movement = direction.normalized.x * running_speed * Time.deltaTime * inputs_magnitude;

        // on calcule le mouvement sur Y
        float y_movement = direction.normalized.y * running_speed * Time.deltaTime * inputs_magnitude;

        // on applique le mouvement au perso
        move_perso(new Vector2(x_movement, y_movement));

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
        // on déplace le perso
        int moved_code = move_perso(force);
        

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
                Camera.main.GetComponent<CameraShaker>().shake(force.magnitude/2f);
                // Debug.Log("shake w glissement : " + force.magnitude);
            }
        }
    }

    protected int move_perso(Vector2 movement)
    {
        // fonction mère de toutes les fonctions de déplacement :
        // applique le mouvement au perso en fonction des collisions

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


        // on maj notre collider_box        
        float y_center = transform.position.y - offset_perso_y_to_feet + feet_collider.size.y / 2f;
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
            RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), world_layers);

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
                    RaycastHit2D hit_down = Physics2D.BoxCast(center_hit_down, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), world_layers);
                    
                    // 2eme raycast
                    Vector2 center_hit_up = new Vector2(feet_collider.center.x, feet_collider.center.y + y_offset);
                    RaycastHit2D hit_up = Physics2D.BoxCast(center_hit_up, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.x), world_layers);

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


            // on déplace le perso
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
            RaycastHit2D hit = Physics2D.BoxCast(feet_collider.center, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), world_layers);

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
                    RaycastHit2D hit_L = Physics2D.BoxCast(center_hit_L, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), world_layers);

                    // 2eme raycast
                    Vector2 center_hit_R = new Vector2(feet_collider.center.x + x_offset, feet_collider.center.y);
                    RaycastHit2D hit_R = Physics2D.BoxCast(center_hit_R, feet_collider.size, 0f, raycast_direction, Mathf.Abs(movement.y), world_layers);

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

            // on déplace le perso
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
        // on FORCE le déplacement du perso
        // ! à utiliser seulement pour des animations
        // ! ne prend pas en compte les collisions
        // ! -> obliger le Being à être en knock out pour utiliser cette fonction

        if (!hasCapacity("knocked_out")) { return; }

        // on déplace le perso
        // transform.position += new Vector3(movement.x, movement.y, 0f);
        transform.position = position;
    }

    // INPUTS SIMULATION
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

    protected Vector2 randomly_circulate(Vector2 input_vecteur)
    {
        // simulate circular input on x
        // et change de direction sur y de temps en temps

        // circular input
        Vector2 new_input_vecteur = simulate_circular_input_on_x(input_vecteur);

        // change direction on y
        if (Random.Range(0f, 1f) < 0.01f)
        {
            new_input_vecteur.y = Random.Range(-1f, 1f);
        }

        return new_input_vecteur;

    }


    // ANIMATIONS
    protected void maj_animations(Vector2 direction)
    {

        // animation de déplacement (en fonction des inputs)
        var input_tresh = 0.1f;
        isMoving = (direction.magnitude > input_tresh);

        // animation de direction (en fonction du regard)
        if (anims.has_up_down_runnin)
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
                    anim_handler.ChangeAnim( anims.has_up_down_idle ? anims.idle_down : anims.idle_side);
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
                    anim_handler.ChangeAnim( anims.has_up_down_idle ? anims.idle_up : anims.idle_side);
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


            // sens du sprite (en fonction du regard)
            // on change seulement si on ne subit pas de knockback
            if (anim_handler.GetCurrentAnimName() != anims.hurted)
            {
                if (Mathf.Abs(lookin_at.x) >= Mathf.Abs(lookin_at.y)) { GetComponent<SpriteRenderer>().flipX = (lookin_at.x > 0f); }
            }
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

            if (anim_handler.GetCurrentAnimName() != anims.hurted)
            {
                GetComponent<SpriteRenderer>().flipX = (lookin_at.x > 0f);
            }
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
    public bool take_damage(float damage, Force knockback=null)
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return false; }

        // on vérifie si on est invincible
        if (hasCapacity("invicible"))
        {
            // floating missing text
            // floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject, 0, transform.position);
            floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddMissed(this.gameObject, transform.position);
            return false;
        }

        // si on est ici on prend des dégats
        vie -= damage;

        // play hurt animation
        anim_handler.ChangeAnimTilEnd(anims.hurted);

        // play hurt sound
        audio_manager.Play(sounds.hurted());


        // knockback
        // apply_force(knockback);
        if (knockback != null)
        {
            forces.Add(knockback);

            // change the flipX of the sprite if needed
            if (knockback.direction.x != 0f)
            {
                GetComponent<SpriteRenderer>().flipX = (knockback.direction.x < 0f);
            }
        }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject,-1f * damage, transform.position);

        // check if dead
        if (vie <= 0f)
        {
            die();
        }

        // Debug.Log(gameObject.name + " took " + damage + " dmg and has " + vie + " hp left");
        return true;
    }

    protected virtual void die()
    {
        // play death animation
        anim_handler.ForcedChangeAnim(anims.die);

        // on donne de l'xp
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + GetComponent<SpriteRenderer>().bounds.size.y / 2f,0);
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_gift, sprite_center);

        // destroy object
        Invoke("DestroyObject", 60f);

        // on change le layer du perso en "meat"
        gameObject.layer = LayerMask.NameToLayer("Meat");
    }

    protected virtual void comeback_from_death()
    {
        CancelInvoke("DestroyObject");
        
        // on remet la vie au max
        vie = max_vie;

        // on remet l'animation de base
        anim_handler.StopForcing();
        anim_handler.ChangeAnim(anims.idle_side);
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

    public float getHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }

    public bool hasCapacity(string capacity)
    {
        return capacities.ContainsKey(capacity) && capacities[capacity];
    }

    // SETTERS
    public void addLife(float life)
    {
        vie += life;
        if (vie > max_vie) { vie = max_vie; }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject, life, transform.position);
    }

    public void addBonusLife(float bonus_life)
    {
        max_vie += (int) bonus_life;
        vie += bonus_life;
    }

    public void heal(int nb_heal=2)
    {
        // each heal gives 10% of max life
        float heal = max_vie * 0.1f * nb_heal;
        addLife(heal);
    }

    public void healMax()
    {
        // restore max life
        addLife(max_vie-vie);
    }

}

public class BeingAnims
{
    public bool has_up_down_runnin = false;
    public bool has_up_down_idle = false;

    // ANIMATIONS
    public string idle_down = "idle_D";
    public string idle_up = "idle_U";
    public string idle_side = "idle_RL";
    public string run_down = "runnin_D";
    public string run_up = "runnin_U";
    public string run_side = "runnin_RL";
    public string die = "die";
    public string hurted = "hurted_RL";

    public virtual void init(string name)
    {
        idle_down = name + "_" + idle_down;
        idle_up = name + "_" + idle_up;
        idle_side = name + "_" + idle_side;
        run_down = name + "_" + run_down;
        run_up = name + "_" + run_up;
        run_side = name + "_" + run_side;
        die = name + "_" + die;
        hurted = name + "_" + hurted;
    }
}

public class BeingSounds
{
    public List<string> s_hurted = new List<string>() {"hurted - 1"};
    public List<string> s_die = new List<string>() {"die - 1"};
    public List<string> s_walk = new List<string>() {"walk - 1"};
    public List<string> s_run = new List<string>() { "run - 1" };
    public List<string> s_idle = new List<string>() { "idle - 1" };

    public virtual void init(string name)
    {
        s_hurted = s_hurted.Select(s => name + "_" + s).ToList();
        s_die = s_die.Select(s => name + "_" + s).ToList();
        s_walk = s_walk.Select(s => name + "_" + s).ToList();
        s_run = s_run.Select(s => name + "_" + s).ToList();
        s_idle = s_idle.Select(s => name + "_" + s).ToList();
    }


    // GETTERS
    public string hurted()
    {
        return s_hurted[Random.Range(0, s_hurted.Count)];
    }

    public string die()
    {
        return s_die[Random.Range(0, s_die.Count)];
    }

    public string walk()
    {
        return s_walk[Random.Range(0, s_walk.Count)];
    }

    public string run()
    {
        return s_run[Random.Range(0, s_run.Count)];
    }

    public string idle()
    {
        return s_idle[Random.Range(0, s_idle.Count)];
    }

}

public class Force
{
    public Vector2 direction;
    public float magnitude;
    public float attenuation;

    public static Force Zero = new Force(Vector2.zero, 0f, 1f);

    public Force(Vector2 direction, float magnitude, float attenuation=1f)
    {
        this.direction = direction;
        this.magnitude = magnitude;
        this.attenuation = attenuation;
    }

}