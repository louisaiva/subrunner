using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class ReturnToNestCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("ReturnToNestCapability");

            builder.AddGoal<ReturnToNestGoal>()
                .AddCondition<HasTrash>(Comparison.SmallerThanOrEqual, 0)
                .AddCondition<IsInNest>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(20);

            builder.AddAction<ReturnToNestAction>()
                .AddCondition<NestIsInSameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<IsInNest>(EffectType.Increase)
                .SetTarget<ClosestNest>()
                .SetStoppingDistance(0.5f);

            builder.AddTargetSensor<ClosestSpecNestSensor>() // todo change this to a specific target sensor that filter nests based on species
                .SetTarget<ClosestNest>();


            // GO TO NEXT ROOM + DOOR OPENING
            builder.AddAction<GoToNextRoomAction<ReturnToNestGoal>>()
                .AddCondition<NestRoomIsAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<NestIsInSameRoom>(EffectType.Increase)
                .SetTarget<NestRoomTarget>();

            builder.AddAction<OpenDoorAction>()
                .AddEffect<NestRoomIsAccessible>(EffectType.Increase)
                .SetTarget<DoorToGetToNest>();

            builder.AddMultiSensor<GoToRoomSensor<NestIsInSameRoom, NestRoomIsAccessible, NestRoomTarget, DoorToGetToNest, ReturnToNestGoal>>();

            return builder.Build();
        }
    }


    // GOAL + TARGETS
    public class ReturnToNestGoal : GoalBase { }
    public class IsInNest : WorldKeyBase { }
    public class ClosestNest : TargetKeyBase { }


    // GTR
    public class NestIsInSameRoom : WorldKeyBase { }
    public class NestRoomIsAccessible : WorldKeyBase { }
    public class NestRoomTarget : TargetKeyBase { }
    public class DoorToGetToNest : TargetKeyBase { }
}