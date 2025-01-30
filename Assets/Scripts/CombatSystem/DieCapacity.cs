
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DieCapacity is a capacity that allows a being to die..
/// gives Immobile for eternity & changes the Layer to "Meat"
/// </summary>

public class DieCapacity : Capacity
{
    [Header("XP parameters")]
    public XPProvider xp_provider;
    public TextManager text_manager;
    public int xp_gift = 10;

    [Header("Die parameters")]
    public float time_before_disappearing = 60f;
    [SerializeField] private bool show_smiley = true;
    [SerializeField] private List<string> smileys = new List<string> { "RIP", "rip", ";-;", ":(" };

    // START
    private void Start()
    {
        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/utils/particles/xp_provider").GetComponent<XPProvider>();

        // on récupère le provider de floating dmg
        text_manager = GameObject.Find("/utils/dmgs_provider").GetComponent<TextManager>();
    }    

    // trigger the dodge
    public override void Use(Capable capable)
    {
        // check if the capable is a Being
        if (capable is not Being) { return; }

        // we play the animation
        Anim anim = play_anim(capable.anim_player, name);
        if (anim == null) { return; }

        // on donne de l'xp
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + capable.GetComponent<SpriteRenderer>().bounds.size.y / 2f,0);
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_gift, sprite_center);

        // on donne un floating dmg
        if (show_smiley)
        {
            float test = Random.Range(0, 100);
            for (int i = 0; i < smileys.Count; i++)
            {
                if (test < 100 / smileys.Count * (i + 1))
                {
                    text_manager.addFloatingText(smileys[i], sprite_center,"red");
                    break;
                }
            }
        }

        // on change le layer du perso en "meat"
        ((Being) capable).life_collider.gameObject.layer = LayerMask.NameToLayer("Meat");

        // destroy object
        Invoke(nameof(destroyObject), time_before_disappearing);
    }

    private void destroyObject()
    {
        Being being = transform.parent.GetComponent<Being>();
        if (being == null) { return; }
        if (being.Alive) { return; }
        Destroy(transform.parent.gameObject);
    }
}
