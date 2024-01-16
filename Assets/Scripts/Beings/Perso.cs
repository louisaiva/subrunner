using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Perso : Attacker
{

    // DEBUG
    [Header("CHEAT")]
    public bool CHEAT = true;

    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    private GameObject floating_text_prefab;


    [Header("HACKIN")]
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
    private Transform hacks_path; // le parent des hackrays

    // HACKIN RAY
    private GameObject hackray_prefab; // le prefab du hackray
    private HackrayHoover hackray_hoover;

    // hoover hackable
    private GameObject current_hoover_hackable = null;
    private Hack current_hoover_hack = null;
    public float aide_a_la_visee = 0.5f; // aide à la visée, rayon autour de la souris pour les objets hackables
    private CursorHandler cursor_handler;

    [Header("DASH")]
    // DASH
    [SerializeField] private float dash_distance = 1.2f;
    [SerializeField] private float dash_duration = 0.5f;

    // global light
    private GameObject global_light;


    [Header("INVENTORY")]
    // minimap
    // public bool has_gyroscope = false;

    // inventory & items
    public Inventory inventory;
    public UI_Inventory big_inventory;
    public Transform items_parent;

    [Header("SKILLTREE")]
    // skill tree
    public SkillTree skills_tree;

    [Header("INTERACTIONS")]
    // interactions
    [SerializeField] private float interact_range = 1f;
    [SerializeField] private LayerMask interact_layers;
    public GameObject current_interactable = null;
    public Bed respawn_bed = null;

    // hoover interactions
    [SerializeField] private GameObject current_hoover_interactable = null;


    [Header("INPUTS")]
    public PlayerInputActions playerInputs;

    [Header("HINTS")]
    private UI_HintControlsManager hints_controls;

    // inputs
    private void Awake()
    {
        // on récupère les inputs
        playerInputs = new PlayerInputActions();

        // on récupère le UI_XboxNavigator
        // ui_inputs = GameObject.Find("/ui").GetComponent<UI_XboxNavigator>();

        playerInputs.dead_perso.revive.performed += ctx => comeback_from_death();
    }

    private void OnEnable()
    {
        playerInputs.perso.Enable();
        playerInputs.enhanced_perso.Enable();
    }

    private void OnDisable()
    {
        playerInputs.perso.Disable();
        playerInputs.enhanced_perso.Disable();
    }

    // unity functions
    new void Start()
    {

        // on start de d'habitude
        base.Start();

        // ON RECUP DES TRUCS


        // on met à jour les layers du hack
        // hack_layer = LayerMask.GetMask("Doors", "Enemies", "Computers");
        hack_layer = LayerMask.GetMask("Hackables");

        // on récupère le parent des hackrays
        hacks_path = transform.Find("hacks");
        hackray_hoover = hacks_path.transform.Find("hoover").GetComponent<HackrayHoover>();

        // on récupère le collider de hack
        hack_collider = hacks_path.GetComponent<CircleCollider2D>();
        hack_contact_filter.SetLayerMask(hack_layer);

        // on récupère les hackray
        hackray_prefab = Resources.Load("prefabs/hacks/hackray") as GameObject;

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
        interact_layers = LayerMask.GetMask("Chests", "Computers","Buttons","Items");

        // on récupère le cursor_handler
        cursor_handler = GameObject.Find("/utils").GetComponent<CursorHandler>();

        // on récupère le hints_controls
        hints_controls = transform.Find("hint_controls").GetComponent<UI_HintControlsManager>();


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
            inventory.createItem("carbon_shoes", true);
            inventory.createItem("noodle_os", true);
            inventory.createItem("zombo_electrochoc");
            inventory.createItem("door_hack");
            inventory.createItem("computer_hack");
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
            if (inputs == Vector2.zero)
            {
                inputs = new Vector2(playerInputs.perso.move.ReadValue<Vector2>().x, playerInputs.perso.move.ReadValue<Vector2>().y);
            }
        }

        // run
        if (hasCapacity("run"))
        {
            if (Input.GetButtonDown("run") || playerInputs.perso.run.ReadValue<float>() == 1f)
            {
                isRunning = true;
            }
            else if (Input.GetButtonUp("run") || playerInputs.perso.run.ReadValue<float>() == 0f)
            {
                isRunning = false;
            }
        }

        // drink
        if (hasCapacity("drink"))
        {
            if (Input.GetButtonDown("drink"))// || playerInputs.perso.use_conso.ReadValue<float>() == 1f)
            {
                drink();
            }
        }

        // hit
        if (hasCapacity("hit"))
        {
            if (Input.GetButtonDown("hit") || playerInputs.perso.hit.ReadValue<float>() == 1f)
            {
                attack();
            }
        }

        // hoover interactions
        if (hasCapacity("hoover_interact"))
        {
            // todo ici on highlight les objets avec lesquels on peut interagir
            InteractHooverEvents();

            // interacts
            if (hasCapacity("interact"))
            {
                if (Input.GetButtonDown("interact"))// || playerInputs.perso.interact.ReadValue<float>() == 1f)
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
            if (Input.GetButtonDown("Mouse X") || Input.GetButtonDown("Mouse Y"))
            {
                HackinHooverEvents();
            }
            else if (playerInputs.enhanced_perso.hackDirection.ReadValue<Vector2>() != Vector2.zero)
            {
                Vector2 direction = playerInputs.enhanced_perso.hackDirection.ReadValue<Vector2>();
                HooverNextHackableInDirection(direction);
            }
            updateHackinHoover();

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
                if (Input.GetButtonDown("hack") || playerInputs.enhanced_perso.hack.ReadValue<float>() == 1f)
                {
                    HackinClickEvents();
                }
            }

            // update hacks
            update_hacks();
        }

        // dash
        if (hasCapacity("dash"))
        {
            if (Input.GetButtonDown("dash") || playerInputs.enhanced_perso.dash.ReadValue<float>() == 1f)
            {
                dash();
            }
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
        else if (playerInputs.perso.inventory.ReadValue<float>() > 0f && !big_inventory.isShowed())
        {
            // on regarde si on a pas un coffre ou un ordi en train d'être ouvert
            if (current_interactable != null && !big_inventory.isShowed())
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
                current_interactable = null;
            }

            // on ouvre l'inventaire
            big_inventory.show();
        }
        else if (playerInputs.perso.inventory.ReadValue<float>() == 0f && big_inventory.isShowed())
        {
            // on ferme l'inventaire
            big_inventory.hide();
        }
    }

    void HackinHooverEvents()
    {

        // le but de cette fonction est de repérer les objets hackables
        // dans le range de hack et que la souris survole

        // on récup d'abord tous les objets dans le range, puis on regarde si la souris est sur un de ces objets
        // et si on peut hacker l'objet

        // 1 - on récupère tous les objets dans le range de hack
        Collider2D[] hits = new Collider2D[30];
        hack_collider.OverlapCollider(hack_contact_filter, hits);

        // 2 - on récupère l'objet hackable le plus proche de la souris dans le range aide_a_la_visee
        Vector2 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // on prépare l'objet hackable le plus proche
        float distance = 10000f;
        GameObject hackable = null;
        Hack used_hack = null;

        // on parcourt tous les objets dans le range
        for (int i=0;i<hits.Length;i++)
        {

            // on regarde si c'est un objet
            if (hits[i] == null) { continue; }

            // on regarde si c'est un hackable
            GameObject hit = hits[i].transform.parent.gameObject;
            if (hit.GetComponent<I_Hackable>() == null) { continue; }

            // on affiche le HackUI
            hit.GetComponent<I_Hackable>().showHackUI();

            // on regarde si la distance entre la souris et l'objet est plus petite que la distance précédente
            float new_distance = Vector2.Distance(mouse_pos, hit.transform.Find("hack_point").position);
            if (new_distance < aide_a_la_visee && new_distance < distance)
            {
                // on parcourt tous nos hacks pour voir si on peut hacker l'objet
                foreach (Hack hack in inventory.getHacks())
                {
                    // on regarde si on peut hacker l'objet
                    if (hit.GetComponent<I_Hackable>().isHackable(hack.hack_type_target, (int)bits))
                    {
                        // on peut hacker l'objet !!
                        hackable = hit;
                        distance = new_distance;
                        used_hack = hack;

                        // on sort de la boucle
                        break;
                    }
                }
            }
        }

        // 3 - on vérifie ce qu'on fait avec l'objet hackable
        if (hackable != null)
        {
            // on regarde si c'est le même objet que le précédent
            if (current_hoover_hackable != null && current_hoover_hackable == hackable)
            {
                return;
            }

            // on peut hacker l'objet !!
            setHooverHackable(hackable, used_hack);

            // on change le cursor
            cursor_handler.SetCursor("target");

            // on sort de la fonction
            return;
        }
        else if (current_hoover_hackable != null)
        {
            unsetHooverHackable();

            // on remet le cursor à la normale
            cursor_handler.SetCursor("arrow");
        }


    }

    void updateHackinHoover()
    {
        // on regarde si on a un hackable en hoover
        if (current_hoover_hackable != null)
        {
            Collider2D target_collider = current_hoover_hackable.GetComponent<Collider2D>();
            // bool is_touching = hack_collider.IsTouching(current_hoover_hackable.GetComponent<Collider2D>(), hack_contact_filter);
            bool is_touching = false;

            // 1 - on récupère tous les objets dans le range de hack
            Collider2D[] hits = new Collider2D[30];
            hack_collider.OverlapCollider(hack_contact_filter, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit == null) { continue; }
                if (hit.transform.parent.gameObject == current_hoover_hackable)
                {
                    is_touching = true;
                    break;
                }
            }


            // on regarde si le hackable est toujours à portee et toujours hackable
            if (!is_touching || !current_hoover_hackable.gameObject.GetComponent<I_Hackable>().isHackable(current_hoover_hack.hack_type_target, (int)bits))
            {
                unsetHooverHackable();

                // on remet le cursor à la normale
                cursor_handler.SetCursor("arrow");
            }

        }
    }

    void HackinClickEvents()
    {
        // le but de cette fonction est de hacker l'objet sur lequel on clique

        // on regarde si on a un hackable en hoover
        if (current_hoover_hackable != null)
        {
            if (current_hackin_targets.ContainsKey(current_hoover_hackable) && current_hackin_targets[current_hoover_hackable] == current_hoover_hack)
            {
                // on a déjà hacké l'objet, on sort de la fonction
                return;
            }

            // on hack l'objet
            bits -= current_hoover_hackable.GetComponent<I_Hackable>().beHacked();

            // on ajoute le hackable au dict des objets hackés
            current_hackin_targets.Add(current_hoover_hackable, current_hoover_hack);

            // on crée un hackray
            GameObject hackray = Instantiate(hackray_prefab, hacks_path) as GameObject;
            hackray.name = "hackray_" + current_hoover_hackable.name + "_" + current_hoover_hackable.GetInstanceID();
            hackray.GetComponent<Hackray>().SetHackerAndTarget(gameObject, current_hoover_hackable);

            unsetHooverHackable();

            return;
        }
    }

    void HooverNextHackableInDirection(Vector2 direction)
    {

        // 1 - on récupère tous les objets dans le range de hack
        Collider2D[] hits = new Collider2D[30];
        hack_collider.OverlapCollider(hack_contact_filter, hits);
        if (hits.Length == 0) { return; }

        // 2 - on parcourt tous les hackable et on trouve celui avec le plus petit angle
        float min_angle = 1000f;
        GameObject hackable = null;
        Hack used_hack = null;

        // on parcourt tous les objets dans le range
        for (int i = 0; i < hits.Length; i++)
        {
            // on regarde si c'est un objet
            if (hits[i] == null) { continue; }

            // on regarde si c'est un hackable
            GameObject hit = hits[i].transform.parent.gameObject;
            if (hit.GetComponent<I_Hackable>() == null) { continue; }

            // on calcule l'angle entre le perso et l'objet
            Vector2 direction_to_hit = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(direction, direction_to_hit);
            if (angle < min_angle)
            {
                // on parcourt tous nos hacks pour voir si on peut hacker l'objet
                foreach (Hack hack in inventory.getHacks())
                {
                    // on regarde si on peut hacker l'objet
                    if (hit.GetComponent<I_Hackable>().isHackable(hack.hack_type_target, (int)bits))
                    {
                        // et si on est pas déjà en train de hacker l'objet
                        if (current_hackin_targets.ContainsKey(hit.gameObject))// && current_hackin_targets[hit.gameObject] == hack)
                        {
                            // on peut plus hacker l'objet, on continue
                            break;
                        }

                        // on peut hacker l'objet !!
                        hackable = hit;
                        min_angle = angle;
                        used_hack = hack;

                        // on sort de la boucle
                        break;
                    }
                }
            }
        }

        // 3 - on vérifie ce qu'on fait avec l'objet hackable
        if (hackable != null)
        {
            // on regarde si c'est le même objet que le précédent
            if (current_hoover_hackable != null && current_hoover_hackable == hackable)
            {
                return;
            }

            // on peut hacker l'objet !!
            setHooverHackable(hackable, used_hack);

            // on sort de la fonction
            return;
        }
        else if (current_hoover_hackable != null)
        {
            unsetHooverHackable();
        }
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
        CancelInvoke("DestroyObject");

        // on désactive les touches
        playerInputs.perso.Disable();
        playerInputs.enhanced_perso.Disable();
        playerInputs.dead_perso.Enable();
    }

    protected override void comeback_from_death()
    {
        base.comeback_from_death();

        // on change le layer du perso en "perso"
        gameObject.layer = LayerMask.NameToLayer("Player");

        // on reactive les touches
        playerInputs.dead_perso.Disable();
        playerInputs.perso.Enable();
        playerInputs.enhanced_perso.Enable();
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

        Dictionary<GameObject,Hack> new_hackin_targets = new Dictionary<GameObject,Hack>();

        // on récupère tous les objets hackables dans le range
        Collider2D[] hackables = new Collider2D[30];
        hack_collider.OverlapCollider(hack_contact_filter, hackables);

        // on regarde si les objets hackés sont toujours hackables
        foreach (GameObject target in current_hackin_targets.Keys)
        {
            // on regarde si l'objet est toujours hackable
            if (target.GetComponent<I_Hackable>().isGettingHacked())
            {
                bool still_hackable = false;
                foreach (Collider2D hackable in hackables)
                {
                    // on vérifie qu'on a un hit
                    if (hackable == null) { continue; }

                    if (hackable.transform.parent.gameObject == target)
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
                // on supprime le hackray
                GameObject hackray = hacks_path.Find("hackray_" + target.gameObject.name + "_" + target.gameObject.GetInstanceID()).gameObject;
                Destroy(hackray);
            }
        }

        // on met à jour le dict des objets hackés
        current_hackin_targets = new_hackin_targets;

        // todo / ça devrait être dans le update de l'objet hacké ?
        // todo / ou dans le noodle_os ?
        foreach (GameObject target in current_hackin_targets.Keys)
        {
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

    private void setHooverHackable(GameObject hackable, Hack hack)
    {
        current_hoover_hackable = hackable;
        current_hoover_hack = hack;

        hackray_hoover.setTarget(hackable);
        hackray_hoover.show();

        print("HOVERING " + current_hoover_hackable.gameObject.name);
    }

    private void unsetHooverHackable()
    {
        current_hoover_hackable = null;
        current_hoover_hack = null;

        // on met à jour le hackray_hoover
        hackray_hoover.removeTarget();
    }

    // INTERACTIONS
    private void InteractHooverEvents()
    {
        // on regarde si on peut interagir avec qqch
        // si oui on donne la capacité "interact" au perso
        // et on active le hint_controls d'interaction

        // on regarde tout d'abord si on a pas des hackables en hoover
        if (current_hoover_hackable != null)
        {
            // on arrête d'interagir avec l'objet si on a
            if (current_interactable != null)
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
                current_interactable = null;
            }
            current_hoover_interactable = null;
        }
        else
        {
            // on regarde si on peut interagir avec qqch
            Collider2D[] hit_interactables = Physics2D.OverlapCircleAll(transform.position, interact_range, interact_layers);
            if (hit_interactables.Length != 0)
            {
                Collider2D target = null;

                // on hoover l'objet le plus proche
                float min_distance = 10000f;
                foreach (Collider2D hit in hit_interactables)
                {
                    // on regarde si c'est un interactable
                    if (hit == null) { continue; }
                    if (hit.gameObject.GetComponent<I_Interactable>() == null) { continue; }

                    // * cas spécial : si c'est un coffre et que le big inventory est ouvert, on ne peut pas interagir avec
                    if (hit.gameObject.GetComponent<InventoryChest>() != null && big_inventory.isShowed()) { continue; }

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

                // on met à jour l'objet avec lequel on peut interagir
                /* if (current_hoover_interactable != null && current_hoover_interactable != target.gameObject)
                {
                    current_hoover_interactable.GetComponent<I_Interactable>().stopHoover();
                } */
                if (target != null)
                {
                    current_hoover_interactable = target.gameObject;
                }
                else
                {
                    current_hoover_interactable = null;
                }
            }
            else
            {
                current_hoover_interactable = null;
            }
        }


        // on regarde si on a un objet avec lequel on peut interagir
        if (current_hoover_interactable != null)
        {
            hints_controls.addHint("b");
            addCapacity("interact");
        }
        else
        {
            hints_controls.removeHint("b");
            removeCapacity("interact");
        }

    }

    private void interact()
    {
        // on interagit avec l'ojet
        if (current_hoover_interactable != null)
        {
            print("INTERACTING WITH " + current_hoover_interactable);

            // on met à jour l'objet avec lequel on interagit
            if (current_interactable != null && current_interactable != current_hoover_interactable)
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
            }
            current_interactable = current_hoover_interactable;

            // on interagit avec l'objet
            current_interactable.GetComponent<I_Interactable>().interact();

            // si c'est un item on arrête d'interagir avec l'objet
            if (current_interactable.GetComponent<Item>() != null)
            {
                current_interactable = null;
            }
        }
    }

    private void update_interactions()
    {

        // on vérifie si l'objet avec lequel on interagit est toujours à portée
        if (current_interactable != null)
        {

            // on regarde si on est toujours à portée de l'objet
            bool is_in_range = false;

            // on récupère tous les objets dans le range
            Collider2D[] hit_interactables = Physics2D.OverlapCircleAll(transform.position, interact_range, interact_layers);
            if (hit_interactables.Length != 0)
            {
                Collider2D target = null;

                // on vérifie si l'objet est dans la liste des objets avec lesquels on peut interagir
                foreach (Collider2D hit in hit_interactables)
                {
                    // on regarde si c'est un interactable
                    if (hit == null) { continue; }
                    if (hit.gameObject.GetComponent<I_Interactable>() == null) { continue; }

                    // et si c'est l'objet avec lequel on interagit
                    if (hit.gameObject == current_interactable)
                    {
                        is_in_range = true;
                        break;
                    }
                }
            }

            // on arrête d'interagir avec l'objet si on est plus à portée
            if (!is_in_range)
            {
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

        bool drop_on_ground = true;

        // si on est en train d'ouvrir un coffre, on drop l'item dedans
        if (current_interactable != null)
        {
            if (current_interactable.GetComponent<InventoryChest>() != null)
            {
                current_interactable.GetComponent<InventoryChest>().grab(item);
                drop_on_ground = false;
            }
            else if (current_interactable.GetComponent<Bed>() != null)
            {
                current_interactable.GetComponent<Bed>().grab(item);
                drop_on_ground = false;
            }
        }


        // sinon on drop l'item par terre
        if (drop_on_ground)
        {
            item.transform.SetParent(items_parent);

            // calcule une position aléatoire autour du perso -> dans la direction du look_at
            float angle = Random.Range(0f, 360f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = (Vector2)transform.position + direction * 0.5f;
            
            item.transform.position = position;
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
                    print("added cooldown " + item.cooldowns[capa] + " to " + capa);
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

    // DASH
    private void dash()
    {
        // start cooldown
        startCapacityCooldown("dash");


        // on fait un dash dans la direction du look_at du being
        float ajout = 0f;

        anim_handler.StopForcing();
        // on joue l'animation
        if (Mathf.Abs(inputs.x) > Mathf.Abs(inputs.y))
        {
            anim_handler.ChangeAnimTilEnd(anims.dash_side);
        }
        else if (inputs.y > 0)
        {
            anim_handler.ChangeAnimTilEnd(anims.dash_up);
        }
        else
        {
            anim_handler.ChangeAnimTilEnd(anims.dash_down);
            ajout = 0.25f;
        }

        // on se met invicible
        beInvicible(dash_duration);

        // on fait le dash
        Vector2 movement = inputs.normalized * (dash_distance + ajout);
        move_perso(movement);

    }

    private void beInvicible(float duration=0.5f)
    {
        capacities["invicible"] = true;
        Invoke("stopInvicibility", duration);
    }

    private void stopInvicibility()
    {
        capacities["invicible"] = false;
        capacities.Remove("invicible");
    }


    // INPUTS
    public void OnUseConso()
    {
        if (hasCapacity("drink"))
        {
            drink();
        }
    }

    public void OnInteract()
    {
        if (hasCapacity("interact"))
        {
            interact();
        }
    }
}


