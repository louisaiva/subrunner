using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace subrunner.goap
{
    public class DependencyInjector : GoapConfigInitializerBase, IGoapInjector
    {
        public override void InitConfig(IGoapConfig config)
        {
            config.GoapInjector = this;
        }

        public void Inject(IAction action)
        {
            if (action is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        public void Inject(IGoal goal)
        {
            if (goal is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        public void Inject(ISensor sensor)
        {
            if (sensor is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }
    }

    public interface IInjectable
    {
        void Inject(DependencyInjector injector);
    }
}