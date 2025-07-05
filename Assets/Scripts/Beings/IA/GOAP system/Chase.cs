using System.Collections.Generic;
using UnityEngine;

public class Chase : Goal
{
    [Header("Chase Goal")]
    [SerializeField] private Transform target;
    [SerializeField] private GameObject action_prefab;

    public override bool Doable
    {
        get
        {
            return target != null;
        }
    }

    // PLANNING
    public override bool Plan()
    {
        if (!Doable) { return false; }

        // we instantiate the action prefab
        GoToAstar action = Instantiate(action_prefab, transform).GetComponent<GoToAstar>();
        action.target = target;

        // we start
        action.Start();

        // we create the plan
        current_plan = new List<Action>() { action };

        return base.Plan();
    }
}