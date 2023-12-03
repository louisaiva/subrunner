using UnityEngine;

public class SkillTree : MonoBehaviour {
    

    // perso
    private GameObject perso;
    private GameObject floating_text_prefab;

    // autre ui à désactiver
    private GameObject main_ui;

    // PHYSICAL SKILLS
    private GameObject ui_physical_tree;
    // private bool is_physical_tree_open = false;
    public int physical_points = 0; // points à dépenser

    /*

        pour chaque skill on a 4 choses :
        - un level (l)
        - une base (b)
        - un modifier (m)
        - une stat de base constante (K)

        soit x la stat du perso,
        
        x = K + l*( b + l*m*K )


    */

    // maximum vie
    public int max_vie_level = 0;
    private float max_vie_base = 15f;
    private float max_vie_modifier = 0.1f;
    private float max_vie_K = 100f;


    // regen vie (en vie par seconde)
    public int regen_vie_level = 0;
    private float regen_vie_base = 0.15f;
    private float regen_vie_modifier = 0.1f;
    private float regen_vie_K = 1f;

    // degats
    public int degats_level = 0;
    private float degats_base = 3f;
    private float degats_modifier = 0.1f;
    private float degats_K = 10f;


    // VIRTUAL SKILLS
    private GameObject ui_virtual_tree;
    public Computer computer;
    // private bool is_virtual_tree_open = false;
    public int virtual_points = 0; // points à dépenser

    // max bits (en bits)
    // exception : on fait par octet (lol)
    // x = K*(l+1)
    public int max_bits_level = 0;
    private float max_bits_K = 8f;

    // regen bits
    public int regen_bits_level = 0;
    private float regen_bits_base = 0.03f;
    private float regen_bits_modifier = 0.2f;
    private float regen_bits_K = 0.1f;

    // portee hack
    public int portee_hack_level = 0;
    private float portee_hack_base = 0.3f;
    private float portee_hack_modifier = 0.2f;
    private float portee_hack_K = 2f;


    // unity functions

    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;

        // on récupère le main_ui
        main_ui = GameObject.Find("/ui").gameObject;


        // on récupère le ui_skill_tree
        ui_physical_tree = transform.Find("physical_tree").gameObject;
        ui_virtual_tree = transform.Find("virtual_tree").gameObject;

        // on cache les skill trees
        ui_physical_tree.GetComponent<Canvas>().enabled = false ;
    }

    public void init()
    {
        // on s'assure qu'on a bien start
        Start();

        // on met à jour les valeurs du perso
        perso.GetComponent<Perso>().max_vie = (int) calculateX("max_vie");
        perso.GetComponent<Perso>().regen_vie = calculateX("regen_vie");
        perso.GetComponent<Perso>().damage = calculateX("degats");
        perso.GetComponent<Perso>().max_bits = (int) calculateX("max_bits");
        perso.GetComponent<Perso>().regen_bits = calculateX("regen_bits");
        perso.GetComponent<Perso>().setHackinRange(calculateX("portee_hack"));
    }

    // OUVERTURE DES SKILL TREES

    // physical

    public void physicalLevelUp()
    {
        // on donne des points à dépenser (1 point par tranche de 5 niveaux)
        physical_points += 1 + perso.GetComponent<Perso>().level / 5;

        // on stoppe le temps
        Time.timeScale = 0f;

        // on desactive le main ui
        // main_ui.GetComponent<Canvas>().enabled = false ;

        // on affiche le skill tree
        ui_physical_tree.GetComponent<Canvas>().enabled = true;
        // is_physical_tree_open = true;

        // on choisit un skill au hasard après 3 secondes
        // Invoke("randomPhysicalLevelUp", 3f);
        
    }

    private void closePhysicalTree()
    {
        // on cache le skill tree
        ui_physical_tree.GetComponent<Canvas>().enabled = false ;
        Time.timeScale = 1f;
        // is_physical_tree_open = false;

        // on reactive le main ui
        // main_ui.GetComponent<Canvas>().enabled = true;
    }

    public void randomPhysicalLevelUp()
    {
        // on choisit un skill au hasard
        string[] skills = { "max_vie", "regen_vie", "degats" };
        string skill = skills[Random.Range(0, skills.Length)];

        // on augmente le skill
        levelUpSkill(skill);
    }


    // virtuel

    public void virtualLevelUp(Computer comp)
    {
        // on récupère le computer
        computer = comp;

        // on calcule les points à distribuer en fonction du niveau de l'ordi
        virtual_points += 1 + (comp.niveau -1) / 5;

        // on stoppe le temps
        Time.timeScale = 0f;

        // on desactive le main ui
        // main_ui.GetComponent<Canvas>().enabled = false ;

        // on affiche le skill tree
        ui_virtual_tree.GetComponent<Canvas>().enabled = true;
    }

    private void closeVirtualTree()
    {
        // on cache le skill tree
        ui_virtual_tree.GetComponent<Canvas>().enabled = false ;
        Time.timeScale = 1f;
        // is_virtual_tree_open = false;

        // on reactive le main ui
        // main_ui.GetComponent<Canvas>().enabled = true;
    }

    // AUGMENTATION DES SKILLS
    // le choix d'augmenter un skill particulier ne se fait pas dans cette classe mais dans les classes des skills
    // via le mouse click event
    // cependant, on met à jour les variables ici

    public void levelUpSkill(string skill)
    {
        print("level up " + skill);

        // on regarde si on a des points à dépenser
        if (isPhysicalSkill(skill) && physical_points <= 0) { return; }
        else if (!isPhysicalSkill(skill) && virtual_points <= 0) { return; }

        // on regarde quel skill on augmente
        if (skill == "max_vie")
        {
            // on augmente le skill
            max_vie_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().max_vie = (int) calculateX("max_vie");
        }
        else if (skill == "regen_vie")
        {
            // on augmente le skill
            regen_vie_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().regen_vie = calculateX("regen_vie");

        }
        else if (skill == "degats")
        {
            // on augmente le skill
            degats_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().damage = calculateX("degats");

        }
        else if (skill == "max_bits")
        {
            // on augmente le skill
            max_bits_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().max_bits = (int) calculateX("max_bits");

        }
        else if (skill == "regen_bits")
        {
            // on augmente le skill
            regen_bits_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().regen_bits = calculateX("regen_bits");

        }
        else if (skill == "portee_hack")
        {
            // on augmente le skill
            portee_hack_level++;

            // on met à jour les stats
            perso.GetComponent<Perso>().setHackinRange(calculateX("portee_hack"));

        }
        else
        {
            Debug.Log("skill " + skill + " not found");
        }

        print("level up " + skill + " to " + calculateX(skill) + " (level " + getLevel(skill) + ")");


        // on affiche un floating text
        /* Vector3 position = perso.transform.position + new Vector3(0, 0.5f, 0);
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        string text = "upgraded " + skill + " to " + calculateX(skill) + " (level " + getLevel(skill) + ")";
        floating_text.GetComponent<FloatingText>().init(text, Color.yellow, 30f, 0.1f, 0.2f, 6f);
        floating_text.transform.SetParent(perso.GetComponent<Perso>().floating_dmg_provider.transform); */


        // on met à jour les points
        if (isPhysicalSkill(skill))
        {
            physical_points--;
            // si on a plus de points, on ferme le skill tree
            if (physical_points <= 0)
            {
                // Invoke("closePhysicalTree", 1f);
                closePhysicalTree();
            }
        }
        else
        {
            virtual_points--;
            // si on a plus de points, on ferme le skill tree
            if (virtual_points <= 0)
            {
                // Invoke("closePhysicalTree", 1f);
                closeVirtualTree();
            }
        }
        

        
    }

    private float calculateX(string skill)
    {

        float x = 0f;

        // cas spécial de max_bits
        if (skill == "max_bits")
        {
            return max_bits_K * (max_bits_level + 1);
        }

        float l = 0f;
        float b = 0f;
        float m = 0f;
        float K = 0f;

        // on regarde quel skill on augmente
        switch (skill)
        {
            case "max_vie":
                l = max_vie_level;
                b = max_vie_base;
                m = max_vie_modifier;
                K = max_vie_K;
                break;
            case "regen_vie":
                l = regen_vie_level;
                b = regen_vie_base;
                m = regen_vie_modifier;
                K = regen_vie_K;
                break;
            case "degats":
                l = degats_level;
                b = degats_base;
                m = degats_modifier;
                K = degats_K;
                break;
            case "regen_bits":
                l = regen_bits_level;
                b = regen_bits_base;
                m = regen_bits_modifier;
                K = regen_bits_K;
                break;
            case "portee_hack":
                l = portee_hack_level;
                b = portee_hack_base;
                m = portee_hack_modifier;
                K = portee_hack_K;
                break;
            default:
                Debug.Log("skill " + skill + " not found");
                break;
        }

        // on calcule x
        x = K + l * (b + l * m * K);

        return x;

    }
    private float calculateXP1(string skill)
    {

        // calcul de x pour le niveau suivant
        // l2 = l + 1

        float x = 0f;

        // cas spécial de max_bits
        if (skill == "max_bits")
        {
            return max_bits_K * (max_bits_level + 2);
        }

        float l = 0f;
        float b = 0f;
        float m = 0f;
        float K = 0f;

        // on regarde quel skill on augmente
        switch (skill)
        {
            case "max_vie":
                l = max_vie_level;
                b = max_vie_base;
                m = max_vie_modifier;
                K = max_vie_K;
                break;
            case "regen_vie":
                l = regen_vie_level;
                b = regen_vie_base;
                m = regen_vie_modifier;
                K = regen_vie_K;
                break;
            case "degats":
                l = degats_level;
                b = degats_base;
                m = degats_modifier;
                K = degats_K;
                break;
            case "regen_bits":
                l = regen_bits_level;
                b = regen_bits_base;
                m = regen_bits_modifier;
                K = regen_bits_K;
                break;
            case "portee_hack":
                l = portee_hack_level;
                b = portee_hack_base;
                m = portee_hack_modifier;
                K = portee_hack_K;
                break;
            default:
                Debug.Log("skill " + skill + " not found");
                break;
        }

        // on calcule x
        int l2 = (int) l + 1;
        x = K + l2 * (b + l2 * m * K);

        return x;

    }

    // GETTERS
    private int getLevel(string skill)
    {
        // on regarde quel skill on augmente
        switch (skill)
        {
            case "max_vie":
                return max_vie_level;
            case "regen_vie":
                return regen_vie_level;
            case "degats":
                return degats_level;
            case "max_bits":
                return max_bits_level;
            case "regen_bits":
                return regen_bits_level;
            case "portee_hack":
                return portee_hack_level;
            default:
                Debug.Log("skill " + skill + " not found");
                return -1;
        }
    }

    public float getSkillValue(string skill)
    {
        return (float) calculateX(skill);
    }

    public float getNextLevelSkillValue(string skill)
    {
        // on regarde quel skill on augmente
        return (float) calculateXP1(skill);
    }

    private bool isPhysicalSkill(string skill)
    {
        // on regarde quel skill on augmente
        switch (skill)
        {
            case "max_vie":
                return true;
            case "regen_vie":
                return true;
            case "degats":
                return true;
            default:
                return false;
        }
    }
}