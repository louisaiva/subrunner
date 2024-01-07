using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perso : Attacker
{

    // DEBUG
    public bool CHEAT = true;

    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    private GameObject floating_text_prefab;


    // bits (mana)
    // public bool has_hackin_os = false;
    public float bits = 8f; // bits = mana (lance des sorts de hacks)
    public int max_bits = 8;
    public float regen_bits = 0.1f; // regen (en bits par seconde)

    // hack
    // public float hack_range = 2f; // distance entre le perso et la porte pour hacker
    private CircleCollider2D hack_collider;
    private ContactFilter2D hack_contact_filter = new ContactFilter2D();
    private LayerMask hack_layer;
    private Dictionary<GameObject,Hack> current_hackin_targets = new Dictionary<GameObject, Hack>(); // liste d'objets hackés en ce moment
    private Transform hacks_path; // le parent des hackin_rays
    private GameObject hackin_ray_prefab; // le prefab du hackin_ray

    // hoover hackable
    private GameObject current_hoover_hackable = null;
    private Hack current_hoover_hack = null;
    public float aide_a_la_visee = 0.5f; // aide à la visée, rayon autour de la souris pour les objets hackables
    private CursorHandler cursor_handler;


    // global light
    private GameObject global_light;

    // minimap
    // public bool has_gyroscope = false;

    // inventory & items
    public Inventory inventory;
    public UI_Inventory big_inventory;
    public Transform items_parent;

    // skill tree
    public SkillTree skills_tree;

    // interactions
    private float interact_range = 1f;
    private LayerMask interact_layers;
    public GameObject current_interactable = null;
    public Bed respawn_bed = null;

    /*


    */

    // unity functions
    new void Start()
    {

        // on start de d'habitude
        base.Start();

        // ON RECUP DES TRUCS


        // on met à jour les layers du hack
        hack_layer = LayerMask.GetMask("Doors", "Enemies","Computers");

        // on récupère le collider de hack
        hack_collider = transform.Find("center").GetComponent<CircleCollider2D>();
        hack_contact_filter.SetLayerMask(hack_layer);

        // on récupère le parent des hackin_rays
        hacks_path = transform.Find("hacks");

        // on récupère le prefab du hackin_ray
        hackin_ray_prefab = Resources.Load("prefabs/hacks/hackin_ray2") as GameObject;

        // on récupère l'inventaire
        inventory = GameObject.Find("/inventory").GetComponent<Inventory>();
        inventory.scalable = true;
        big_inventory = GameObject.Find("/ui/inventory").GetComponent<UI_Inventory>();

        // on récupère le parent des items
        // items_parent = GameObject.Find("/world/sector_2/items").transform;
        items_parent = null;

        //
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on récupère le global_light
        global_light = GameObject.Find("/world/global_light").gameObject;

        // on met à jour les interactions
        interact_layers = LayerMask.GetMask("Chests", "Computers","Buttons");

        // on récupère le cursor_handler
        cursor_handler = GameObject.Find("/utils").GetComponent<CursorHandler>();



        // ON MET A JOUR DES TRUCS

        // on ajoute des capacités
        addCapacity("hoover_interact");
        addCapacity("interact");
        addCapacity("debug_capacities");

        // on met à jour les capacités deja dans notre inventaire
        foreach (Item item in inventory.getItems())
        {
            grab(item);
        }


        // on met les differents paramètres du perso
        skills_tree = transform.Find("skills_tree").GetComponent<SkillTree>();
        skills_tree.init();

        // max_vie = 100;
        vie = (float) max_vie;
        speed = 3f;
        running_speed = 5f;
        // damage = 10f;
        attack_range = 0.3f; // defini par l'item
        damage_range = 0.5f; // defini par l'item
        cooldown_attack = 0.5f; // defini par l'item
        knockback_base = 10f;

        xp_gift = 0; // on ne donne pas d'xp quand on tue un perso

        // on met à jour les animations
        anims.init("perso");


        // on CHEAT
        if (CHEAT)
        {
            // on met l'attaque à 0.1
            damage = 0.1f;

            // on se met lvl 10 sur le skill tree
            // skills_tree.setGlobalLevel(10);
            skills_tree.setLevel("max_vie", 30);

            // on met à jour les paramètres du perso
            vie = (float) max_vie;
            // speed = 5f;

            // on rajoute hacks, lunettes etc
            inventory.createItem("speed_glasses");
            inventory.createItem("electrochoc");
            inventory.createItem("door_hack");
            inventory.createItem("ordi_hack");
        }


        // on MAJ les items
        List<Item> perso_items = inventory.getItems();
        foreach (Item item in perso_items)
        {
            grab(item);
        }


        // ON AFFICHE DES TRUCS


        // on affiche un texte de début
        Invoke("showWelcome", 5f);
    }

    // welcoming
    void showWelcome()
    {
        // on affiche un texte de début

        // WELCOME TO
        Vector3 position = transform.position + new Vector3(0, 1f, 0);
        string text = "welcome to";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 16f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);

        // SUBRUNNER
        position = transform.position + new Vector3(0, 0.5f, 0);
        text = "SUBRUNNER";
        GameObject floating_text2 = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text2.GetComponent<FloatingText>().init(text, Color.green, 30f, 0.1f, 0.2f, 16f);
        floating_text2.transform.SetParent(floating_dmg_provider.transform);


        // on affiche la quete 5sec après
        // Invoke("showQuest", 5f);
    }

    void showQuest()
    {
        // on affiche un texte de début
        Vector3 position = transform.position + new Vector3(0, 1f, 0);
        string text = "you need to";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);


        position = transform.position + new Vector3(0, 0.5f, 0);
        text = "HACK THE DOOR";
        GameObject floating_text2 = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text2.GetComponent<FloatingText>().init(text, Color.red, 30f, 0.1f, 0.2f, 6f);
        floating_text2.transform.SetParent(floating_dmg_provider.transform);

        // on affiche la suite de la quete 5sec après
        Invoke("showQuest2", 5f);
    }

    void showQuest2()
    {
        // on affiche un texte de début
        Vector3 position = transform.position + new Vector3(0, 1f, 0);
        string text = "find the usb key to hack it";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.green, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);

        // on affiche la suite de la quete 5sec après
        Invoke("showQuest3", 5f);
    }

    void showQuest3()
    {
        // on affiche un texte de début
        Vector3 position = transform.position + new Vector3(0, 1f, 0);
        string text = "but beware of";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);

        // on affiche la suite de la quete 5sec après
        position = transform.position + new Vector3(0, 0.5f, 0);
        text = "ZOMBIES";
        GameObject floating_text2 = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text2.GetComponent<FloatingText>().init(text, Color.red, 30f, 0.1f, 0.2f, 6f);
        floating_text2.transform.SetParent(floating_dmg_provider.transform);
    }

    // CAPACITES
    public override void Events()
    {

        // on vérifie que le temps est pas en pause
        if (Time.timeScale == 0f) { return; }

        // on check toutes les capacities dans le bon ordre pour voir comment on peut agir etc.
        base.Events();

        // walk
        if (hasCapacity("walk"))
        {
            inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        // run
        if (hasCapacity("run"))
        {
            if (Input.GetButtonDown("run"))
            {
                isRunning = true;
            }
            else if (Input.GetButtonUp("run"))
            {
                isRunning = false;
            }
        }

        // drink
        if (hasCapacity("drink"))
        {
            if (Input.GetButtonDown("drink"))
            {
                drink();
            }
        }

        // hit
        if (hasCapacity("hit"))
        {
            if (Input.GetButtonDown("hit"))
            {
                attack();
            }
        }

        // hoover interactions
        if (hasCapacity("hoover_interact"))
        {
            // todo ici on highlight les objets avec lesquels on peut interagir

            // interacts
            if (hasCapacity("interact"))
            {
                if (Input.GetButtonDown("interact"))
                {
                    interact();
                }
            }

            // on update les interactions
            update_interactions();
        }

        // gv vision
        if (capacities.ContainsKey("gv_vision") && capacities["gv_vision"])
        {
            global_light.GetComponent<GlobalLight>().setMode("on");
        }
        else
        {
            global_light.GetComponent<GlobalLight>().setMode("off");
        }

        // hoover hack
        if (hasCapacity("hoover_hack"))
        {
            HackinHooverEvents();

            // bit regen
            if (hasCapacity("bit_regen"))
            {
                if (bits < max_bits)
                {
                    bits += regen_bits * Time.deltaTime;
                }
            }

            // hacks
            if (hasCapacity("hack"))
            {
                if (Input.GetButtonDown("hack"))
                {
                    HackinClickEvents();
                }
            }

            // update hacks
            update_hacks();
        }



        // todo DEBUG
        if (hasCapacity("debug_capacities"))
        {
            if (Input.GetButtonDown("debug_capacities"))
            {
                string s = "<color=green>DEBUG CAPACITIES : </color>\n";
                foreach (string capacity in capacities.Keys)
                {
                    s += capacity + " : " + capacities[capacity] + "\n";
                }
                print(s);
            }
        }


        // ouverture de l'inventaire
        if (Input.GetButtonDown("Inventory"))
        {
            // on regarde si on a pas un coffre ou un ordi en train d'être ouvert
            if (current_interactable != null && !big_inventory.isShowed())
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
                current_interactable = null;
            }

            // on ouvre l'inventaire
            big_inventory.rollShow();
        }
    }

    void HackinHooverEvents()
    {
        // le but de cette fonction est de repérer les objets hackables
        // que la souris survole
        
        // 1 - on regarde si la souris est dans le range de hack
        Vector2 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, mouse_pos);
        if (distance > hack_collider.radius) {
            if (current_hoover_hackable != null)
            {
                current_hoover_hackable.gameObject.GetComponent<I_Hackable>().unOutlineMe();
                current_hoover_hackable = null;
                current_hoover_hack = null;
            }
            return;
        }

        // 2 - on récupère tous les objets dans le layermask que la souris survole
        Collider2D[] hits = Physics2D.OverlapCircleAll(mouse_pos, aide_a_la_visee, hack_layer);

        // print("POTENTIAL HACKIN " + hits.Length + " OBJECTS");

        // 3 - on regarde si on peut hacker qqch
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            // on regarde si c'est un hackable
            if (hit.gameObject.GetComponent<I_Hackable>() == null) { continue; }

            // on regarde si on est pas déjà en train de hacker l'objet
            if (!current_hackin_targets.ContainsKey(hit.gameObject))
            {
                // on parcourt tous nos hacks pour voir si on peut hacker l'objet
                foreach (Hack hack in inventory.getHacks())
                {
                    // on regarde si on peut hacker l'objet
                    if (hit.gameObject.GetComponent<I_Hackable>().isHackable(hack.hack_type_target, (int) bits))
                    {
                        // on peut hacker l'objet !!
                        // on met à jour le current_hoover_hackable.gameObject.GetComponent<I_Hackable>()
                        if (current_hoover_hackable != null && current_hoover_hackable != hit.gameObject)
                        {
                            current_hoover_hackable.gameObject.GetComponent<I_Hackable>().unOutlineMe();
                        }
                        current_hoover_hackable = hit.gameObject;
                        current_hoover_hack = hack;

                        // on change le material de l'objet
                        current_hoover_hackable.gameObject.GetComponent<I_Hackable>().outlineMe();

                        // on change le cursor
                        cursor_handler.SetCursor("target");

                        // on sort de la boucle
                        return;
                    }
                }
            }
        }

        // si on arrive ici, c'est qu'on a rien trouvé
        // on met à jour le current_hoover_hackable.gameObject.GetComponent<I_Hackable>()
        if (current_hoover_hackable != null)
        {
            // on change le material de l'objet
            current_hoover_hackable.gameObject.GetComponent<I_Hackable>().unOutlineMe();
            current_hoover_hackable = null;
            current_hoover_hack = null;

            // on remet le cursor à la normale
            cursor_handler.SetCursor("arrow");
        }
    }

    void HackinClickEvents()
    {
        // le but de cette fonction est de hacker l'objet sur lequel on clique

        // on regarde si on a un hackable en hoover
        if (current_hoover_hackable != null)
        {
            // on hack l'objet
            bits -= current_hoover_hackable.GetComponent<I_Hackable>().beHacked();

            // on ajoute le hackable au dict des objets hackés
            current_hackin_targets.Add(current_hoover_hackable, current_hoover_hack);

            // on crée un hackin_ray
            GameObject hackin_ray = Instantiate(hackin_ray_prefab, hacks_path) as GameObject;
            
            // on met à jour le hackin_ray avec le nom
            hackin_ray.name = "hackin_ray_" + current_hoover_hackable.name + "_" + current_hoover_hackable.GetInstanceID();

            // on sort de la fonction
            return;
        }
    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // update de d'habitude
        base.Update();

        /* // on update le hackin
        has_hackin_os = false;
        foreach (Item item in inventory.getItems())
        {
            if (item.action_type == "passive")
            {
                if (item.item_name == "hackin_os")
                {
                    has_hackin_os = true;
                }
            }
        }

        // on regarde si on a le gyroscope
        has_gyroscope = false;
        foreach (Item item in inventory.getItems())
        {
            if (item.action_type == "passive")
            {
                if (item.item_name == "gyroscope")
                {
                    has_gyroscope = true;
                }
            }
        }

        // on applique les capacités passives des items
        bool has_speed_glasses = false;
        foreach (Item item in inventory.getItems())
        {
            if (item.action_type == "passive")
            {
                if (item.item_name == "speed_glasses")
                {
                    has_speed_glasses = true;
                }
            }
        }

        global_light.GetComponent<GlobalLight>().setMode(has_speed_glasses ? "on" : "off"); */

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
        // xp_to_next_level = (int)(xp_to_next_level * 1.5f);

        Debug.Log("LEVEL UP ! level " + level);

        // on augmente les stats
        // max_vie += 5 + ((int) 0.2*level);
        // damage += 4 + ((int) 0.1*level);
        // cooldown_attack -= 0.05f;
        // speed += 0.1f;

        // on affiche un floating text
        /* Vector3 position = transform.position + new Vector3(0, 1f, 0);
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init("LEVEL "+level.ToString(), Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform); */

        // on ouvre le physical tree
        skills_tree.physicalLevelUp();

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

        // on desactive l'inventaire
        inventory.setShow(false);

        base.die();
    }


    // HACK
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
            hackin_ray.GetComponent<LineRenderer>().SetPosition(0, transform.Find("center").position);
            hackin_ray.GetComponent<LineRenderer>().SetPosition(1, target.transform.Find("hack_point").position);

            // on inflige des dégats à l'objet si c'est un hack de dégats
            if (current_hackin_targets[target] is DmgHack)
            {
                float damage = ((DmgHack)current_hackin_targets[target]).damage * Time.deltaTime;
                target.GetComponent<Being>().take_damage(damage);
            }
        }
    }

    public void addBits(int count)
    {
        bits += count;
        if (bits > max_bits) { bits = max_bits; }
    }

    public void setHackinRange(float range)
    {
        hack_collider.radius = range;
    }


    // INTERACTIONS
    private void interact()
    {
        // on regarde si on peut interagir avec qqch
        Collider2D[] hit_interactables = Physics2D.OverlapCircleAll(transform.position, interact_range, interact_layers);
        if (hit_interactables.Length == 0) { return; }

        Collider2D target = null;

        // on interagit avec l'objet le plus proche
        float min_distance = 10000f;
        foreach (Collider2D hit in hit_interactables)
        {
            // on regarde si c'est un interactable
            if (hit == null) { continue; }
            if (hit.gameObject.GetComponent<I_Interactable>() == null) { continue; }

            // on regarde si on peut interagir avec l'objet
            if (hit.gameObject.GetComponent<I_Interactable>().isInteractable())
            {
                // on regarde si l'objet est plus proche que le précédent
                if (Vector2.Distance(transform.position, hit.transform.position) < min_distance)
                {
                    min_distance = Vector2.Distance(transform.position, hit.transform.position);
                    target = hit;
                }
            }
        }

        // on interagit avec l'ojet
        if (target != null)
        {
            print("INTERACTING WITH " + target);

            // on met à jour l'objet avec lequel on interagit
            if (current_interactable != null && current_interactable != target.gameObject)
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
            }
            current_interactable = target.gameObject;

            // on interagit avec l'objet
            current_interactable.GetComponent<I_Interactable>().interact();
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
                // on arrête d'interagir avec l'objet
                current_interactable.GetComponent<I_Interactable>().stopInteract();
                current_interactable = null;
            }
            else if (current_interactable.GetComponent<I_LongInteractable>() != null)
            {
                I_LongInteractable long_interactable = current_interactable.GetComponent<I_LongInteractable>();
                // on regarde si cet objet interagit encore avec nous
                if (!(long_interactable.is_interacting || long_interactable.is_being_activated))
                {
                    // on arrête d'interagir avec l'objet
                    current_interactable = null;
                }
            }
        }

    }

    public void setRespawnBed(Bed bed)
    {
        respawn_bed = bed;

        // on affiche un texte de début
        Vector3 position = transform.position + new Vector3(0, 1.2f, 0);
        string text = "your respawn point has been set";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(floating_dmg_provider.transform);
    }

    // INVENTORY
    public void drop(Item item)
    {
        removeCapaOfItem(item);

        // si on est en train d'ouvrir un coffre, on drop l'item dedans
        if (current_interactable != null)
        {
            if (current_interactable.GetComponent<InventoryChest>() != null)
            {
                current_interactable.GetComponent<InventoryChest>().grab(item);
            }
            else if (current_interactable.GetComponent<Bed>() != null)
            {
                current_interactable.GetComponent<Bed>().grab(item);
            }
        }
        else
        {
            // on drop l'item par terre
            item.transform.SetParent(items_parent);
            item.transform.position = transform.position;
            item.fromInvToGround();
        }


        // on met à jour l'inventaire
        if (item.legendary_item)
        {
            big_inventory.dropLeg(item);
        }
        else
        {
            big_inventory.dropItem(item);
        }
    }

    public void grab(Item item)
    {
        item.transform.SetParent(inventory.transform);

        // on ajoute la capacité de l'item
        addCapaOfItem(item);

        // on met à jour l'inventaire
        if (item.legendary_item)
        {
            big_inventory.grabLeg(item);
        }
        else
        {
            big_inventory.grabItem(item);
        }
    }

    private void removeCapaIfNotInInv(string capa, Item item_capa=null)
    {
        // on regarde si on a encore un item avec cette capacité
        bool has_capa = false;
        foreach (Item item in inventory.getItems())
        {
            if (item == item_capa) { continue; }

            if (item.action_type == "capacity")
            {
                foreach (string capa_item in item.capacities)
                {
                    if (capa_item == capa)
                    {
                        has_capa = true;
                        break;
                    }
                }
            }
        }

        // si on a plus de capacité, on la supprime
        if (!has_capa)
        {
            removeCapacity(capa);
        }
    }

    private void removeCapaOfItem(Item item)
    {
        // on enlève la capacité de l'item si on a plus l'item dans notre inventaire
        if (item.action_type == "capacity")
        {
            foreach (string capa in item.capacities)
            {
                removeCapaIfNotInInv(capa, item);
            }
        }
    }

    private void addCapaOfItem(Item item)
    {
        // on ajoute la capacité de l'item
        if (item.action_type == "capacity")
        {
            for (int i = 0; i < item.capacities.Count; i++)
            {
                string capa = item.capacities[i];

                // on récupère le cooldown de l'item
                float cooldown = 0f;
                if (item.cooldowns.ContainsKey(capa))
                {
                    cooldown = item.cooldowns[capa];
                }

                addCapacity(capa, cooldown);
            }
        }
    }

    // DRINK
    private void drink()
    {
        // on boit la première boisson de notre inventaire
        foreach (Item item in inventory.getItems())
        {
            // on vérifie si c'est bien une boisson
            if (!(item is Drink)) { continue; }

            Drink drink = (Drink) item;

            // on boit la boisson
            drink.drink();

            // on enlève la capacité de l'item si on a plus l'item dans notre inventaire
            removeCapaIfNotInInv("drink");

            // on met à jour l'inventaire
            big_inventory.dropItem(item);

            // on sort de la fonction
            return;
        }
    }

}


