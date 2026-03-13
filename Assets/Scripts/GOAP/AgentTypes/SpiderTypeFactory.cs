using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class SpiderFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("spider");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<EatCapabilityFactory>();
            factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}