using System;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class GoToNextRoomAction<GoalT> : IA_Action<GoToNextRoomAction<GoalT>.Data> where GoalT : IGoal
    {
        // DATA
        public class Data : IA_ActionData
        {
            // here we can store the room link
            public DoorEngine.RoomLink link;
            public Action<DoorEngine.RoomLink> link_changed_callback;
        }

        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);

            // get the room link
            if (!data.ia.TryGetCapacity(out MotorCapacity mc)) { return; } // weird but ok why not ? should we directly end the action ?
            if (!mc.mdata.ExtractCurrentAndDestinationRooms(typeof(GoalT), out string start, out string dest)) { return; }
            if (Logger.LazyInstance.LOG_GTR_ACTION) { Debug.Log($"(GoToNextRoomAction) '{data.ia.ID}' - Is starting going to next room :     {start} ---> {dest}"); }
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(dest) || string.Equals(start,dest)) { return; }

            // get the room link so we can continuously check its state in Perform
            data.link = RoomEngine.DoorEngine.GetLinkBetweenRooms(start, dest);
            if (data.link == null)
            {
                if (Logger.LazyInstance.LOG_GTR_ACTION) { Debug.LogWarning($"(GoToNextRoomAction) '{data.ia.ID}' - Could not find a link between rooms apparently..."); }
                Stop(agent, data); return;
            }
            data.link_changed_callback = (ctx) => on_link_state_updated(agent, data);
            data.link.OnStateChanged += data.link_changed_callback;
            if (Logger.LazyInstance.LOG_GTR_ACTION) { Debug.Log($"(GoToNextRoomAction) '{data.ia.ID}' - callback set properly"); }
        }
        
        // update link state
        private void on_link_state_updated(IMonoAgent agent, Data data)
        {
            if (data.link == null) { Stop(agent,data); return; }
            if (Logger.LazyInstance.LOG_GTR_ACTION) { Debug.Log($"(GoToNextRoomAction) '{data.ia.ID}' - Link swapped to state : " + data.link.State); }
            if (!data.link.IsOpen) { Stop(agent, data); return; } // can't walk through link if it is not open
        }

        // PERFORM
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.Completed;
        }

        // end
        public override void End(IMonoAgent agent, Data data)
        {
            // remove link callback if set
            if (data.link == null) { return; }
            if (data.link_changed_callback == null) { return; }
            data.link.OnStateChanged -= data.link_changed_callback;
            data.link_changed_callback = null;
            if (Logger.LazyInstance.LOG_GTR_ACTION) { Debug.Log($"(GoToNextRoomAction) '{(data.ia == null ? "null" : data.ia.ID)}' - callback removed properly"); }
        }
    }
}