using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class ZomboTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("zombo");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<EatCapabilityFactory>();

            return factory.Build();
        }
    }
}