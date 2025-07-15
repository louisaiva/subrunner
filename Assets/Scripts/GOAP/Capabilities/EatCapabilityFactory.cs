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
                .AddCondition<Hunger>(Comparison.SmallerThanOrEqual, 20);

            builder.AddAction<EatAction>()
                .AddEffect<Hunger>(EffectType.Decrease)
                .SetTarget<ClosestFood>()
                .SetStoppingDistance(0.3f);

            builder.AddTargetSensor<ClosestFoodSensor>()
                .SetTarget<ClosestFood>();

            builder.AddWorldSensor<HungerSensor>()
                .SetKey<Hunger>();

            // builder.AddMultiSensor<FoodSensor>();

            return builder.Build();
        }
    }
}