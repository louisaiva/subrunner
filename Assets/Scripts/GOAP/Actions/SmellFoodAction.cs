using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    [GoapId("SmellFood-898315a9-1d95-4bd3-997f-48gggdda03cc")]
    public class SmellFoodAction : GoapActionBase<SmellFoodAction.Data>
    {
        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.WaitThenComplete(0.5f);
        }

        // This method is called when the action is completed
        public override void Complete(IMonoAgent agent, Data data)
        {
            if (data.Target is not TransformTarget transformTarget)
                return;

            (data.Brain.ia as Cat).food_ready_to_be_eaten = true;
            GameObject.Destroy(transformTarget.Transform.gameObject); // todo delete this and put it in EatAction
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            // When using the GetComponent attribute, the system will automatically inject the reference
            [GetComponent] public Brain Brain { get; set; }
        }
    }
}