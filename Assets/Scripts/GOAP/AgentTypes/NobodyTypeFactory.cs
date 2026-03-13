using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class NobodyTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("nobody");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}