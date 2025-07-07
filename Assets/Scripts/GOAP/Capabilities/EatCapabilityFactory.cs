using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class EatCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("FoodCapability");

            builder.AddGoal<EatGoal>()
                .AddCondition<FoodReadyToBeEaten>(Comparison.GreaterThanOrEqual, 3);

            builder.AddAction<EatAction>()
                .AddEffect<FoodReadyToBeEaten>(EffectType.Increase)
                .SetTarget<ClosestFood>()
                .SetStoppingDistance(0.2f);

            builder.AddMultiSensor<FoodSensor>();

            return builder.Build();
        }
    }
}