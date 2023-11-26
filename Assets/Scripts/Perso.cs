using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Attacker
{

    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 10;

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


    // inventory
    public Inventory inventory = new Inventory();

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

        // on récupère le collider de hack
        hack_collider = transform.Find("hack_range").GetComponent<CircleCollider2D>();
        hack_contact_filter.SetLayerMask(hack_layer);

        // on ajoute un hack de door après 2 secondes
        Invoke("add_door_hack", 2f);
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            hack();
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
        xp_to_next_level = (int)(xp_to_next_level * 1.5f);
        // max_bits += 1;
        // bits = max_bits;
        Debug.Log("LEVEL UP ! level " + level);

        // on augmente les stats
        max_vie += 10*level;
        damage += 2*level;
        cooldown_attack -= 0.05f;
        vitesse += 0.1f;
    }

    // DAMAGE
    protected override void die()
    {
        Debug.Log("YOU DIED");
        base.die();
    }


    // HACK
    private void hack()
    {
        // on regarde si on a assez de bits
        if (bits < 1) { return; }

        // on regarde si on peut hacker qqch
        // Collider2D[] hit_hackable = Physics2D.OverlapCircleAll(transform.position, hack_range, hack_layer);
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
        GameObject hackin_ray = Instantiate(Resources.Load("prefabs/hacks/hackin_ray"), hacks_path) as GameObject;
        
        // on met à jour le hackin_ray avec le nom
        hackin_ray.name = "hackin_ray_" + target.gameObject.name + "_" + target.gameObject.GetInstanceID();

        return;
        
    }

    private void update_hacks()
    {
        // on affiche les noms des objets hackés
        string hackin_targets_names = "HACKS : ";
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            hackin_targets_names += target.gameObject.name + " ";
        }
        print(hackin_targets_names);

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
            if (current_hackin_targets[target].item_type == "hack.dmg")
            {
                float damage = ((DmgHack)current_hackin_targets[target]).damage * Time.deltaTime;
                target.GetComponent<Being>().take_damage(damage, Vector2.zero);
            }
        }
    }

    private void add_door_hack(){

        print("j'ajoute un hack de porte");
        Hack door_hack = new Hack("door_hack", "door");
        inventory.addHack(door_hack);
    }

}


public class Inventory
{

    // items
    public List<Item> items = new List<Item>();

    // hacks
    public List<Hack> hacks = new List<Hack>();

    // constructor
    public Inventory()
    {
        // on ajoute des items de test
        items.Add(new Item());

        // on ajoute des hacks de test
        hacks.Add(new DmgHack("zombo_hack", "zombo",15f));

    }

    // special functions

    // functions
    public void addItem(Item item)
    {
        items.Add(item);
    }

    public void addHack(Hack hack)
    {
        hacks.Add(hack);
    }

    public void removeItem(Item item)
    {
        items.Remove(item);
    }

    public void removeHack(Hack hack)
    {
        hacks.Remove(hack);
    }

    // getters

    public List<Item> getItems()
    {
        return items;
    }

    public List<Hack> getHacks()
    {
        return hacks;
    }

    public Item getItem(string item_name)
    {
        foreach (Item item in items)
        {
            if (item.item_name == item_name)
            {
                return item;
            }
        }
        return null;
    }

    public Hack getHack(string hack_name)
    {
        foreach (Hack hack in hacks)
        {
            if (hack.item_name == hack_name)
            {
                return hack;
            }
        }
        return null;
    }

}

public class DmgHack : Hack
{
    // damage
    public float damage = 0f; // damage infligés par le hack par seconde

    // constructor
    public DmgHack()
    {
        // do nothing
        this.item_type = "hack.dmg";
    }

    public DmgHack(string item_name, string hack_type_target, float damage)
    {
        // item
        // this.init(item_name, item_description, item_icon);
        this.item_name = item_name;

        // hack
        this.item_type = "hack.dmg";
        this.hack_type_target = hack_type_target;

        // damage
        this.damage = damage;
    }

}

public class Hack : Item
{
    // hack target
    public string hack_type_target = "nothin";


    // constructor
    public Hack()
    {
        // do nothing
        this.item_type = "hack";
    }

    public Hack(string item_name, string hack_type_target)
    {
        // item
        // this.init(item_name, item_description, item_icon);
        this.item_name = item_name;

        // hack
        this.item_type = "hack";
        this.hack_type_target = hack_type_target;
    }

}

public class Item
{
    
        // item basics
        public string item_type = "item";
        public string item_name = "item";
        public string item_description = "item description";
        public string item_icon = "item icon";
    
        // constructor
        public Item()
        {
            // do nothing
        }
        
        public Item(string item_name, string item_description, string item_icon)
        {
            init(item_name, item_description, item_icon);
        }

        public void init(string item_name, string item_description, string item_icon)
        {
            this.item_name = item_name;
            this.item_description = item_description;
            this.item_icon = item_icon;
        }
}