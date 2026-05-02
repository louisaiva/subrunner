using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class WaitCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("WaitCapability");

            builder.AddGoal<WaitGoal>()
                .AddCondition<IsWaiting>(Comparison.GreaterThanOrEqual, 1)
                /* .SetBaseCost(2) */;

            builder.AddAction<WaitAction>()
                .AddEffect<IsWaiting>(EffectType.Increase);

            return builder.Build();
        }
    }
}