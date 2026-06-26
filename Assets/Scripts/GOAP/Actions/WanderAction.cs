using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class WanderAction : IA_Action<IA_ActionData>
    {
        public override IActionRunState Perform(IMonoAgent agent, IA_ActionData data, IActionContext context)
        {
            // we first move with GoToBehaviour to the WanderTarget (depends on the MoveMode),
            // then when we arrive in range the AgentBehaviour will Start this action
            // and the Perform method will occur each frame
            // so we wait 2s at the wander target before completing the action
            return ActionRunState.WaitThenComplete(2f);
        }
        public override void End(IMonoAgent agent, IA_ActionData data)
        {
            // here we can reset the room destination in the motor data !
            if (data.ia.TryGetCapacity(out MotorCapacity mc))
            {
                mc.mdata.ClearDestination(typeof(WanderGoal));
            }
        }
    }
}