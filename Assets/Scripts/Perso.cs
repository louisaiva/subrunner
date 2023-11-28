using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Attacker
{

    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 300;

    private GameObject floating_text_prefab;


    // bits (mana)
    public float bits = 8f; // bits = mana (lance des sorts de hacks)
    public int max_bits = 8;
    private float regen_bits = 0.1f; // regen (en bits par seconde)

    // hack
    // public float hack_range = 2f; // distance entre le perso et la porte pour hacker
    private CircleCollider2D hack_collider;
    private ContactFilter2D hack_contact_filter = new ContactFilter2D();
    private LayerMask hack_layer;
    private Dictionary<GameObject,Hack> current_hackin_targets = new Dictionary<GameObject, Hack>(); // liste d'objets hackés en ce moment
    private Transform hacks_path; // le parent des hackin_rays
    private GameObject hackin_ray_prefab; // le prefab du hackin_ray


    // global light
    private GameObject global_light;

    // inventory
    public Inventory inventory;

    // interactions
    private float interact_range = 1f;
    private LayerMask interact_layers;
    public GameObject current_interactable = null;

    /*


    */

    // unity functions
    new void Start(){

        // on start de d'habitude
        base.Start();

        // on met les differents paramètres du perso
        max_vie = 100;
        vie = (float) max_vie;
        vitesse = 3f;
        damage = 10f;
        attack_range = 0.3f;
        damage_range = 0.5f;
        cooldown_attack = 0.5f;

        // on met à jour les animations
        anims.init("perso");

        // on met à jour les layers du hack
        hack_layer = LayerMask.GetMask("Doors","Enemies");

        // on récupère le parent des hackin_rays
        hacks_path = transform.Find("hacks");

        // on récupère le prefab du hackin_ray
        hackin_ray_prefab = Resources.Load("prefabs/hacks/hackin_ray2") as GameObject;

        // on récupère le collider de hack
        hack_collider = transform.Find("hack_range").GetComponent<CircleCollider2D>();
        hack_contact_filter.SetLayerMask(hack_layer);

        // on récupère l'inventaire
        inventory = transform.Find("inventory").GetComponent<Inventory>();
        inventory.scalable = true;

        //
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on récupère le global_light
        global_light = GameObject.Find("/world/global_light").gameObject;

        // on met à jour les interactions
        interact_layers = LayerMask.GetMask("Chests");
    }

    public override void Events()
    {
        // Z,S,Q,D
        inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // attaque
        if (Input.GetKeyDown(KeyCode.Space))
        {
            attack();
        }

        // hack de porte
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hack();
        }

        // interactions
        if (Input.GetKeyDown(KeyCode.E))
        {
            interact();
        }

        // changement de mode de lumière
        if (Input.GetKeyDown(KeyCode.L))
        {
            global_light.GetComponent<GlobalLight>().roll();
        }

        // ouverture de l'inventaire
        if (Input.GetKeyDown(KeyCode.F))
        {
            inventory.rollShow();
        }
    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // régèn des bits
        if (bits < max_bits)
        {
            bits += regen_bits * Time.deltaTime;
        }

        // update de d'habitude
        base.Update();


        // on update les hacks
        update_hacks();

        // on update les interactions
        update_interactions();
    }


    // XP
    public void addXP(int count)
    {
        xp += count;
        total_xp += count;
        if (xp >= xp_to_next_level)
        {
            levelUp();
        }
    }

    private void levelUp()
    {
        level += 1;
        xp = 0;
        xp_to_next_level = (int)(xp_to_next_level * 2f);
        // max_bits += 1;
        // bits = max_bits;
        Debug.Log("LEVEL UP ! level " + level);

        // on augmente les stats
        max_vie += 5 + ((int) 0.2*level);
        damage += 4 + ((int) 0.1*level);
        cooldown_attack -= 0.05f;
        vitesse += 0.1f;

        // on affiche un floating text
        Vector3 position = transform.position + new Vector3(0, 0.5f, 0);
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init("LEVEL "+level.ToString(), Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);

    }

    // DAMAGE
    protected override void die()
    {
        Debug.Log("YOU DIED");

        // on affiche un floating text
        Vector3 position = transform.position + new Vector3(0, 0.5f, 0);
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init("YOU DIED", Color.red, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);

        base.die();
    }


    // HACK
    private void hack()
    {
        // on regarde si on a assez de bits
        if (bits < 1) { return; }

        // on regarde si on peut hacker qqch
        Collider2D[] hit_hackable = new Collider2D[30];
        int nb_hackables = hack_collider.OverlapCollider(hack_contact_filter,hit_hackable);
        if (nb_hackables == 0) { return; }

        Collider2D target = null;
        Hack used_hack = null;

        // on hacke le 1er objet qu'on trouve
        foreach (Collider2D hit in hit_hackable)
        {
            if (hit == null) { continue; }

            // on regarde si on est pas déjà en train de hacker l'objet
            if (!current_hackin_targets.ContainsKey(hit.gameObject))
            {
                // on parcourt tous nos hacks pour voir si on peut hacker l'objet
                foreach (Hack hack in inventory.getHacks())
                {
                    // on regarde si on peut hacker l'objet
                    if (hit.gameObject.GetComponent<I_Hackable>().isHackable(hack.hack_type_target, level))
                    {
                        target = hit;
                        used_hack = hack;
                        break;
                    }
                }
            }
        }

        // si on a rien trouvé, on quitte
        if (target == null) { return; }

        // on hack l'objet
        target.GetComponent<I_Hackable>().beHacked(level);

        // on enlève des bits
        bits -= 1;

        // on ajoute la porte au dict des objets hackés
        current_hackin_targets.Add(target.gameObject, used_hack);

        // on crée un hackin_ray
        GameObject hackin_ray = Instantiate(hackin_ray_prefab, hacks_path) as GameObject;
        
        // on met à jour le hackin_ray avec le nom
        hackin_ray.name = "hackin_ray_" + target.gameObject.name + "_" + target.gameObject.GetInstanceID();

        return;
        
    }

    private void update_hacks()
    {
        // on affiche les noms des objets hackés
        /* string hackin_targets_names = "HACKS : ";
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            hackin_targets_names += target.gameObject.name + " ";
        }
        print(hackin_targets_names); */

        // on vérifie si les objets hackés sont toujours hackés et toujours à portée
        Dictionary<GameObject,Hack> new_hackin_targets = new Dictionary<GameObject,Hack>();
        Collider2D[] hit_hackable = new Collider2D[30];
        hack_collider.OverlapCollider(hack_contact_filter, hit_hackable);
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            if (target.GetComponent<I_Hackable>().isGettingHacked())
            {
                bool still_hackable = false;
                foreach (Collider2D hackable in hit_hackable)
                {
                    if (hackable == target.GetComponent<Collider2D>())
                    {
                        // si l'objet est toujours hackable, on l'ajoute au dict des nouveaux objets hackés
                        new_hackin_targets.Add(target, current_hackin_targets[target]);
                        still_hackable = true;
                        break;
                    }
                }

                // si l'objet n'est plus hackable, on cancel le hack
                if (!still_hackable){
                    target.GetComponent<I_Hackable>().cancelHack();
                }
            }
        }

        // on supprime les anciens rayons de hack qui ne sont plus hackés
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            if (!new_hackin_targets.ContainsKey(target))
            {
                // on supprime le hackin_ray
                ;
                GameObject hackin_ray = hacks_path.Find("hackin_ray_" + target.gameObject.name + "_" + target.gameObject.GetInstanceID()).gameObject;
                Destroy(hackin_ray);
            }
        }


        // on met à jour le dict des objets hackés
        current_hackin_targets = new_hackin_targets;

        // affiche des rayons de hack entre le perso et les objets actuellement hackés
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            // on vérifie si on a pas déjà un hackin_ray avec le target
            GameObject hackin_ray = hacks_path.Find("hackin_ray_" + target.gameObject.name + "_" + target.gameObject.GetInstanceID()).gameObject;

            // on met à jour le hackin_ray
            hackin_ray.GetComponent<LineRenderer>().SetPosition(0, transform.position);
            hackin_ray.GetComponent<LineRenderer>().SetPosition(1, target.transform.position);

            // on inflige des dégats à l'objet si c'est un hack de dégats
            print("hackin target : " + target.name + " " + current_hackin_targets[target]);
            if (current_hackin_targets[target] is DmgHack)
            {
                print("on fait des dégats : " + ((DmgHack)current_hackin_targets[target]).damage);
                float damage = ((DmgHack)current_hackin_targets[target]).damage * Time.deltaTime;
                target.GetComponent<Being>().take_damage(damage, Vector2.zero);
            }
        }
    }

    public void addBits(int count)
    {
        bits += count;
        if (bits > max_bits) { bits = max_bits; }
    }


    // INTERACTIONS
    private void interact()
    {
        // on regarde si on peut interagir avec qqch
        Collider2D[] hit_interactables = Physics2D.OverlapCircleAll(transform.position, interact_range, interact_layers);
        if (hit_interactables.Length == 0) { return; }

        Collider2D target = null;

        // on interagit avec le 1er objet qu'on trouve
        foreach (Collider2D hit in hit_interactables)
        {
            if (hit == null) { continue; }

            // on regarde si on est pas déjà en train d'interagir avec l'objet
            if (hit.gameObject != current_interactable)
            {
                target = hit;
                break;
            }
        }

        // on interagit avec le 1er objet qu'on trouve
        if (target != null)
        {
            // on met à jour l'objet avec lequel on interagit
            current_interactable = target.gameObject;

            // on regarde si c'est un coffre
            if (current_interactable.GetComponent<Chest>() != null)
            {
                current_interactable.GetComponent<Chest>().open();
            }
        }

    }

    private void update_interactions()
    {

        // on vérifie si l'objet avec lequel on interagit est toujours à portée
        if (current_interactable != null)
        {
            // on regarde si on est toujours à portée de l'objet
            float distance = Vector2.Distance(transform.position, current_interactable.transform.position);
            if (distance > interact_range*1.5f)
            {
                // on referme le coffre
                if (current_interactable.GetComponent<Chest>() != null)
                {
                    current_interactable.GetComponent<Chest>().close();
                }

                // on arrête d'interagir avec l'objet
                current_interactable = null;
            }
        }

    }

}