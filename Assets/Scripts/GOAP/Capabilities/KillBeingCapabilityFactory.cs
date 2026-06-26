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
                .AddCondition<BeingHealth>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(60);

            builder.AddAction<AttackAction>()
                .AddCondition<PreyIsInSameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<BeingHealth>(EffectType.Decrease)
                .SetTarget<ClosestBeing>()
                .SetStoppingDistance(0.5f);

            builder.AddTargetSensor<ClosestBeingSensor>()
                .SetTarget<ClosestBeing>();



            // GO TO NEXT ROOM + DOOR OPENING
            builder.AddAction<GoToNextRoomAction<KillBeingGoal>>()
                .AddCondition<PreyRoomIsAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<PreyIsInSameRoom>(EffectType.Increase)
                .SetTarget<PreyRoomTarget>();

            builder.AddAction<OpenDoorAction>()
                .AddEffect<PreyRoomIsAccessible>(EffectType.Increase)
                .SetTarget<DoorToGetToPrey>();

            builder.AddMultiSensor<GoToRoomSensor<PreyIsInSameRoom, PreyRoomIsAccessible, PreyRoomTarget, DoorToGetToPrey, KillBeingGoal>>();

            return builder.Build();
        }
    }

    public class PreyIsInSameRoom : WorldKeyBase { }
    public class PreyRoomIsAccessible : WorldKeyBase { }
    public class PreyRoomTarget : TargetKeyBase { }
    public class DoorToGetToPrey : TargetKeyBase { }
}