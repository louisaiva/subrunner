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
    private GameObject cam;


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
    [SerializeField] private float dash_magnitude = 25f;
    [SerializeField] private float dash_duration = 0.5f;
    public float last_dash_time = 0f;
    private List<int> dash_forces = new List<int>();

    // global light
    private GameObject global_light;


    [Header("INVENTORY")]
    public Inventory inventory;
    public UI_Inventory big_inventory;
    public Transform items_parent;
    private LayerMask grabber_layer;

    [Header("UI - MENUS")]
    public SkillTree skills_tree;
    public UI_Fullmap big_map;
    public UI_PauseMenu pause_menu;

    [Header("INTERACTIONS")]
    // interactions
    [SerializeField] private float interact_range = 1f;
    [SerializeField] private LayerMask interact_layers;
    public GameObject current_interactable = null;

    // hoover interactions
    [SerializeField] private GameObject current_hoover_interactable = null;


    [Header("INPUTS")]
    [SerializeField] private InputManager input_manager;
    public PlayerInputActions playerInputs;

    // [Header("HINTS")]
    // private UI_HintControlsManager hints_controls;

    // TALKING
    private List<string> talks_random = new List<string>()
                        {
                            "here we go again/.",
                            "well/.i'm not dead yet :D/lthat's a good start",
                            "give me some noodles !/l/.anyone ?",
                            "still not dead/./lbut am i alive ?",
                            "let's decrypt\nthe chaos, babyy",
                            "ARGHHH/.ZOMBOS !",
                            "those posters reminds\nme of the pixel war/./lwait/.wtf is the pixel war ?",
                            "looks like life has/./lno meaning after all",
                            "let's ctrl+alt+suppr\nall those cyberzombies",
                            "do you feel the\nbreath of the city ?",
                            "i'm talking alone/./las always..",
                            "bits & bytes are\nmy only love, honey",
                            "404: noodles not found :/",
                            "feel like the labyrinth\nkeeps moving/.is it ?",
                            "omg i almost glitched oO",
                            "what if i could pass\nthrough those walls ?/lwell/.I can't",
                            "this city is full of bugs/./llooks like a game actually",
                            "ahh, the smell of the city,/lso/.digital",
                            "i'm a pixel runner/.i'm a pixel runner",
                            "let's hack the world, baby !",
                            "running in da\nunderground,/lagain/.and again/.n agn",
                            "i'm so tired/./lwhen was the last\ntime i slept ?",
                            "why do i keep\nrunning, really ?",
                            "this wall looks suspicious/. O.o",
                            "feels good to run after/./l502420h of debugging",
                            "here i dash/.here i slash",
                            "where is my old\nfriend Marco ?",
                            "ayy, 9 years without\nseeing the sun,/lwho can beat that ?",
                            "i need to find the exit/./lis there even an exit ?",
                            "ahhh what if I\ncould ctrl-Z irl ?",
                            "alright./ltime to remember passwords/./ls7j3k9../lwell/.not this time apparently",
                            "what's my name again ?",
                            "ahh/./lI love feeling the\nbytes flowing in my veins..",
                            "where is the internet again ?",
                            "zombos, zombos\neverywhere/./lwelcome to the\nzapocalypse",
                            "noodles,/lthe ultimate cure\nfor existential hunger !",
                            "starving and debugging/./la perfect combo, really",
                            "is this reality or just\na weird illusion ?",
                            "zombos & hacking/./lno way it's real/./lit's so/.mainstream",
                            "OH MY GOD,/ljust found the bug\nI was struggling with/./lToo bad my computer\npassed away..",
                            "line 475,car. 45 :\n(WorldError)\nhere's comes the \"déjà vu\"",
                            "looks like the matrix/./lugh ?/lwhat did I just say ?",
                            "i could use a jetpack/./lwell no that's\nanother game./lwait/.a game ?",
                            "ohhh, cuty rat !",
                            "why do I keep\ntalking alone ?/l;-;",
                            "i'm so tired of\nrunning/l/.looks like\nI can't stop",
                            "how many times\nI've been here ?",
                            "mmmmh/./lhow many zombos lives is\nworth some noodles ?",
                            "pff !/lthose zombos are\nso annoying",
                            "ahh/./lloneliness is almost\nas scary as\nthe deep web",
                            "maybe I'll find\nsome friends/./l/. but I want noodles !",
                        };

    private List<string> talks_random_nsfw = new List<string>()
                        {
                            "fuck this shit/.\ni'm HUNGRY !",
                            "is all of this\nsh*t even real ?",
                            "ahh orangina I love that/l/.f*ck capitalism\nperchance",
                            "fuck Google I'm gonna hack them",
                        };
    
    
    [Header("TALKING")]
    [SerializeField] private bool allow_nsfw = true;
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
        enemy_layers = LayerMask.GetMask("Enemies","Beings");

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
        inventory = GameObject.Find("/inventory").GetComponent<Inventory>();
        inventory.scalable = true;
        big_inventory = GameObject.Find("/ui/inventory/ui_inventory").GetComponent<UI_Inventory>();

        // on récupère la map
        big_map = GameObject.Find("/ui/fullmap").GetComponent<UI_Fullmap>();

        // on récupère le pause_menu
        pause_menu = GameObject.Find("/ui/pause_menu").GetComponent<UI_PauseMenu>();

        // on récupère le parent des items
        // items_parent = GameObject.Find("/world/sector_2/items").transform;
        items_parent = null;

        // on récupère le grabber_layer
        grabber_layer = LayerMask.GetMask("Chests");

        //
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on récupère le global_light
        global_light = GameObject.Find("/world/global_light").gameObject;

        // on met à jour les interactions
        interact_layers = LayerMask.GetMask("Chests", "Computers","Buttons","Items","Interactives");

        // on récupère le cursor_handler
        cursor_handler = GameObject.Find("/utils").GetComponent<CursorHandler>();

        // on récupère le hints_controls
        // hints_controls = transform.Find("hint_controls").GetComponent<UI_HintControlsManager>();


        // ON MET A JOUR DES TRUCS

        // on ajoute des capacités
        addCapacity("hoover_interact");
        // addCapacity("interact");
        addCapacity("debug_capacities");
        addCapacity("talk");

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
        // anims.init("perso");
        anims = new PersoAnims();

        // on met à jour les sons
        sounds = new PersoSounds();
        // audio_manager.LoadSoundsFromPath("audio/perso");


        // on CHEAT
        if (CHEAT)
        {
            cheat();
        }

        // on MAJ les items
        inventory.getItems().ForEach(item => grab(item));




        // ON AFFICHE DES TRUCS


        // on affiche un texte de début
        Invoke("showWelcome", 5f);
        Invoke("randomTalk", Random.Range(talking_delay_range.x, talking_delay_range.y));
    }

    // welcoming
    void showWelcome()
    {
        // on affiche un texte de début

        // WELCOME TO
        /* Vector3 position = transform.position + new Vector3(0, 1f, 0);
        string text = "welcome to";
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 16f);
        floating_text.transform.SetParent(floating_dmg_provider.transform); */
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("WELCOME TO", transform.position + new Vector3(0, 1f, 0), "yellow");

        // SUBRUNNER
        /* position = transform.position + new Vector3(0, 0.5f, 0);
        text = "SUBRUNNER";
        GameObject floating_text2 = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text2.GetComponent<FloatingText>().init(text, Color.green, 30f, 0.1f, 0.2f, 16f);
        floating_text2.transform.SetParent(floating_dmg_provider.transform); */
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("SUBRUNNER", transform.position + new Vector3(0, 0.5f, 0), "green");


        // on affiche la quete 5sec après
        // Invoke("showQuest", 5f);
    }

    /* void showQuest()
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
    } */

    void randomTalk()
    {
        if (!isAlive()) { return; }
        // on fait parler le perso
        int index = Random.Range(0, talks_random.Count + (allow_nsfw ? talks_random_nsfw.Count : 0));
        if (index >= talks_random.Count)
        {
            floating_dmg_provider.GetComponent<TextManager>().talk(talks_random_nsfw[index - talks_random.Count], this);
        }
        else
        {
            floating_dmg_provider.GetComponent<TextManager>().talk(talks_random[index], this);
        }

        // on relance
        Invoke("randomTalk", Random.Range(talking_delay_range.x, talking_delay_range.y));
    }

    // INPUTS
    private void initInputs()
    {
        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        playerInputs = input_manager.inputs;

        // on active les inputs
        playerInputs.perso.Enable();
        playerInputs.enhanced_perso.Enable();

        // on set les callbacks
        playerInputs.dead_perso.revive.performed += ctx => comeback_from_death();
        playerInputs.enhanced_perso.dash.performed += ctx => OnDash();
    }

    // CAPACITES
    public override void Events()
    {
        // showCapacities();

        // on vérifie que le temps est pas en pause
        if (Time.timeScale == 0f) { return; }

        // hoover interactions
        if (hasCapacity("hoover_interact"))
        {
            // todo ici on highlight les objets avec lesquels on peut interagir
            InteractHooverEvents();

            // on update les interactions
            update_interactions();
        }

        // on vérifie qu'on est pas KO
        if (hasCapacity("knocked_out")) { return; }

        // on check toutes les capacities dans le bon ordre pour voir comment on peut agir etc.
        base.Events();

        // walk
        if (hasCapacity("walk"))
        {
            Vector2 raw_inputs = new Vector2(playerInputs.perso.move.ReadValue<Vector2>().x, playerInputs.perso.move.ReadValue<Vector2>().y);
            
            // we check if the raw inputs are below the deadzone
            raw_inputs.x = Mathf.Abs(raw_inputs.x) < 0.2 ? 0f : raw_inputs.x;
            raw_inputs.y = Mathf.Abs(raw_inputs.y) < 0.2 ? 0f : raw_inputs.y;
            
            // we normalize the inputs
            inputs = raw_inputs.normalized;
            inputs_magnitude = raw_inputs.magnitude;

            // print("inputs : " + inputs + " / raw_inputs : " + raw_inputs + " / inputs_magnitude : " + inputs_magnitude);
        }

        // run
        if (hasCapacity("run"))
        {
            if (playerInputs.perso.run.ReadValue<float>() == 1f)
            {
                isRunning = true;
            }
            else if (playerInputs.perso.run.ReadValue<float>() == 0f)
            {
                isRunning = false;
            }
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
            if (playerInputs.enhanced_perso.hackDirection.ReadValue<Vector2>() != Vector2.zero)
            {
                if (input_manager.isUsingGamepad())
                {
                    Vector2 direction = playerInputs.enhanced_perso.hackDirection.ReadValue<Vector2>();
                    HooverNextHackableInDirection(direction);
                }
                else
                {
                    Vector2 mouse_position = playerInputs.enhanced_perso.hackDirection.ReadValue<Vector2>();
                    HackinHooverEvents(mouse_position);
                }
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
                if (playerInputs.enhanced_perso.hack.ReadValue<float>() == 1f)
                {
                    HackinClickEvents();
                }
            }

            // update hacks
            update_hacks();
        }

        // DEBUG
        /* if (hasCapacity("debug_capacities"))
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
        if (!skills_tree.isShowed() && !big_map.isShowed())
        {
            if (input_manager.isUsingGamepad())
            {
                if (playerInputs.perso.inventory.ReadValue<float>() >= 1f && !big_inventory.isShowed())
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
                else if (playerInputs.perso.inventory.ReadValue<float>() < 1f && big_inventory.isShowed())
                {
                    // on ferme l'inventaire
                    big_inventory.hide();
                }
            }
        }
    }
    void HackinHooverEvents(Vector2 mouse_position)
    {

        // le but de cette fonction est de repérer les objets hackables
        // dans le range de hack et que la souris survole

        // on récup d'abord tous les objets dans le range, puis on regarde si la souris est sur un de ces objets
        // et si on peut hacker l'objet

        // 1 - on récupère tous les objets dans le range de hack
        Collider2D[] hits = new Collider2D[30];
        hack_collider.OverlapCollider(hack_contact_filter, hits);

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

    // CHEAT
    public void cheat()
    {
        // on met l'attaque à 0.1
        // damage = 0.1f;

        // on se met lvl 10 sur le skill tree
        // skills_tree.setGlobalLevel(10);
        skills_tree.setLevel("max_vie", 30);

        // on met à jour les paramètres du perso
        vie = (float)max_vie;
        // speed = 5f;

        // on rajoute hacks, lunettes etc
        grab(inventory.createItem("carbon_shoes", true));
        grab(inventory.createItem("nood_os", true));
        grab(inventory.createItem("gyroscope", true));
        grab(inventory.createItem("zombo_electrochoc"));
        grab(inventory.createItem("door_hack"));
        grab(inventory.createItem("computer_hack"));

        Debug.Log("OMGGG R U CHEATING ?!");
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
        skills_tree.physicalLevelUp();

        // on affiche un texte de level up
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("LEVEL "+level.ToString(), transform.position + new Vector3(0, 0.5f, 0), "yellow");

    }

    // DAMAGE
    protected override int attack()
    {
        int attack_status = base.attack();

        if (attack_status == 0) { return 0; }

        cam.GetComponent<CameraShaker>().shake((float) attack_status);

        return attack_status;
    }
    protected override void die()
    {
        Debug.Log("YOU DIED");

        // on affiche un floating text
        floating_dmg_provider.GetComponent<TextManager>().addFloatingText("YOU DIED", transform.position + new Vector3(0, 0.5f, 0), "red");

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
            addCapacity("interact");
        }
        else
        {
            // hints_controls.removeHint("b");
            removeCapacity("interact");
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
        } */

        if (drop_on_ground)
        {
            // on drop sur le sol avec une force dans une direction aléatoire
            item.transform.SetParent(items_parent);

            // calcule une position aléatoire autour du perso -> dans la direction du look_at
            float angle = Random.Range(0f, 360f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            // Vector2 position = (Vector2)transform.position + direction * 0.5f;

            // item.transform.position = position;
            item.transform.position = transform.position;
            item.addForce(new Force(direction, 7.5f,1.5f));
        }


        // on met à jour l'inventaire
        if (item is LegendaryItem)
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
        item.clearForces();



        // on ajoute la capacité de l'item
        addCapaOfItem(item);

        // on met à jour l'inventaire
        if (item is LegendaryItem)
        {
            big_inventory.grabLeg(item);
        }
        else
        {
            big_inventory.grabItem(item);
        }
    
        // on désactive l'item
        item.gameObject.SetActive(false);
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
    }
    private void addCapaOfItem(Item item)
    {
        // on ajoute la capacité de l'item
        if (item.action_type == "capacity")
        {
            // Debug.Log("(Perso) adding capacities of " + item.item_name + " to the perso");
            List<string> capacities = item.getCapacities();
            Dictionary<string,float> cooldowns = item.getCooldownsBase();
            
            /* string s = "capacities : ";
            foreach (string capa in capacities)
            {
                s += capa + " " + (cooldowns.ContainsKey(capa) ? cooldowns[capa] : "/") + " \n";
            }
            Debug.Log(s); */


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


        // we check the synchroneity of the dash
        float dash_tempo = Time.time - last_dash_time;
        // Debug.Log("dash tempo : " + (dash_tempo));
        last_dash_time = Time.time;

        if (dash_tempo > 0.65f && dash_tempo < 0.95f)
        {
            // on est dans le tempo !
            dash_magnitude = 64f;
            dash_duration = 0.5f;
        }
        else
        {
            // on revient à la normale
            dash_magnitude = 32f;
            dash_duration = 0.25f;
        }

        


        // on fait un dash dans la direction du look_at du being
        // float ajout = 0f;


        // on clamp les inputs dans l'une des 4 directions
        Vector2Int dash_direction = new Vector2Int(0, 0);
        if (Mathf.Abs(inputs.x) > Mathf.Abs(inputs.y))
        {
            dash_direction.x = inputs.x > 0f ? 1 : -1;
        }
        else
        {
            dash_direction.y = inputs.y > 0f ? 1 : -1;
        }




        anim_handler.StopForcing();
        // on joue l'animation
        /* if (Mathf.Abs(inputs.x) > Mathf.Abs(inputs.y))
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_side);
        }
        else if (inputs.y > 0)
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_up);
        }
        else
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_down);
            // ajout = 0.25f;
        }
        */
        if (new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(-1, 0) }.Contains(dash_direction))
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_side);
        }
        else if (dash_direction.y > 0)
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_up);
        }
        else
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims) anims).dash_down);
        }



        // on se met invicible
        // beInvicible(dash_duration);
        // beGhost(dash_duration);

        // on permet d'orienter le dash
        // addCapacity("dash_orientable");
        addEphemeralCapacity("invicible", dash_duration);
        addEphemeralCapacity("ghost", dash_duration);
        addEphemeralCapacity("dashin", 0.6f*dash_duration);

        // on fait le dash
        Force dash_force = new Force(dash_direction, dash_magnitude);
        addForce(dash_force);
        // forces.Add(dash_force);
        dash_forces.Add(dash_force.id);
    }
    private void orient_dash()
    {
        // on clamp les inputs dans l'une des 4 directions
        Vector2Int dash_direction = new Vector2Int(0, 0);
        if (Mathf.Abs(inputs.x) > Mathf.Abs(inputs.y))
        {
            dash_direction.x = inputs.x > 0f ? 1 : -1;
        }
        else
        {
            dash_direction.y = inputs.y > 0f ? 1 : -1;
        }

        // on récupère toutes les forces de dash encore actives
        List<Force> forces = getForces(dash_forces);

        // on oriente la force de dash
        foreach (Force force in forces)
        {
            force.direction = dash_direction;
        }

        // on change l'animation du perso
        anim_handler.StopForcing();
        if (new List<Vector2Int>() { new Vector2Int(1, 0), new Vector2Int(-1, 0) }.Contains(dash_direction))
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims)anims).dash_side);
        }
        else if (dash_direction.y > 0)
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims)anims).dash_up);
        }
        else
        {
            anim_handler.ChangeAnimTilEnd(((PersoAnims)anims).dash_down);
        }
    }



    // INPUTS
    public void OnUseConso()
    {
        if (!hasCapacity("knocked_out") && hasCapacity("drink"))
        {
            drink();
        }
    }
    public void OnInteract()
    {
        if (hasCapacity("knocked_out")) { return; }
        
        if (hasCapacity("interact"))
        {
            interact();
        }
    }
    public void OnInventory()
    {
        if (hasCapacity("knocked_out")) { return; }

        if (!skills_tree.isShowed())
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
        }
    }
    public void OnHit()
    {
        if (!hasCapacity("knocked_out") && hasCapacity("hit"))
        {
            attack();
        }
    }
    public void OnDash()
    {
        if (!hasCapacity("knocked_out"))
        {
            if (hasCapacity("dash"))
            {
                dash();
            }
            else if (hasCapacity("dash_orientable"))
            {
                orient_dash();
            }
        }
    }
    public void OnRandomTalk()
    {
        if (!hasCapacity("knocked_out"))
        {
            if (hasCapacity("talk"))
            {
                CancelInvoke("randomTalk");
                randomTalk();
            }
            else
            {
                floating_dmg_provider.GetComponent<TextManager>().talk("./.@.~@#", this);
            }
        }
    }
    public void OnMap()
    {
        if (!hasCapacity("knocked_out"))
        {
            if (hasCapacity("gyroscope"))
            {
                big_map.rollShow();
            }
        }
    }
    public void OnPause()
    {
        pause_menu.rollShow();
    }

}


public class PersoAnims : AttackerAnims
{
    public string dash_side = "perso_dash_RL";
    public string dash_up = "perso_dash_U";
    public string dash_down = "perso_dash_D";

    public PersoAnims()
    {
        base.init("perso");
        has_up_down_runnin = true;
        has_up_down_idle = true;
    }
}

public class PersoSounds : AttackerSounds
{
    public PersoSounds()
    {
        s_hurted = new List<string>() { "hurted - 1" , "hurted - 2" , "hurted - 3" , "hurted - 4" };
        s_attack = new List<string>() { "attack - 1" , "attack - 2"};
        base.init("perso");
    }
}