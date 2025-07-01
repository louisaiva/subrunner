using UnityEngine;
using System.Collections.Generic;

public class EatMeat : Goal
{
    public override bool Doable
    {
        get
        {
            // if we already have meat is good !
            if (meat && meat.Eatable) { return true; }

            // else we try to find some meat
            detect_meat();

            // if we have a target meat then we can achieve this goal, otherwise noooo
            return meat != null;
        }
    }

    [Header("Actions Prefabs")]
    public GameObject goto_prefab; // action to go to somewhere
    public GameObject eat_meat_prefab; // action for eating meat


    [Header("Meat detection")]
    [SerializeField] protected Meat meat;
    public LayerMask meat_layers;
    protected ContactFilter2D contact_filter;
    protected CircleCollider2D circle_collider; // collider to detect meat

    // AWAKE
    private void Awake()
    {
        // checks if we have the required action prefab we need
        if (goto_prefab == null || eat_meat_prefab == null)
        {
            Debug.LogError("(EatMeat) " + name + " must have goto_prefab & eat_meat_prefab set in the inspector!");
        }

        // set up the contact filter
        contact_filter = new ContactFilter2D();
        contact_filter.SetLayerMask(meat_layers);
        contact_filter.useTriggers = true; // we want to detect triggers

        // get the circle collider
        circle_collider = GetComponent<CircleCollider2D>();
    }

    // PLANNING
    public override bool Plan()
    {
        // we need a meat to plan
        if (!Doable) { return false; }

        // we create a plan
        current_plan = new List<Action>();

        // check the distance between meat and the zombo
        if (Vector2.Distance(transform.position, meat.transform.position) > 0.4f)
        {
            // we are not close enough to eat the meat, so we need to go to it

            GoToAction goto_action = Instantiate(goto_prefab, transform).GetComponent<GoToAction>();
            // we set the meat of the goto action to the meat
            goto_action.destination = meat.transform.position;
            goto_action.threshold_distance = 0.4f;

            // we add the goto action to the plan
            current_plan.Add(goto_action);
        }

        // we set the eat meat action
        EatMeatAction eat_meat_action = Instantiate(eat_meat_prefab, transform).GetComponent<EatMeatAction>();
        eat_meat_action.capacity = "eat_meat"; // we set the capacity to eat
        eat_meat_action.animation_name = "eat"; // we set the animation name to eat
        eat_meat_action.target = meat; // we set the target to the meat

        // we add it to the plan
        current_plan.Add(eat_meat_action);

        return base.Plan();
    }
    private bool detect_meat()
    {
        // we check if our collider2D collides with a meat
        List<Collider2D> overlapping_colliders = new List<Collider2D>();
        circle_collider.Overlap(contact_filter, overlapping_colliders);

        // we filter the colliders to find Eatable Meat
        List<Meat> eatable_meats = new List<Meat>();
        eatable_meats.AddRange(overlapping_colliders.ConvertAll(collider => collider.transform.parent.GetComponent<Meat>()));
        eatable_meats.RemoveAll(meat => meat == null || !meat.Eatable);

        // sorts them by distance
        eatable_meats.Sort((a, b) =>
        {
            float distance1 = Vector2.Distance(transform.position, a.transform.position);
            float distance2 = Vector2.Distance(transform.position, b.transform.position);
            return distance1.CompareTo(distance2);
        });

        // check if we have some colliders overlapping
        if (eatable_meats.Count == 0) { return false; }

        // else we have a detected meat !!
        meat = eatable_meats[0];
        return true;
    }

    // UPDATE
    public override void UpdateGoal()
    {
        base.UpdateGoal();

        // check the distance between the meat and us
        float distance_to_meat = Vector2.Distance(transform.position, meat.transform.position);
        if (distance_to_meat > circle_collider.radius)
        {
            meat = null;
            return;
        }

        // checks if the meat is still Eatable
        if (!meat.Eatable)
        {
            if (debug) { Debug.LogWarning("(EatMeat) " + name + " detected meat is not Eatable anymore!"); }
            meat = null; // we reset the meat
            return;
        }

        // we update the GoToAction destination if we still have a meat in sight !
        if (current_action is GoToAction goto_action)
        {
            goto_action.destination = meat.transform.position;
        }

        // if we don't have any action in the plan it means we did everything (going to meat & eating it), so we replan
        else if (!current_action) { Plan(); }
    }

}