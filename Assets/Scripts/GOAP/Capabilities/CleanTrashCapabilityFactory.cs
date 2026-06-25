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
                .AddCondition<SameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<TrashOnFloor>(EffectType.Decrease)
                .SetTarget<ClosestTrash>()
                .SetStoppingDistance(0.3f);

            builder.AddTargetSensor<ClosestTrashSensor>()
                .SetTarget<ClosestTrash>();

            builder.AddWorldSensor<TrashOnFloorSensor>()
                .SetKey<TrashOnFloor>();



            // GO TO NEXT ROOM + DOOR OPENING
            builder.AddAction<GoToNextRoomAction>()
                .AddCondition<NextRoomAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<SameRoom>(EffectType.Increase)
                .SetTarget<RoomTarget>();

            builder.AddAction<InteractAction<Door>>()
                .AddEffect<NextRoomAccessible>(EffectType.Increase)
                .SetTarget<DoorTarget>();

            builder.AddMultiSensor<GoToRoomSensor>();

            return builder.Build();
        }
    }
}