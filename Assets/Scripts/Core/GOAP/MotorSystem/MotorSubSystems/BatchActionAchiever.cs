using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;

public class BatchActionAchiever : MonoBehaviour
{


    [Header("Actions Data")]
    private Dictionary<string, EntityAction> actions = new Dictionary<string, EntityAction>();


    [Header("Components")]
    private BatchActionProvider _provider;
    public BatchActionProvider Provider
    {
        get
        {
            if (_provider == null) { _provider = GetComponent<BatchActionProvider>(); }
            return _provider;
        }
    }




    public void SetAction(EntityMotor entity, IAction action, ITarget target)
    {
        if (!actions.ContainsKey(entity.ID))
        {
            actions.Add(entity.ID, new EntityAction { entity = entity });
        }
        EntityAction entityAction = actions[entity.ID];

        // we stop the current action
        if (entityAction.State.Action != null)
        {
            StopAction(entity, false);
        }

        // we assign new action things we need
        var data = action.GetData();
        data.Target = target;
        entityAction.State.SetAction(action, data);
        entityAction.AgentState = AgentState.StartingAction;

        // action.Start(this, data); // todo here we start the action

        entityAction.Events.ActionStart(action);
    }
    public void StopAction(EntityMotor entity, bool resolveAction = true)
    {
        if (!actions.TryGetValue(entity.ID, out EntityAction entityAction)) { return; }

        var action = entityAction.State.Action;

        // action?.Stop(entityAction.State, entityAction.State.Data); // ! no mono agent, how do we do this ?
        entityAction.State.Reset();
        entityAction.Events.ActionStop(action);

        if (resolveAction) { Provider.RegisterForResolve(entity); }
    }
}

public class EntityAction
{
    public EntityMotor entity;
    public IActionState State { get; private set; } = new ActionState();
    public AgentState AgentState { get; set; } = AgentState.NoAction;
    public AgentMoveState MoveState { get; set; } = AgentMoveState.Idle;
    
    
    // ? is it the best ? don't we need to fire events with EntityMotor so we can retrieve the EntityAction ?
    public IAgentEvents Events { get; private set; } = new AgentEvents();
}