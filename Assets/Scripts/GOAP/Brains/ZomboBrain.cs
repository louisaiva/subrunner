using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ZomboBrain : Brain
    {
        private void Start()
        {
            this.provider.RequestGoal<WanderGoal, EatGoal>();
        }
    }
}