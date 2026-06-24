using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class NPCTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = CreateBuilder("npc");

            factory.AddCapability<CleanTrashCapabilityFactory>();
            factory.AddCapability<WanderCapabilityFactory>();
            // factory.AddCapability<EatCapabilityFactory>();
            // factory.AddCapability<KillBeingCapabilityFactory>();

            return factory.Build();
        }
    }
}