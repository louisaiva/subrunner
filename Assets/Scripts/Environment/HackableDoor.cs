using UnityEngine;

[RequireComponent(typeof(AnimationHandler))]
public class HackableDoor : MonoBehaviour, I_Hackable
{
    // La classe DOOR sert à créer des portes et à les ouvrir
    // comme elle n'est pas alignée avec les tiles, il faut faire attention à la position de la porte
    // elle doit être alignée avec les tiles

    // ANIMATION
    private AnimationHandler anim_handler;
    private DoorAnims anims = new DoorAnims();
    

    // secu de départ
    public int secu = 1;

    // OPENING
    public bool is_open = false;
    private BoxCollider2D box_collider;

    // closing
    private float auto_closin_delay = 8f;

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

    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on initialise le hackin
        initHack();
    }

    void Update()
    {

        // on met à jour le hackin
        if (is_getting_hacked)
        {
            // on met à jour le hackin
            updateHack();
        }

        // on met à jour les animations
        if (is_open)
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_open);
        }
        else
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_closed);
        }

        // on met à jour le collider
        box_collider.enabled = !is_open;
    }

    public void open()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.openin)) { return; }

        // on ouvre la porte
        is_open = true;

        // on ferme la porte après un certain temps
        Invoke("close", auto_closin_delay);
    }

    public void close()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.closin)) { return; }

        // on ferme la porte
        is_open = false;
    }

    // HACKIN
    public void initHack()
    {

        // on récupère le bit_provider
        bit_provider = GameObject.Find("/particles/xp_provider");
        
        // on initialise le hackin
        hack_type_self = "door";
        is_getting_hacked = false;
        hacking_duration_base = 2f;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // bits necessaires pour hacker
        security_lvl = secu;
        required_bits_base = 2;
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));

        // on met à jour le material
        default_material = GetComponent<SpriteRenderer>().material;
        outline_material = Resources.Load<Material>("materials/outlined/outlined_unlit");
    }

    public bool isHackable(string hack_type, int bits)
    {
        // on regarde si on a le bon type de hack
        if (hack_type != hack_type_self) { return false; }

        // on regarde si on a le bon niveau de hack
        if (bits < required_bits) { return false; }

        return true;
    }

    int I_Hackable.beHacked()
    {

        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return 0; }

        // on calcule la durée du hack
        // todo voir Computer.cs

        hacking_current_duration = hacking_duration_base;
        float hackin_speed = hacking_duration_base / hacking_current_duration;
        if (hacking_current_duration < 0.1f) { hacking_current_duration = 0.1f; }

        // on met à jour les animations
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(anims.hackin,hackin_speed);

        // on hack la porte
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
        int bits_left = Mathf.RoundToInt((required_bits * time_left / hacking_duration_base) / 2);

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on drop les bits restants
        bit_provider.GetComponent<XPProvider>().EmitBits(bits_left, transform.position, 0.5f);

        // on ferme la porte
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(anims.idle_closed);
        is_open = false;
    }

    public void succeedHack()
    {
        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on ouvre la porte
        is_open = true;

        // on ferme la porte après un certain temps
        Invoke("close", auto_closin_delay);
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
}

