using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class SmellFoodCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("FoodCapability");

            builder.AddGoal<SmellFoodGoal>()
                .AddCondition<FoodReadyToBeEaten>(Comparison.GreaterThanOrEqual, 3);

            builder.AddAction<SmellFoodAction>()
                .AddEffect<FoodReadyToBeEaten>(EffectType.Increase)
                .SetTarget<ClosestFood>()
                .SetStoppingDistance(0.2f);

            builder.AddMultiSensor<FoodSensor>();

            return builder.Build();
        }
    }
}