using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
public class XP_Chest : Chest, I_Buttonable
{

    // XP Provider
    public GameObject xp_provider;
    [SerializeField] protected int xp_amount = 100;
    [SerializeField] protected float xp_speed = 1f;

    // unity functions
    protected override void Awake()
    {
        base.Awake();

        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/particles/xp_provider");

        // on défini les animations
        anims = new XPChestAnims();
    }

    void Start()
    {
        success_close();
    }

    // OPENING
    protected override void success_open()
    {
        base.success_open();

        // on fait apparaitre les bits
        emitXP();
    }

    // BUTTONS
    public void buttonDown()
    {
        if (is_open) { return; }

        open();
    }
    public void buttonUp() {}

    // XP
    private void emitXP()
    {
        if (xp_amount <= 0) { return;}

        // calcule notre position centrale du sprite
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + GetComponent<SpriteRenderer>().bounds.size.y / 2f, 0);

        // on fait apparaitre les bits
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_amount, sprite_center, xp_speed);

        // on détruit le coffre
        xp_amount = 0;
    }


}


public class XPChestAnims : ChestAnims
{
    // constructor
    public XPChestAnims()
    {
        idle_open = "xp_chest_idle_open";
        idle_closed = "xp_chest_idle_closed";
        openin = "xp_chest_openin";
        closin = "xp_chest_closin";
    }

}