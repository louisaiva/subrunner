using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Perso : Being
{

    [Header("PERSO")]
    // exploits (xp)
    public int level = 1;
    public int xp = 0;
    public int total_xp = 0;
    public int xp_to_next_level = 100;

    private GameObject floating_text_prefab;
    private GameObject cam;

    public Room current_room { get; set; }


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
    
    
    // [Header("DASH")]
    // DASH
    // [SerializeField] private float dash_magnitude = 25f;
    // [SerializeField] private float dash_duration = 0.5f;
    // public float last_dash_time = 0f;
    // private List<int> dash_forces = new List<int>();

    // global light
    // private GameObject global_light;


    [Header("INVENTORY")]
    public Inventory inventory;
    // public UI_OldInventory big_inventory;
    public Transform items_parent;
    private LayerMask grabber_layer;

    // [Header("UI - MENUS")]
    // public SkillTree skills_tree;
    // public UI_Fullmap big_map;

    [Header("INTERACTIONS")]
    // interactions
    // [SerializeField] private float interact_range = 1f;
    [SerializeField] private LayerMask interact_layers;
    public GameObject current_interactable = null;

    // hoover interactions
    // [SerializeField] private GameObject current_hoover_interactable = null;


    [Header("INPUTS")]
    [SerializeField] private InputManager input_manager;
    public PlayerInputActions inputs_actions;
    private event System.Action<InputAction.CallbackContext> reviveCallback;
    private event System.Action<InputAction.CallbackContext> dodgeCallback;


    // [Header("HINTS")]
    // private UI_HintControlsManager hints_controls;

    // TALKING

    [SerializeField] private Vector2 talking_delay_range = new Vector2(10f, 30f);

    // unity functions
    new void Start()
    {

        // on récupère les inputs
        initInputs();

        // on start de d'habitude
        base.Start();

        // ON RECUP DES TRUCS

        // on récup la camera
        cam = GameObject.Find("/cam_follow/cam");


        // on met à jour le layer des ennemis
        // target_layers = LayerMask.GetMask("Enemies","Beings");

        // on met à jour les layers du hack
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
        inventory = GameObject.Find("/inventory")?.GetComponent<Inventory>();
        if (inventory != null) {inventory.scalable = true;}
        // big_inventory = GameObject.Find("/ui/inventory/ui_inventory").GetComponent<UI_OldInventory>();

        // on récupère la map
        // big_map = GameObject.Find("/ui/fullmap").GetComponent<UI_Fullmap>();

        // on récupère le parent des items
        // items_parent = GameObject.Find("/world/sector_2/items").transform;
        items_parent = null;

        // on récupère le grabber_layer
        grabber_layer = LayerMask.GetMask("Chests");

        //
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on met à jour les interactions
        interact_layers = LayerMask.GetMask("Chests", "Computers","Buttons","Items","Interactives");



        // ON MET A JOUR DES TRUCS

        // on met les differents paramètres du perso
        // skills_tree = transform.Find("skills_tree").GetComponent<SkillTree>();
        // skills_tree.init();

        life = (float) max_life;

        // on MAJ les items
        // inventory?.getItems().ForEach(item => grab(item));




        // ON AFFICHE DES TRUCS


        // on affiche un texte de début
        // Invoke("showWelcome", 5f);
        Invoke("showQuest", 10f);
    }

    // welcoming
    void showWelcome()
    {
        // on affiche un texte de début
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("WELCOME TO", transform.position + new Vector3(0, 1f, 0), "yellow");
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("SUBRUNNER", transform.position + new Vector3(0, 0.5f, 0), "green");

        // on affiche la quete 5sec après
        // Invoke("showQuest", 5f);
    }

    private string quest_text = "mission 1 :\nfind the\nELEVATOR";
    void showQuest()
    {
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText(quest_text, transform.position + new Vector3(0, 0.5f, 0), "yellow");
    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        inputs_actions = input_manager.inputs;
        inputs_actions.perso.Enable();

        // on set les callbacks
        reviveCallback = ctx => comeback_from_death();
        dodgeCallback = ctx => OnDodge();
        inputs_actions.perso.dodge.performed += dodgeCallback;
        inputs_actions.perso.attack.performed += ctx => OnAttack();
        inputs_actions.perso.randomTalk.performed += ctx => OnRandomTalk();
        // inputs_actions.perso.interact.performed += ctx => OnInteract();
    }

    // CAPACITES
    public override void Events()
    {
        // showCapacities();

        // on vérifie que le temps est pas en pause
        if (Time.timeScale == 0f) { return; }

        // hoover interactions
        if (Can("hoover_interact"))
        {
            // todo ici on highlight les objets avec lesquels on peut interagir
            // InteractHooverEvents();

            // on update les interactions
            // update_interactions();
        }

        // on vérifie qu'on est pas KO
        if (HasEffect(Effect.Stunned)) { return; }

        // on check toutes les capacities dans le bon ordre pour voir comment on peut agir etc.
        base.Events();

        // walk
        if (Can("walk"))
        {
            Vector2 raw_inputs = new Vector2(inputs_actions.perso.move.ReadValue<Vector2>().x, inputs_actions.perso.move.ReadValue<Vector2>().y);
            
            // we check if the raw inputs are below the deadzone
            raw_inputs.x = Mathf.Abs(raw_inputs.x) < 0.2 ? 0f : raw_inputs.x;
            raw_inputs.y = Mathf.Abs(raw_inputs.y) < 0.2 ? 0f : raw_inputs.y;

            // we normalize the inputs
            Orientation = raw_inputs.normalized;
            inputs_magnitude = raw_inputs.magnitude;


            // print("inputs : " + inputs + " / raw_inputs : " + raw_inputs + " / inputs_magnitude : " + inputs_magnitude);
        }

        // run
        if (Can("run"))
        {
            if (inputs_actions.perso.run.ReadValue<float>() == 1f)
            {
                isRunning = true;
            }
            else if (inputs_actions.perso.run.ReadValue<float>() == 0f)
            {
                isRunning = false;
            }
        }


        // gv vision
        // global_light.GetComponent<GlobalLight>().setMode("on");
        /* if (capacities.ContainsKey("gv_vision") && capacities["gv_vision"])
        {
        }
        else
        {
            global_light.GetComponent<GlobalLight>().setMode("off");
        } */

        // hoover hack
        if (Can("hoover_hack"))
        {
            if (inputs_actions.enhanced_perso.hackDirection.ReadValue<Vector2>() != Vector2.zero)
            {
                if (input_manager.isUsingGamepad())
                {
                    Vector2 direction = inputs_actions.enhanced_perso.hackDirection.ReadValue<Vector2>();
                    HooverNextHackableInDirection(direction);
                }
                else
                {
                    Vector2 mouse_position = inputs_actions.enhanced_perso.hackDirection.ReadValue<Vector2>();
                    HackinHooverEvents(mouse_position);
                }
            }
            updateHackinHoover();

            // bit regen
            if (Can("bit_regen"))
            {
                if (bits < max_bits)
                {
                    bits += regen_bits * Time.deltaTime;
                }
            }

            // hacks
            if (Can("hack"))
            {
                if (inputs_actions.enhanced_perso.hack.ReadValue<float>() == 1f)
                {
                    HackinClickEvents();
                }
            }

            // update hacks
            update_hacks();
        }

        // DEBUG
        /* if (Can("debug_capacities"))
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
        } */

        // ouverture de l'inventaire
        /* if (!skills_tree.isShowed() && !big_map.isShowed())
        { 
            if (input_manager.isUsingGamepad())
            {
                if (inputs_actions.perso.inventory.ReadValue<float>() >= 1f && !big_inventory.isShowed())
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
                else if (inputs_actions.perso.inventory.ReadValue<float>() < 1f && big_inventory.isShowed())
                {
                    // on ferme l'inventaire
                    big_inventory.hide();
                }
            }
        }*/
    }
    void HackinHooverEvents(Vector2 mouse_position)
    {

        // le but de cette fonction est de repérer les objets hackables
        // dans le range de hack et que la souris survole

        // on récup d'abord tous les objets dans le range, puis on regarde si la souris est sur un de ces objets
        // et si on peut hacker l'objet

        // 1 - on récupère tous les objets dans le range de hack
        Collider2D[] hits = new Collider2D[30];
        hack_collider.Overlap(hack_contact_filter, hits);

        // 2 - on récupère l'objet hackable le plus proche de la souris dans le range aide_a_la_visee
        Vector2 mouse_pos = Camera.main.ScreenToWorldPoint(mouse_position);

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

            // on regarde si on est pas déjà en train de hacker l'objet
            /* if (current_hackin_targets.ContainsKey(hit.gameObject))// && current_hackin_targets[hit.gameObject] == hack)
            {
                // on peut plus hacker l'objet, on continue
                continue;
            } */

            // on affiche le HackUI
            // hit.GetComponent<I_Hackable>().showHackUI();

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
            hack_collider.Overlap(hack_contact_filter, hits);
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
        hack_collider.Overlap(hack_contact_filter, hits);
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
        xp_to_next_level = (int)(xp_to_next_level * 1.5f);
        // xp_to_next_level = (int)(xp_to_next_level * 1.5f);

        Debug.Log("LEVEL UP ! level " + level);


        // on ouvre le physical tree
        // skills_tree.physicalLevelUp();

        // on augmente x1.5 l'attaque
        if (hasCapacity("attack"))
        {
            transform.Find("attack").GetComponent<AttackCapacity>().damage *= 1.5f;
        }

        // on affiche un texte de level up
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("LEVEL "+level.ToString(), transform.position + new Vector3(0, 0.5f, 0), "yellow");

    }

    // DAMAGE
    public override bool take_damage(float damage, Force knockback = null)
    {
        bool dmg_status = base.take_damage(damage, knockback);
        if (!dmg_status) { return false; }

        // we make a little screenshake if perso
        float shake_magnitude = damage / life * 2f;
        cam.GetComponent<CameraShaker>().shake(shake_magnitude);

        return true;
    }
    public void Die()
    {

        Debug.Log("YOU DIED");

        // on affiche un floating text
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("YOU DIED", transform.position + new Vector3(0, 0.5f, 0), "red");

        // on desactive l'inventaire
        inventory.setShow(false);

        // on désactive les touches
        inputs_actions.perso.Disable();

        // on active la possibilité de revenir à la vie
        inputs_actions.any.keyboard.performed += reviveCallback;
        inputs_actions.any.gamepad.performed += reviveCallback;
    }
    protected override void comeback_from_death()
    {
        base.comeback_from_death();

        // on reactive les touches
        inputs_actions.perso.Enable();
        // inputs_actions.enhanced_perso.Enable();

        // on active la possibilité de revenir à la vie
        inputs_actions.any.keyboard.performed -= reviveCallback;
        inputs_actions.any.gamepad.performed -= reviveCallback;
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
        hack_collider.Overlap(hack_contact_filter, hackables);

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

        // on vérifie si on pas déjà en train de hacker l'objet
        if (!current_hackin_targets.ContainsKey(hackable))// && current_hackin_targets[hackable] == hack)
        {
            // on affiche le hackray_hoover
            hackray_hoover.setTarget(hackable);
            hackray_hoover.show();
            return;
        }

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
    /* private void InteractHooverEvents()
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
            // on autorise les triggers
            Physics2D.queriesHitTriggers = true;
            
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
                    // if (hit.gameObject.GetComponent<InventoryChest>() != null && big_inventory.isShowed()) { continue; }

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
                if (target != null)
                {
                    if (current_hoover_interactable != null && current_hoover_interactable != target.gameObject)
                    {
                        current_hoover_interactable.GetComponent<I_Interactable>().OnPlayerInteractRangeExit();
                    }
                    current_hoover_interactable = target.gameObject;
                    current_hoover_interactable.GetComponent<I_Interactable>().OnPlayerInteractRangeEnter();
                }
                else
                {
                    if (current_hoover_interactable != null)
                    {
                        current_hoover_interactable.GetComponent<I_Interactable>().OnPlayerInteractRangeExit();
                    }
                    current_hoover_interactable = null;
                }
            }
            else
            {
                if (current_hoover_interactable != null)
                {
                    current_hoover_interactable.GetComponent<I_Interactable>().OnPlayerInteractRangeExit();
                }
                current_hoover_interactable = null;
            }
        }


        // on regarde si on a un objet avec lequel on peut interagir
        if (current_hoover_interactable != null)
        {
            // hints_controls.addHint("b");
            AddCapacity("interact");
        }
        else
        {
            // hints_controls.removeHint("b");
            RemoveCapacity("interact");
        }

    }
    private void interact()
    {
        // on interagit avec l'ojet
        if (current_hoover_interactable != null)
        {
            print("INTERACTING WITH " + current_hoover_interactable);

            // on interagit avec l'objet
            current_hoover_interactable.GetComponent<I_Interactable>().interact(gameObject);

            // si c'est un item, on quitte tout simplement
            if (current_hoover_interactable.GetComponent<Item>() != null) { return; }

            // on met à jour l'objet avec lequel on interagit
            if (current_interactable != null && current_interactable != current_hoover_interactable)
            {
                current_interactable.GetComponent<I_Interactable>().stopInteract();
            }
            current_interactable = current_hoover_interactable;
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
        }

    }
 */


    // INVENTORY
    /* public void drop(Item item)
    {
        // removeCapaOfItem(item);

        bool drop_on_ground = true;

        // si on est en train d'ouvrir un coffre, on drop l'item dedans
        if (current_interactable != null)
        {
            if (current_interactable.GetComponent<InventoryChest>() != null)
            {
                current_interactable.GetComponent<InventoryChest>().grab(item);
                drop_on_ground = false;
            }
        }



        // sinon on drop l'item par terre
        /* if (drop_on_ground)
        {

            I_Grabber grabber = null;

            // on cherche si on a un grabber dans le rayon
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, grabber_layer);
            foreach (Collider2D hit in hits)
            {
                if (hit == null) { continue; }
                if (hit.gameObject.GetComponent<I_Grabber>() != null && hit.gameObject.GetComponent<I_Grabber>().canGrab())
                {
                    grabber = hit.gameObject.GetComponent<I_Grabber>();
                    break;
                }
            }


            if (grabber != null)
            {
                grabber.grab(item.gameObject);
            }
            else
            {
                // on drop sur le sol

                item.transform.SetParent(items_parent);

                // calcule une position aléatoire autour du perso -> dans la direction du look_at
                float angle = Random.Range(0f, 360f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 position = (Vector2)transform.position + direction * 0.5f;
                
                item.transform.position = position;
                // item.fromInvToGround();
            }
        } 

        /* if (drop_on_ground)
        {
            // on drop sur le sol avec une force dans une direction aléatoire
            item.transform.SetParent(items_parent);

            // calcule une position aléatoire autour du perso -> dans la direction du look_at
            float angle = Random.Range(0f, 360f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            // Vector2 position = (Vector2)transform.position + direction * 0.5f;

            // item.transform.position = position;
            item.transform.position = transform.position;
            // item.AddForce(new Force(direction, 7.5f,1.5f));
        } 


        // on met à jour l'inventaire
        /* if (item is LegendaryItem)
        {
            big_inventory.dropLeg(item);
        }
        else
        {
            big_inventory.dropItem(item);
        } 

        // on active l'item
        item.gameObject.SetActive(true);
    }
    public void grab(Item item)
    {

        // Debug.Log("GRABBING " + item.gameObject.name + " prefab scene ? " + item.gameObject.scene.name);
        // on vérifie que l'item existe
        if (item == null || item.gameObject == null) { return; }

        // on vérifie si on a affaire à un item déjà instancié ou pas
        if (item.gameObject.scene.name == null)
        {
            // on instancie l'item
            item = Instantiate(item, transform.position, Quaternion.identity) as Item;

            // on met le zoom à 0.5
            item.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        item.transform.SetParent(inventory.transform);

        // on vérifie si l'item a un component collider activé
        if (item.GetComponent<Collider2D>() != null && !item.GetComponent<Collider2D>().enabled)
        {
            item.GetComponent<Collider2D>().enabled = true;
        }

        // on supprime les forces de l'item
        item.ClearForces();



        // on ajoute la capacité de l'item
        // addCapaOfItem(item);

        // on met à jour l'inventaire
        f (item is LegendaryItem)
        {
            big_inventory.grabLeg(item);
        }
        else
        {
            big_inventory.grabItem(item);
        } 
    
        // on désactive l'item
        item.gameObject.SetActive(false);
    } */
    /* private void removeCapaIfNotInInv(string capa, Item item_capa=null)
    {
        // on regarde si on a encore un item avec cette capacité
        bool has_capa = false;
        foreach (Item item in inventory.getItems())
        {
            if (item == item_capa) { continue; }

            if (item.action_type == "capacity")
            {
                foreach (string capa_item in item.getCapacities())
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
            foreach (string capa in item.getCapacities())
            {
                removeCapaIfNotInInv(capa, item);
            }
        }
    } */
    /* private void addCapaOfItem(Item item)
    {
        // on ajoute la capacité de l'item
        if (item.action_type == "capacity")
        {
            // Debug.Log("(Perso) adding capacities of " + item.item_name + " to the perso");
            List<string> capacities = item.getCapacities();
            Dictionary<string,float> cooldowns = item.getCooldownsBase();

            for (int i = 0; i < capacities.Count; i++)
            {
                string capa = capacities[i];

                // on récupère le cooldown de l'item
                float cooldown = 0f;
                if (cooldowns.ContainsKey(capa))
                {
                    // print("added cooldown " + cooldowns[capa] + " to " + capa);
                    cooldown = cooldowns[capa];
                }
                addCapacity(capa, cooldown);
            }
        }
    } */


    // DRINK
    /* private void drink()
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
            // removeCapaIfNotInInv("drink");

            // on met à jour l'inventaire
            // big_inventory.dropItem(item);

            // on sort de la fonction
            return;
        }
    } */
  

    // INPUTS
    /* public void OnUseConso()
    {
        if (Can("drink") && !HasEffect(Effect.Stunned))
        {
            drink();
        }
    }
    public void OnInteract()
    {
        if (HasEffect(Effect.Stunned)) { return; }
        
        if (Can("interact"))
        {
            interact();
        }
    } */
    public void OnInventory()
    {
        if (HasEffect(Effect.Stunned)) { return; }

        /* if (!skills_tree.isShowed())
        {
            if (!input_manager.isUsingGamepad())
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
        } */
    }
    public void OnAttack()
    {
        // if (!inputs_actions.perso.enabled) { return; }

        if (Can("attack") && !HasEffect(Effect.Stunned))
        {
            Do("attack");
        }
    }
    public void OnRandomTalk()
    {
        if (Can("talk") && !HasEffect(Effect.Stunned))
        {
            Do("talk");
        }
    }
    private void OnDodge()
    {
        // on vérifie que l'input action map est activé
        // if (!inputs_actions.perso.enabled) { return; }

        // Debug.Log("DODGE : input_action enabled ?"+inputs_actions.perso.enabled);

        // on vérifie que le perso peut dodge
        if (!Can("dodge")) { return; }
        Do("dodge");

        // on devient invicible et on fait un PETIT dodge dans une direction.
        // nous stunt un peu après ?
        // reset le cooldown de l'attaque ?
        // addEphemeralCapacity("invicible", dodge_duration);
        // AddEffect(Effect.Invincible, dodge_duration);


        // reset le dodge
        // startCapacityCooldown("dodge");


    }

}