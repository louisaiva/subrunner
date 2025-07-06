using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class CatTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("cat");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<SmellFoodCapabilityFactory>();

            return factory.Build();
        }
    }
}