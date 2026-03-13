using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class RatTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("rat");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<EatCapabilityFactory>();
            factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}