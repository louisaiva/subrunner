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
            public string next_room;
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
            data.next_room = GoToRoomSensor.GetNextRoomAlongPath(start, dest, out DoorData door);

            // get the room link so we can continuously check its state in Perform
            data.link = RoomEngine.DoorEngine.GetLinkBetweenRooms(start, data.next_room);
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
            // check if we finally are in the good room !
            string real_room = data.ia.data.room;
            if (!string.IsNullOrEmpty(data.next_room) && real_room == data.next_room) { return ActionRunState.Completed; }

            Debug.LogError($"(GoToNextRoomAction) '{data.ia.ID}' travelled to the next room but is still not in the room {data.next_room} ??? It is still in {real_room}");
            return ActionRunState.WaitThenStop(1f);
        }

        // end
        public override void End(IMonoAgent agent, Data data)
        {
            clear_link_callbacks(data);

            if (string.IsNullOrEmpty(data.next_room)) { return; }
            if (!data.ia.TryGetCapacity(out MotorCapacity mc)) { return; }
            if (!mc.mdata.TryGetDestination(typeof(GoalT), out string destination)) { return; }
            if (!string.Equals(destination, data.next_room)) { return; }

            // we clear the destination if we successfully travelled until room destination !
            mc.mdata.ClearDestination(typeof(GoalT));
        }
        public void clear_link_callbacks(Data data)
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