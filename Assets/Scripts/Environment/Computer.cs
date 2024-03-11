using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Computer : MonoBehaviour, I_Hackable, I_Interactable, I_FileHolder
{

    // la classe CHEST sert à créer des coffres
    // peut contenir des objets physiques
    // pas obligatoirement alignée avec les tiles

    // mettre un champ de force qui attire les objets vers le coffre,
    // comme ça on peut juste drop les objets à côté du coffre

    // perso
    private SkillTree tree;

    // ORDI
    public bool is_on = false;
    public bool is_loading = false;
    public float time_to_live = 0f; // si on ne l'utilise pas pendant ce temps, il s'éteint
    [SerializeField] private float time_to_live_base = 30f; // si on ne l'utilise pas pendant ce temps, il s'éteint


    // LOCKIN
    public bool is_locked = true;
    public string password = "802";

    public int niveau = 1;





    // ANIMATION
    private AnimationHandler anim_handler;
    protected ComputerAnims anims = new ComputerAnims();
    [SerializeField] private float turnin_on_duration = 1f;

    // HACKIN
    public string hack_type_self { get; set; }
    public int required_bits { get; set; }
    public int required_bits_base { get; set; }
    public int security_lvl { get; set; }
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set; }
    public float hacking_end_time { get; set; }
    public float hacking_current_duration { get; set; } // temps de hack actuel

    // bit_provider
    public GameObject bit_provider { get; set; }

    // hackable outline
    public Material outline_material { get; set; }
    public Material default_material { get; set; }

    // HackUI
    // public HackUI // hack_ui { get; set; }

    // interactions
    public GameObject ui_computer;
    public bool is_interacting { get; set; } // est en train d'interagir
    public Transform interact_tuto_label { get; set; }

    // UNITY FUNCTIONS
    protected void Start()
    {
        // on récupère le label
        interact_tuto_label = transform.Find("interact_tuto_label");

        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le skill tree
        tree = GameObject.Find("/perso/skills_tree").GetComponent<SkillTree>();

        // on récupère le ui_computer
        ui_computer = GameObject.Find("/ui/screen/ui_computer");

        // on initialise le hackin
        initHack();

        // on initialise les fichiers
        files = new List<File>();
        max_files = 12;
        AddFile(new File("mdp", "802"));
        AddFile(new File("mot", "aujourd'hui, j'ai mangé une pomme"));
        AddFile(new File("delta - etoile", "", "mp3"));
        AddFile(new File("overwatch", "", "exe"));
        AddFile(new File("castor", "", "png"));
        AddFile(new File("castor2", "", "png"));

    }

    protected void Update()
    {

        // on met à jour les bits nécessaires pour hacker
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));


        // on update selon si l'ordi est allumé ou pas
        if (is_on)
        {
            // on regarde si on a fini l'animation
            // if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            // anim_handler.ChangeAnim(anims.idle_on);

            // on update le hackin
            if (is_getting_hacked)
            {
                // on met à jour le hackin
                updateHack();
            }

            // on update le ttl
            if (is_interacting || is_getting_hacked)
            {
                // on est utilisé -> on reset le temps de vie
                time_to_live = time_to_live_base;
            }

            // on regarde si on doit s'éteindre
            if (time_to_live >= 0f)
            {
                time_to_live -= Time.deltaTime;
                if (time_to_live <= 0f)
                {
                    turnOff();
                }
            }
            
        }
    }

    // MAIN FUNCTIONS
    public void turnOn()
    {
        // on regarde si on est déjà allumé
        if (is_on) { return; }

        is_loading = true;

        // on met à jour les animations
        anim_handler.ChangeAnim(anims.onin, turnin_on_duration);

        // on allume l'ordi
        Invoke("succeedTurnOn", turnin_on_duration);
    }

    public virtual void succeedTurnOn()
    {
        // on met à jour les animations
        anim_handler.ChangeAnim(anims.idle_on);

        // on allume l'ordi
        is_on = true;
        is_loading = false;
    }

    public virtual void turnOff()
    {
        // on regarde si on est déjà éteint
        if (!is_on) { return; }

        // on regarde si on est pas en train d'intéragir
        if (is_interacting)
        {
            // on arrête l'affichage de l'UI_Computer
            ui_computer.GetComponent<UI_Computer>().hide();
            is_interacting = false;
        }


        is_loading = true;

        // on met à jour les animations
        anim_handler.ChangeAnim(anims.offin, turnin_on_duration);

        // on allume l'ordi
        Invoke("succeedTurnOff", turnin_on_duration);
    }	

    public virtual void succeedTurnOff()
    {
        // on met à jour les animations
        anim_handler.ChangeAnim(anims.idle_off);

        // on allume l'ordi
        is_on = false;
        is_loading = false;

        // on bloque l'ordi
        is_locked = true;
    }


    // UNLOCKIN
    public bool unlock(string password="")
    {
        // on regarde si on peut le débloquer
        if (!is_on || !is_locked) { return false; }

        // on regarde si le mot de passe est bon
        if (password != this.password) { return false; }

        // on débloque l'ordi
        Invoke("succeedUnlock", 1f);

        return true;
    }

    public void succeedUnlock()
    {
        // on met à jour les animations
        anim_handler.ChangeAnim(anims.idle_on);

        // on débloque l'ordi
        is_locked = false;
    }


    // HACKIN
    public void initHack()
    {

        // on récupère le bit_provider
        bit_provider = GameObject.Find("/particles/xp_provider");

        // on initialise le hackin
        hack_type_self = "computer";
        is_getting_hacked = false;
        hacking_duration_base = 5f;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // bits necessaires pour hacker
        security_lvl = niveau;
        required_bits_base = 4;
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));

        // on met à jour le material
        default_material = GetComponent<SpriteRenderer>().material;
        outline_material = Resources.Load<Material>("materials/targeted/hack_computer");

        // on récupère le // hack_ui
        // hack_ui = transform.Find("// hack_ui").GetComponent<HackUI>();
    }

    public bool isHackable(string hack_type, int bits)
    {
        // on change le mode de l'UI
        // hack_ui.setMode("unhackable");

        // on regarde si l'ordi est allumé
        if (!is_on) { return false; }
        if (anim_handler.IsForcing()) { return false; }

        // on regarde si on a le bon type de hack
        if (hack_type != hack_type_self) { return false; }

        // on regarde si on a assez de bits
        if (bits < required_bits) { return false; }

        // on change le mode de l'UI
        // hack_ui.setMode("hackable");

        return true;
    }

    int I_Hackable.beHacked()
    {

        // on regarde si l'ordi est allumé
        if (!is_on) { return 0; }

        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return 0; }

        // on calcule la durée du hack
        // chaque niveau de hack en plus réduit le temps de hack de 5% par rapport au niveau précédent
        // fonction qui tend vers 0 quand le niveau de hack augmente MAIS qui ne peut pas être négative
        // * FONCTIONNE JUSQU'AU NIVEAU 60 !! c super

        // todo : envoyer le Hack dans la fonction beHacked et appliquer les dégats en fonction du hack
        // todo : le hack influe sur la vitesse de hack, PAS sur le nombre de bits nécessaires pour hacker

        /* float hackin_duration = hacking_duration_base * Mathf.Pow(0.95f, lvl - required_hack_lvl); */

        hacking_current_duration = hacking_duration_base;
        // float hackin_speed = hacking_duration_base / hacking_current_duration;
        if (hacking_current_duration < 0.1f) { hacking_current_duration = 0.1f; }

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.hackin, hacking_current_duration)) { return 0; }

        // on change le mode de l'UI
        // hack_ui.setMode("hacked");

        // on commence le hack
        is_getting_hacked = true;
        hacking_end_time = Time.time + hacking_current_duration;

        return required_bits;
    }

    bool I_Hackable.isGettingHacked()
    {
        return is_getting_hacked;
    }

    public void updateHack()
    {
        // on regarde si on a fini le hack
        if (Time.time > hacking_end_time)
        {
            // on réussit le hack
            succeedHack();
        }
    }

    public void cancelHack()
    {
        // on calcule le temps restant
        float time_left = hacking_end_time - Time.time;

        // on calcule le nombre de bits restants et on en drop la moitié
        int bits_left = Mathf.RoundToInt((required_bits * time_left / hacking_duration_base)/2);

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on drop les bits restants
        bit_provider.GetComponent<XPProvider>().EmitBits(bits_left, transform.position, 0.5f);

        // on met à jour les animations
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(is_on ? anims.idle_on : anims.idle_off);

        // on met à jour HackUI
        // hack_ui.setMode("unhackable");

        // on met à jour les animations
        succeedTurnOn();
    }

    public void succeedHack()
    {

        // on affiche le virtual tree du perso
        // tree.virtualLevelUp(this);

        // on augmente le niveau de l'ordi
        // niveau++;
        // security_lvl = niveau;

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on met à jour HackUI
        // hack_ui.setMode("unhackable");

        // on débloque l'ordi
        // succeedTurnOn();
        succeedUnlock();
    }


    // outlines
    public void outlineMe()
    {
        // on change le material
        GetComponent<SpriteRenderer>().material = outline_material;
    }
    public void unOutlineMe()
    {
        // on change le material
        GetComponent<SpriteRenderer>().material = default_material;
    }

    // HackUI
    /* public void showHackUI()
    {
        // on le montre
        // hack_ui.show();
    }
    public void hideHackUI()
    {

        // on le montre
        // hack_ui.hide();
    } */

    // INTERACTIONS
    public bool isInteractable()
    {
        // on vérifie qu'on est pas déjà en train d'allumer/eteindre l'ordi
        if (is_loading) { return false; }

        // on vérifie qu'on est pas déjà en train d'interagir
        if (is_on && is_interacting) { return false; }

        return true;
    }
    public void interact(GameObject target)
    {

        // on vérifie quelle interaction on fait
        if (!is_on)
        {
            // on allume l'ordi
            turnOn();

            // on lance le timer d'exctinction automatique
            time_to_live = time_to_live_base;
        }
        else
        {
            // on lance l'affichage de l'UI_Computer
            ui_computer.GetComponent<UI_Computer>().setComputer(this);
            ui_computer.GetComponent<UI_Computer>().show();

            is_interacting = true;
        }

    }
    public void stopInteract()
    {
        OnPlayerInteractRangeExit();

        // on arrête l'interaction
        if (is_interacting)
        {
            // on arrête l'affichage de l'UI_Computer
            ui_computer.GetComponent<UI_Computer>().hide();
            is_interacting = false;
        }
    }


    public void OnPlayerInteractRangeEnter()
    {
        if (interact_tuto_label == null)
        {
            interact_tuto_label = transform.Find("interact_tuto_label");
            if (interact_tuto_label == null) { return; }
        }

        // on affiche le label
        interact_tuto_label.gameObject.SetActive(true);
    }

    public void OnPlayerInteractRangeExit()
    {
        // on cache le label
        interact_tuto_label.gameObject.SetActive(false);
    }







    // FILES
    public List<File> files { get; set; }
    public int max_files { get; set; }
    public Transform root { get; set; }
    public string root_name { get; set; }


    // FILE MANAGEMENT
    public void ClearFiles()
    {
        files.Clear();
    }

    public bool AddFile(File file)
    {
        // on vérifie qu'on peut ajouter un fichier
        if (files.Count < max_files)
        {
            // on ajoute le fichier
            files.Add(file);
            return true;
        }
        return false;
    }

    public bool RemoveFile(File file)
    {
        // on vérifie qu'on peut retirer un fichier
        if (files.Contains(file))
        {
            // on retire le fichier
            files.Remove(file);
            return true;
        }
        return false;
    }

    public List<File> GetFiles()
    {
        // if (files == null) {  }

        return files;
    }




}

public class ComputerAnims
{

    // ANIMATIONS
    public string idle_on = "computer_idle_on";
    public string idle_off = "computer_idle_off";
    public string onin = "computer_powerin_on";
    public string offin = "computer_powerin_off";
    public string hackin = "computer_hacked";

}