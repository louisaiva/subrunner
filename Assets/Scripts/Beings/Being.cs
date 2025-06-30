using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;


public class Being : Movable
{

    [Header("LIFE")]
    public float life = 100f;
    public int max_life = 100;

    public bool Alive { get { return life > 0f; } }
    public float regen_life = 0f; // en point de life par seconde
    public Collider2D life_collider;


    // [Header("MOVEMENT")]
    // public float inputs_magnitude=1f;
    // public float speed = 3f; // speed de déplacement
    // protected bool isRunning = false;
    // public float running_speed = 5f; // speed de déplacement
    // private bool isMoving = false;

    [Header("taking damage")]
    // public GameObject xp_provider;
    public GameObject floating_dmg_provider;

    // ANIMATIONS
    protected float lookin_at_angle = 40f; // angle du regard du perso en degrés


    // START & AWAKE
    protected override void Awake()
    {
        base.Awake();

        // on récupère les composants
        life_collider = transform.Find("body").GetComponent<Collider2D>();

        // on récupère le provider de floating dmg
        floating_dmg_provider = GameObject.Find("/utils/dmgs_provider");
    }
    protected virtual void Start()
    {
        // on initialise les capacités
        AddCapacity("hurted");
        AddEffect(Effect.RegenLife, -888f);
    }



    // UPDATE HIGH LEVEL
    protected override void Update()
    {
        // on vérifie si le perso est mort
        if (!Alive)
        {
            inputs = Vector2.zero;
            base.Update();
            return;
        }

        // on update les behaviour
        UpdateGOAP();

        // on récupère les inputs
        UpdateBeingEffects();

        base.Update();
    }
    protected virtual void UpdateGOAP() { }
    public virtual void UpdateBeingEffects()
    {
        // life regen
        if (HasEffect(Effect.RegenLife) && life < max_life)
        {
            life += regen_life * Time.deltaTime;
        }

        // Invisible
        if (HasEffect(Effect.Invisible))
        {
            // change the body collider to Ghosts layer
            life_collider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else
        {
            // reset the body collider to Beings layer
            life_collider.gameObject.layer = LayerMask.NameToLayer("Beings");
        }
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
        if (UnityEngine.Random.Range(0f, 1f) < 0.01f)
        {
            new_input_vecteur.y = UnityEngine.Random.Range(-1f, 1f);
        }

        return new_input_vecteur;

    }


    // DAMAGE
    public virtual bool take_damage(float damage, Force knockback=null)
    {
        // ! à mettre tjrs au début de la fonction update
        if (!Alive) { return false; }

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
        Do("hurted");


        // knockback
        if (knockback != null)
        {
            // forces.Add(knockback);
            AddForce(knockback);

            // change the flipX of the sprite if needed
            if (knockback.direction.x != 0f)
            {
                GetComponent<SpriteRenderer>().flipX = (knockback.direction.x < 0f);
            }
        }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(this.gameObject,-1f * damage, transform.position);

        // check if dead
        if (life <= 0f && Can("die"))
        {
            Do("die");
            if (this is Perso) { ((Perso) this).Die(); }
        }

        return true;
    }
    protected virtual void comeback_from_death()
    {
        // on remet la life au max
        life = max_life;

        // on remet l'animation de base
        anim_player.StopPlaying("die");

        // on remet le layer à "default"
        life_collider.gameObject.layer = LayerMask.NameToLayer("Beings");
        feet_collider.isTrigger = false;
    }

    
    // SETTERS
    public void AddLife(float life)
    {
        this.life += life;
        if (this.life > max_life)
        { 
            // floating dmg
            floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(gameObject, max_life - this.life, transform.position);

            this.life = max_life;
        }

        // floating dmg
        floating_dmg_provider.GetComponent<FloatingDmgProvider>().AddFloatingDmg(gameObject, life, transform.position);
    }
    public void heal(int nb_heal=2)
    {
        // each heal gives 10% of max life
        float heal = max_life * 0.1f * nb_heal;
        AddLife(heal);
    }
    public void healMax()
    {
        // restore max life
        AddLife(max_life-life);
    }



    // gizmos
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // calculate lookin_at vector base position
        Vector3 lookin_at_pos = transform.position + new Vector3(Orientation.x, Orientation.y, 0f);

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