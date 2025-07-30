using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    // [GoapId("Idle-9b53930e-6c9f-44ee-8788-2b7c75333941")]
    public class WanderAction : GoapActionBase<WanderAction.Data>
    {
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // we first move with GoToBehaviour to the WanderTarget (depends on the MoveMode),
            // then when we arrive in range the AgentBehaviour will Start this action
            // and the Perform method will occur each frame
            // so we wait 2s at the wander target before completing the action
            return ActionRunState.WaitThenComplete(2f);
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
        }
    }
}