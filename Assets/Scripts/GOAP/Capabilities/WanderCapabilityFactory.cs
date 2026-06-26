using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class WanderCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("WanderCapability");

            builder.AddGoal<WanderGoal>()
                .AddCondition<IsWandering>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(100);

            builder.AddAction<WanderAction>()
                .AddCondition<SameRoom>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<IsWandering>(EffectType.Increase)
                .SetTarget<WanderTarget>();

            builder.AddTargetSensor<WanderTargetSensor>()
                .SetTarget<WanderTarget>();

            builder.AddAction<GoToNextRoomAction<WanderGoal>>()
                .AddCondition<NextRoomAccessible>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<SameRoom>(EffectType.Increase)
                .SetTarget<RoomTarget>();

            builder.AddAction<OpenDoorAction>()
                .AddEffect<NextRoomAccessible>(EffectType.Increase)
                .SetTarget<DoorTarget>();

            builder.AddMultiSensor<GoToRoomSensor<SameRoom,NextRoomAccessible, RoomTarget, DoorTarget, WanderGoal>>();

            return builder.Build();
        }
    }
}