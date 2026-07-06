using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class DevTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("dev");

            factory.AddCapability<WanderCapabilityFactory>();
            // factory.AddCapability<EatCapabilityFactory>();
            // factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}