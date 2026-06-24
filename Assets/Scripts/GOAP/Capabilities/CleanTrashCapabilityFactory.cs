using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class CleanTrashCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("CleanTrashCapability");

            builder.AddGoal<CleanTrashGoal>()
                .AddCondition<TrashOnFloor>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(10);

            builder.AddAction<InteractAction<Item>>()
                .AddEffect<TrashOnFloor>(EffectType.Decrease)
                .SetTarget<ClosestTrash>()
                .SetStoppingDistance(0.3f);

            builder.AddTargetSensor<ClosestTrashSensor>()
                .SetTarget<ClosestTrash>();

            builder.AddWorldSensor<TrashOnFloorSensor>()
                .SetKey<TrashOnFloor>();

            return builder.Build();
        }
    }
}