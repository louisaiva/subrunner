using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class BurnTrashCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("BurnTrashCapability");

            builder.AddGoal<BurnTrashGoal>()
                .AddCondition<HasTrash>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(12);

            builder.AddAction<BurnTrashAction>()
                .AddCondition<BurnerIsInSameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<HasTrash>(EffectType.Decrease)
                .SetTarget<ClosestBurner>()
                .SetStoppingDistance(0.5f);

            builder.AddTargetSensor<ClosestSensor<Burner, BurnTrashGoal>>()
                .SetTarget<ClosestBurner>();

            builder.AddWorldSensor<HasTrashSensor>()
                .SetKey<HasTrash>();



            // GO TO NEXT ROOM + DOOR OPENING
            builder.AddAction<GoToNextRoomAction<BurnTrashGoal>>()
                .AddCondition<BurnerRoomIsAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<BurnerIsInSameRoom>(EffectType.Increase)
                .SetTarget<BurnerRoomTarget>();

            builder.AddAction<OpenDoorAction>()
                .AddEffect<BurnerRoomIsAccessible>(EffectType.Increase)
                .SetTarget<DoorToGetToBurner>();

            builder.AddMultiSensor<GoToRoomSensor<BurnerIsInSameRoom, BurnerRoomIsAccessible, BurnerRoomTarget, DoorToGetToBurner, BurnTrashGoal>>();

            return builder.Build();
        }
    }


    // GOAL + TARGETS
    public class BurnTrashGoal : GoalBase { }
    public class HasTrash : WorldKeyBase { }
    public class ClosestBurner : TargetKeyBase { }


    // GTR
    public class BurnerIsInSameRoom : WorldKeyBase { }
    public class BurnerRoomIsAccessible : WorldKeyBase { }
    public class BurnerRoomTarget : TargetKeyBase { }
    public class DoorToGetToBurner : TargetKeyBase { }
}