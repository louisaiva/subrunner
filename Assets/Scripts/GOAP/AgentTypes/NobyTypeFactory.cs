using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class NobyTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("noby");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}