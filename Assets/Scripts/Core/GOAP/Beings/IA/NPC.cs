using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using subrunner.goap;

public class NPC : IA
{

    // GOAL OVERRIDE METHODS
    /* public override void OnNoActionFound(IGoalRequest request)
    {
        check_if_trash_then_request_goal();
    }
    public override void OnActionEnd(IAction action)
    {
        check_if_trash_then_request_goal();
    }
    public override void OnGoalCompleted(IGoal goal)
    {
        check_if_trash_then_request_goal();
    }
    private void check_if_trash_then_request_goal()
    {
        if (CapableEngine.TrashEngine.GetQuantityOfTrash(LevelEngine.Instance.CurrentLevelID) <= 0)
        {
            GetCapacity<MotorCapacity>()?.RequestSuitedGoal();
            return;
        }

        GetCapacity<MotorCapacity>()?.RequestGoal<CleanTrashGoal>();
    } */
}
