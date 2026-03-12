
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DieCapacity is a capacity that allows a being to die..
/// gives Immobile for eternity & changes the Layer to "Meat"
/// </summary>

public class DieCapacity : Capacity
{
    public int deaths = 0;

    [Header("XP parameters")]
    public XPProvider xp_provider;
    public TextManager text_manager;
    public int xp_gift = 10;

    [Header("Die parameters")]
    [SerializeField] private bool show_smiley = true;
    [SerializeField] private List<string> smileys = new List<string> { "RIP", "rip", ";-;", ":(", "://" };

    // START
    private void Start()
    {
        // on récupère le provider d'xp
        xp_provider = GameObject.Find("/game/particles/xp_provider").GetComponent<XPProvider>();

        // on récupère le provider de floating dmg
        text_manager = GameObject.Find("/game/dmgs_provider").GetComponent<TextManager>();
    }

    // trigger the dying
    public override void Use(Capable capable)
    {
        // check if the capable is a Being
        if (capable is not Being) { return; }

        // we play the animation
        Anim anim = capable.AnimPlayer.Play(name);
        if (anim == null) { return; }

        // on meurt
        deaths += 1;

        // on donne de l'xp
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + capable.AnimPlayer.Renderer.bounds.size.y / 2f, 0);
        int xp_to_drop = xp_gift;
        if (capable is Perso perso) { xp_to_drop = perso.total_xp/2; } // if the player dies he drops half of his total xp
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_to_drop, sprite_center);

        // on donne un floating dmg
        if (show_smiley)
        {
            float test = Random.Range(0, 100);
            for (int i = 0; i < smileys.Count; i++)
            {
                if (test < 100 / smileys.Count * (i + 1))
                {
                    text_manager.addFloatingText(smileys[i], sprite_center, "red");
                    break;
                }
            }
        }

        // destroy object
        StartCoroutine(destroyObject());
    }
    private IEnumerator destroyObject()
    {
        // get the being
        Being being = capable as Being;
        being.body_collider.gameObject.layer = LayerMask.NameToLayer("Meat");

        // 1 - DROP ITEMS
        if (being.Inventory != null && being.Inventory.Count > 0)
        {
            /* // we get the drop capacity
            DropCapacity dropper = being.GetCapacity<DropCapacity>();
            if (dropper == null)
            {
                // we add it if not present
                being.AddCapacity("drop");

                // we wait a frame
                yield return null;
                dropper = being.GetCapacity<DropCapacity>();
            }
            dropper.random_direction = true;
            dropper.lock_magnitude = false;

            // we drop all items
            int i = 0;
            while (i < being.Inventory.Items.Count)
            {
                Item item = being.Inventory.Items[i];
                if (item == null)
                {
                    being.Inventory.Items.RemoveAt(i);
                    continue; // skip null items
                }

                // we drop the item
                dropper.Select(item);
                dropper.Use(being);
            } */
            yield return being.DropAllItems(); // we wait for dropping all items
        }
        // being.Inventory?.RemoveAllUIs(); // on supprime les ui de l'inventory


        being.Die();

        // 2 - DESTROYING CAPACITIES
        if (log) { Debug.Log("Destroying capacities of " + being.name); }

        // we destroy all capacities (except DieCapacity FOR NOW)
        List<Capacity> capacities = new List<Capacity>(being.GetCapacities());
        capacities.RemoveAll(capa => capa.name == "die");
        while (capacities.Count > 0)
        {
            being.RemoveCapacity(capacities[0].name);
            capacities.RemoveAt(0);
        }


        // 3 - DESTROYING OTHER ELEMENTS
        if (being.transform.Find("brain") is Transform brain && brain != null) { Destroy(brain.gameObject); }
        if (being.transform.Find("goals") is Transform goal && goal != null) { Destroy(goal.gameObject); }
        if (being.transform.Find("eyes") is Transform eyes && eyes != null) { Destroy(eyes.gameObject); }
        // if (being.transform.Find("inventory") is Transform inventory && inventory != null) { Destroy(inventory.gameObject); }
        if (being.transform.Find("head") is Transform head && head != null) { Destroy(head.gameObject); }
        if (being.transform.Find("light") is Transform light && light != null) { Destroy(light.gameObject); }
        if (being.transform.Find("hacks") is Transform hacks && hacks != null) { Destroy(hacks.gameObject); }
        if (being.transform.Find("processor") is Transform processor && processor != null) { Destroy(processor.gameObject); }

        // 4 - HANDLE PHYSICS
        // we switch the rigidbody collision detection to discrete since the dead body won't move very fast (not affected by our forces)
        Rigidbody2D rb = being.GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        // we wait for a frame in order to the capacities to be destroyed & hover to be instanced
        yield return null;



        // 5 - TURNING TO CORPSE
        Corpse corpse = being.gameObject.AddComponent<Corpse>();
        corpse.name = "Corpse";
        corpse.Initialize(being);
        corpse.SetForces(being.GetForces());

        // we add a hover capacity to it (it is an interactable now)
        corpse.AddCapacity("hover");



        // 6 - DESTROYING OLD BEING & DIE CAPACITY
        Destroy(being);
        corpse.RemoveCapacity("die"); // and we finally remove the die capacity which will destroy it (this)
    }
}
