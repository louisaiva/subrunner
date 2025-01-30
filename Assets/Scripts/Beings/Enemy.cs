using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Being
{

    [Header("Target")]
    private float player_detection_radius = 3f;
    public bool target_detected = false;
    GameObject target;
    public LayerMask target_layers;


    [Header("ATTACKER")]
    public float attack_range = 0.25f; // distance entre le point d'attaque et le being qui attaque

    // unity functions
    protected virtual Collider2D[] get_targets_colliders(float radius)
    {
        // on récupère tous les ennemis dans le rayon de détection
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, target_layers);

        // on s'enleve soi même
        targets = targets.Where(target => target.transform.parent.gameObject != gameObject).ToArray();

        // on enlève les beings décédés
        targets = targets.Where(target => target.transform.parent.GetComponent<Being>().Alive).ToArray();

        return targets;

    }
    protected void detectTarget(float radius)
    {
        Collider2D[] targets = get_targets_colliders(radius);

        // on regarde si on a trouvé un ennemi
        target_detected = targets.Length > 0;
        if (target_detected)
        {
            target = targets[0].transform.parent.gameObject;
        }
        else
        {
            target = null;
        }
    }

    protected void try_to_attack_target()
    {
        // on recupère la distance entre le zombo et le joueur
        float distance = Vector2.Distance(transform.position, target.transform.position);

        // on regarde si la target est dans le cercle d'attaque
        if (distance < attack_range)
        {
            // on attaque
            if (Can("attack")) { Do("attack"); }
        }
    }

    public override void Events()
    {
        // treshold distance "trop proche"
        float treshold_distance = 0.1f;

        // 1 - on essaye de détecter le joueur
        if (target_detected)
        {
            // on se dirige vers le joueur
            Orientation = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);

            // on regarde si on est pas TROP proche du joueur
            if (Orientation.magnitude < treshold_distance)
            {
                Orientation = new Vector2(0, 0);
            }

            // on normalise les Orientation
            Orientation.Normalize();

            return;
        }

        // 2 - on bouge pas
        Orientation = new Vector2(0, 0);
    }
    protected override void Update()
    {
        // on essaye de détecter le joueur
        detectTarget(player_detection_radius);

        // update de d'habitude
        base.Update();

        // on essaye d'attaquer le joueur si on le détecte
        if (target_detected && Can("attack"))
        {
            // Debug.Log("target detected" + target.name);
            if (target.GetComponent<Being>().Alive)
            {
                try_to_attack_target();
            }
        }
    }

}