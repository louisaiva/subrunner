using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : IA
{

    [Header("Target")]
    public bool target_detected = false;
    public LayerMask target_layers;
    protected float player_detection_radius = 3f;
    protected Being target;


    [Header("ATTACKER")]
    public float attack_range = 0.25f; // distance entre le point d'attaque et le being qui attaque

    // UPDATE
    /* protected override void Update()
    {
        // update de d'habitude
        base.Update();

        // on fait l'idle
        IdleBehaviour();
    }
    protected virtual void IdleBehaviour()
    {
        // on essaye de détecter le joueur
        detectTarget(player_detection_radius);

        // on s'y dirige
        if (target_detected)
        {
            GoTo(target.transform.position);
        }
        else { GoNowhere(); } // on n'a pas de cible, on ne bouge pas


        // on essaye d'attaquer le joueur si on le détecte
        try_to_attack_target();
    } */

    // DETECTING TARGET
    protected void detectTarget(float radius)
    {
        Collider2D[] targets = get_targets_colliders(radius);

        // on regarde si on a trouvé un ennemi
        target_detected = targets.Length > 0;
        if (target_detected)
        {
            target = targets[0].transform.parent.gameObject.GetComponent<Being>();
        }
        else
        {
            target = null;
        }
    }
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

    // ATTACKING TARGET
    protected void try_to_attack_target()
    {
        // on vérifie qu'on a un target et qu'il est vivant
        if (!target_detected) { return; }
        if (!target.Alive) { return; }
        
        // on recupère la distance entre le zombo et le joueur
        float distance = Vector2.Distance(transform.position, target.transform.position);

        // on regarde si la target est dans le cercle d'attaque
        if (distance < attack_range)
        {
            // on attaque
            if (Can("attack")) { Do("attack"); }
        }
    }
}