
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
    public bool destroy_object = true;
    // public float time_before_disappearing = 60f;
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

    // trigger the dying
    public override void Use(Capable capable)
    {
        // check if the capable is a Being
        if (capable is not Being) { return; }

        // we play the animation
        Anim anim = capable.anim_player.Play(name);
        if (anim == null) { return; }

        // on meurt
        deaths += 1;

        // on donne de l'xp
        Vector3 sprite_center = new Vector3(transform.position.x, transform.position.y + capable.GetComponent<SpriteRenderer>().bounds.size.y / 2f, 0);
        xp_provider.GetComponent<XPProvider>().EmitXP(xp_gift, sprite_center);

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
        // Invoke(nameof(destroyObject), time_before_disappearing);
        StartCoroutine(destroyObject());
    }

    private IEnumerator destroyObject()
    {
        // get the being
        Being being = capable as Being;

        // on change le layer du perso en "meat"
        being.body_collider.gameObject.layer = LayerMask.NameToLayer("Meat");

        // si c'est le perso on attend 3000s
        if (being is Perso)
        {
            (being as Perso).Die();
            yield return new WaitForSeconds(3000f);
        }

        // destroy the object if the parameter is set
        if (destroy_object)
        {
            Destroy(transform.parent.gameObject);
            if (debug) { Debug.Log("Destroying " + being.name); }
            yield break;
        }

        // else we just disable the being, including all capacities & etc
        if (debug) { Debug.Log("Destroying capacities of " + being.name); }

        // we destroy all capacities (except DieCapacity FOR NOW)
        List<Capacity> capacities = new List<Capacity>(being.GetCapacities());
        capacities.RemoveAll(capa => capa.name == "die");
        while (capacities.Count > 0)
        {
            being.RemoveCapacity(capacities[0].name);
            capacities.RemoveAt(0);
        }

        // we also destroy all Goal if this is an IA
        if (being is IA ia)
        {
            if (debug) { Debug.Log("Destroying goals of " + being.name); }
            List<GameObject> goal_objects = new List<GameObject>(ia.goals.ConvertAll(goal => goal.gameObject));
            while (goal_objects.Count > 0)
            {
                Destroy(goal_objects[0]);
                goal_objects.RemoveAt(0);
            }
        }

        // And finally we add a Meat that will replace the being
        // List<Force> forces = new List<Force>(being.GetForces()); // we save the current forces of the capable
        Meat meat = being.gameObject.AddComponent<Meat>();
        meat.SetForces(being.GetForces()); // we set the forces back to the Meat

        // we destroy the old being component
        Destroy(being);

        // and we destroy ourselves (the DieCapacity)
        Destroy(this.gameObject);
    }
}
