using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class WanderAction : IA_Action<WanderAction.Data>
    {
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // we first move with GoToBehaviour to the WanderTarget (depends on the MoveMode),
            // then when we arrive in range the AgentBehaviour will Start this action
            // and the Perform method will occur each frame
            // so we wait 2s at the wander target before completing the action
            return ActionRunState.WaitThenComplete(2f);
        }

        public class Data : IA_ActionData { }
    }
}