using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class KillBeingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("KillBeingCapability");

            builder.AddGoal<KillBeingGoal>()
                .AddCondition<BeingHealth>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<AttackAction>()
                .AddEffect<BeingHealth>(EffectType.Decrease)
                .SetTarget<ClosestBeing>()
                .SetStoppingDistance(0.5f);

            builder.AddTargetSensor<ClosestBeingSensor>()
                .SetTarget<ClosestBeing>();

            return builder.Build();
        }
    }
}