using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class RobotFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("paf_1");

            factory.AddCapability<WanderCapabilityFactory>();

            return factory.Build();
        }
    }
}