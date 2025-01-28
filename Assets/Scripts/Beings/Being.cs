using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Being : Movable
{


    [Header("BEING")]
    // a BEING is a character that can move, die.

    // VIE
    public float life = 100f;
    public int max_life = 100;
    public float regen_life = 0f; // en point de life par seconde
    public Collider2D life_collider;


    // HURTED
    // private float last_hurted_time = 0f; // temps de la dernière attaque subie
    public GameObject xp_provider;
    public GameObject floating_dmg_provider;
    public int xp_gift = 10;

    // DEPLACEMENT
    public Vector2 inputs;
    public float inputs_magnitude=1f;
    public float speed = 3f; // speed de déplacement
    public float running_speed = 5f; // speed de déplacement
    protected bool isRunning = false;
    // private bool isMoving = false;


    // ANIMATIONS
    // protected AnimationHandler anim_handler;
    protected Vector2 lookin_at = new Vector2(0f, -1f);
    protected float lookin_at_angle = 40f; // angle du regard du perso en degrés
    protected BeingAnims anims = new();


    // unity functions
    protected override void Start()
    {

        base.Start();
        
        // on initialise les capacités
        AddCapacity("hurted");
        AddCapacity("die");
        AddEffect(Effect.RegenLife, -888f); 


        // on récupère les composants
        life_collider = transform.Find("body").GetComponent<Collider2D>();
        // gameObject.AddComponent<AnimationHandler>();
        // anim_handler = GetComponent<AnimationHandler>();


        inputs = new Vector2(0.1f,0f);


        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/utils/particles/xp_provider");

        // on récupère le provider de floating dmg
        floating_dmg_provider = GameObject.Find("/utils/dmgs_provider");

    }

    public virtual void Events()
    {


        // on vérifie qu'on est pas KO
        if (HasEffect(Effect.Stunned)) { return; }

        // life regen
        if (HasEffect(Effect.RegenLife))
        {
            if (life < max_life)
            {
                life += regen_life * Time.deltaTime;
            }
        }

        // walk
        if (Can("walk") && !(this is Perso))
        {
            inputs = randomly_circulate(inputs);
        }
        
    }

    protected override void Update()
    {
        base.Update();

        // on vérifie si le perso est mort
        if (!isAlive()) { return; }

        // on sauvegarde la position du perso
        last_position = transform.position;

        // on récupère les inputs
        Events();

        // on calcule la direction du regard
        calculate_lookin_at(inputs);
        anim_player.SetOrientation(lookin_at);

        // maj des animations
        // maj_animations(inputs);

        if (!HasEffect(Effect.Stunned) && inputs.magnitude > 0.1f)
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
        }
        else
        {
            anim_player.StopPlaying("run");
            anim_player.StopPlaying("walk");
        }

        // update forces
        updateForces();

        // on calcule la current velocity
        Vector3 vel = (transform.position - last_position) / Time.deltaTime;
        velocity = new Vector2(vel.x, vel.y);
    }


    // DEPLACEMENT
    protected void walk(Vector2 direction, float inputs_magnitude=1f)
    {
        // on calcule le mouvement sur X
        float x_movement = direction.normalized.x * speed * Time.deltaTime * inputs_magnitude;

        // on calcule le mouvement sur Y
        float y_movement = direction.normalized.y * speed * Time.deltaTime * inputs_magnitude;

        // on applique le mouvement au perso
        move(new Vector2(x_movement, y_movement));
        Do("walk");
    }
    protected void run(Vector2 direction, float inputs_magnitude=1f)
    {
        // on calcule le mouvement sur X
        float x_movement = direction.normalized.x * running_speed * Time.deltaTime * inputs_magnitude;

        // on calcule le mouvement sur Y
        float y_movement = direction.normalized.y * running_speed * Time.deltaTime * inputs_magnitude;

        // on applique le mouvement au perso
        move(new Vector2(x_movement, y_movement));
        Do("run");
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
    /* protected void maj_animations(Vector2 direction)
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

    } */

    protected void calculate_lookin_at(Vector2 direction)
    {
        // on calcule la direction du regard
        if (direction != Vector2.zero)
        {
            lookin_at = direction.normalized;
        }
    }


    // DAMAGE
    public virtual bool take_damage(float damage, Force knockback=null)
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return false; }

        // on vérifie si on est invincible
        if (HasEffect(Effect.Invincible))
        {
            // floating missing text
            // floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject, 0, transform.position);
            floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddMissed(this.gameObject, transform.position);
            return false;
        }

        // si on est ici on prend des dégats
        life -= damage;

        // play hurt animation
        // anim_handler.ChangeAnimTilEnd(anims.hurted);
        Do("hurted");


        // knockback
        // apply_force(knockback);
        if (knockback != null)
        {
            // forces.Add(knockback);
            addForce(knockback);

            // change the flipX of the sprite if needed
            if (knockback.direction.x != 0f)
            {
                GetComponent<SpriteRenderer>().flipX = (knockback.direction.x < 0f);
            }
        }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject,-1f * damage, transform.position);

        // check if dead
        if (life <= 0f)
        {
            die();
        }


        // Debug.Log(gameObject.name + " took " + damage + " dmg and has " + life + " hp left");
        return true;
    }

    protected virtual void die()
    {
        // play death animation
        // anim_handler.ForcedChangeAnim(anims.die);
        Do("die");

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
        
        // on remet la life au max
        life = max_life;

        // on remet l'animation de base
        // anim_handler.StopForcing();
        // anim_handler.ChangeAnim(anims.idle_side);
        anim_player.StopPlaying("die");
    }


    // GETTERS
    public bool isAlive()
    {
        return life > 0f;
    }

    
    // SETTERS
    public void addLife(float life)
    {
        life += life;
        if (life > max_life) { life = max_life; }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject, life, transform.position);
    }

    public void addBonusLife(float bonus_life)
    {
        max_life += (int) bonus_life;
        life += bonus_life;
    }

    public void heal(int nb_heal=2)
    {
        // each heal gives 10% of max life
        float heal = max_life * 0.1f * nb_heal;
        addLife(heal);
    }

    public void healMax()
    {
        // restore max life
        addLife(max_life-life);
    }



    // gizmos
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

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
        
        // on dessine le Collider de life du Being
        if (!life_collider) { return; }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(life_collider.bounds.center, life_collider.bounds.size);
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
