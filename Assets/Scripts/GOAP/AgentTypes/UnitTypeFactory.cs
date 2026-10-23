using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class UnitTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("usb");

            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}