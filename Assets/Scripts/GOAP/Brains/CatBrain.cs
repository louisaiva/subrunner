using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class CatBrain : Brain
    {
        private void Start()
        {
            this.provider.RequestGoal<WanderGoal, EatGoal, KillBeingGoal>();
        }
    }
}