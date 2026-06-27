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

            builder.AddAction<InteractAction<Item, CleanTrashGoal>>()
                .AddCondition<TrashSameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<TrashOnFloor>(EffectType.Decrease)
                .SetTarget<ClosestTrash>()
                .SetStoppingDistance(0.5f);

            builder.AddTargetSensor<ClosestTrashSensor>()
                .SetTarget<ClosestTrash>();

            builder.AddWorldSensor<TrashOnFloorSensor>()
                .SetKey<TrashOnFloor>();



            // GO TO NEXT ROOM + DOOR OPENING
            builder.AddAction<GoToNextRoomAction<CleanTrashGoal>>()
                .AddCondition<TrashRoomAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<TrashSameRoom>(EffectType.Increase)
                .SetTarget<TrashRoomTarget>();
                
            builder.AddAction<OpenDoorAction>()
                .AddEffect<TrashRoomAccessible>(EffectType.Increase)
                .SetTarget<TrashDoorTarget>();

            builder.AddMultiSensor<GoToRoomSensor<TrashSameRoom, TrashRoomAccessible, TrashRoomTarget, TrashDoorTarget, CleanTrashGoal>>();

            return builder.Build();
        }
    }
}