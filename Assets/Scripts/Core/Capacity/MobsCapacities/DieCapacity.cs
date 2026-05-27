
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DieCapacity is a capacity that allows a being to die..
/// gives Immobile for eternity & changes the Layer to "Meat"
/// </summary>

[Obsolete("DieCapacity is deprecated. we must make a DieEngine capacity subsystem instead")]
public class DieCapacity : Capacity
{
    public int deaths = 0;

    [Header("XP parameters")]
    public int xp_gift = 10;

    [Header("Die parameters")]
    [SerializeField] private bool show_smiley = true;
    [SerializeField] private List<string> smileys = new List<string> { "RIP", "rip", ";-;", ":(", "://" };


    // trigger the dying
    public override void Use(Capable capable)
    {
        // check if the capable is a Being
        // if (capable is not Being) { return; }

        // we play the animation
        Anim anim = capable.AnimPlayer.Play(name);
        if (anim == null) { return; }

        // on meurt
        deaths += 1;

        // on donne de l'xp
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + capable.AnimPlayer.Renderer.bounds.size.y / 2f, 0);
        int xp_to_drop = xp_gift;
        if (capable is Perso perso) { xp_to_drop = perso.total_xp/2; } // if the player dies he drops half of his total xp
        XPProvider.Instance.EmitXP(xp_to_drop, sprite_center);

        // on donne un floating dmg
        if (show_smiley)
        {
            float test = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < smileys.Count; i++)
            {
                if (test < 100 / smileys.Count * (i + 1))
                {
                    FloatingDmgProvider.Instance.TextManager.addFloatingText(smileys[i], sprite_center, "red");
                    break;
                }
            }
        }

        // destroy object
        if (Capable.HasCapacity<HealthCapacity>()) { StartCoroutine(destroyHealthCapaBeing()); }
    }
    private IEnumerator destroyHealthCapaBeing()
    {
        // get the being
        HealthCapacity health = Capable.GetCapacity<HealthCapacity>();
        health.HealthCollider.gameObject.layer = LayerMask.NameToLayer("Meat");

        // 1 - DROP ITEMS
        if (Capable.Inventory != null && Capable.Inventory.Count > 0)
        {
            Capable.DropAllItems(); // we wait for dropping all items
        }

        // health.Die();

        // 2 - DESTROYING CAPACITIES
        if (log) { Debug.Log("Destroying capacities of " + Capable.name); }

        // we destroy all capacities (except DieCapacity FOR NOW)
        List<Capacity> capacities = new List<Capacity>(Capable.GetCapacities());
        capacities.RemoveAll(capa => capa.name == "die");
        /* while (capacities.Count > 0)
        {
            Capable.RemoveCapacity(capacities[0].name);
            capacities.RemoveAt(0);
        } */


        // 3 - DESTROYING OTHER ELEMENTS
        if (Capable.transform.Find("brain") is Transform brain && brain != null) { Destroy(brain.gameObject); }
        if (Capable.transform.Find("goals") is Transform goal && goal != null) { Destroy(goal.gameObject); }
        if (Capable.transform.Find("eyes") is Transform eyes && eyes != null) { Destroy(eyes.gameObject); }
        // if (Capable.transform.Find("inventory") is Transform inventory && inventory != null) { Destroy(inventory.gameObject); }
        if (Capable.transform.Find("head") is Transform head && head != null) { Destroy(head.gameObject); }
        if (Capable.transform.Find("light") is Transform light && light != null) { Destroy(light.gameObject); }
        if (Capable.transform.Find("hacks") is Transform hacks && hacks != null) { Destroy(hacks.gameObject); }
        if (Capable.transform.Find("processor") is Transform processor && processor != null) { Destroy(processor.gameObject); }

        // 4 - HANDLE PHYSICS
        // we switch the rigidbody collision detection to discrete since the dead body won't move very fast (not affected by our forces)
        Rigidbody2D rb = Capable.GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        // we wait for a frame in order to the capacities to be destroyed & hover to be instanced
        yield return null;



        // 5 - TURNING TO CORPSE
        Corpse corpse = Capable.gameObject.AddComponent<Corpse>();
        corpse.name = "Corpse";
        corpse.Initialize(Capable);
        if (Capable is Movable movable) { corpse.SetForces(movable.GetForces()); }

        // we add a hover capacity to it (it is an interactable now)
        // corpse.AddCapacity("hover");



        // 6 - DESTROYING OLD BEING & DIE CAPACITY
        Destroy(health);
        // corpse.RemoveCapacity("die"); // and we finally remove the die capacity which will destroy it (this)
    }
}
