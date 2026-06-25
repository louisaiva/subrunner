using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class GoToRoomSensor : MultiSensorBase
    {
        
        public GoToRoomSensor()
        {
            this.AddLocalWorldSensor<SameRoom>((agent, references) =>
            {
                if (!extract_current_and_destination_room(references, out string room, out string destination)) { return 0; }
                if (string.IsNullOrEmpty(destination)) { return 1; } // we have no destination, we suggest we are in same room
                return room == destination ? 1 : 0;
            });

            this.AddLocalWorldSensor<NextRoomAccessible>((agent, references) =>
            {
                if (!extract_current_and_destination_room(references, out string room, out string destination)) { return 0; }
                if (string.IsNullOrEmpty(destination)) { return 1; } // we have no destination, we suggest we are in same room
                if (string.Equals(room,destination)) { return 1; }
                if (RoomEngine.DoorEngine.IsNextRoomAccessibleAlongPath(room, destination)) { return 1; }
                return 0;
            });

            this.AddLocalTargetSensor<RoomTarget>((agent, references, target) =>
            {
                if (!extract_current_and_destination_room(references, out string current_room, out string destination)) { return null; }
                if (string.IsNullOrEmpty(destination)) { return null; } // we have no destination, we suggest we are in same room
                if (string.Equals(current_room, destination)) { return null; }
                DoorData next_door = RoomEngine.DoorEngine.GetNextDoorAlongPath(current_room, destination);
                if (next_door == null) { return null; }

                // we get the room id that is NOT current_room
                string other_room_id = next_door.room1_id == current_room ?
                                                next_door.room2_id :
                                                next_door.room1_id;

                if (!next_door.GetPositionInsideRoom(other_room_id, out Vector2 position)) { return null; }

                // we successfully found the position of the door that is slightly in the next room !
                if (target is PositionTarget pos_target)
                {
                    return pos_target.SetPosition(position);
                }
                return new PositionTarget(position);
            });

            this.AddLocalTargetSensor<DoorTarget>((agent, references, target) =>
            {
                // if the next room is not accessible we need to Interact with the door, so we
                // first need to target the position of the door in the current room
                if (!extract_current_and_destination_room(references, out string current_room, out string destination)) { return null; }
                if (string.IsNullOrEmpty(destination)) { return null; } // we have no destination, we suggest we are in same room
                if (string.Equals(current_room, destination)) { return null; }
                DoorData next_door = RoomEngine.DoorEngine.GetNextDoorAlongPath(current_room, destination);
                if (next_door == null) { return null; }

                if (target is CapableTarget captarg)
                {
                    return captarg.SetCapableData(next_door);
                }
                return new CapableTarget(next_door);
            });
        }

        // The Created method is called when the sensor is created
        // This can be used to gather references to objects in the scene
        public override void Created() { }

        // This method is equal to the Update method of a local sensor.
        // It can be used to cache data, like gathering a list of all pears in the scene.
        public override void Update() { }



        private bool extract_current_and_destination_room(IComponentReference references, out string current_room, out string destination)
        {
            current_room = "";
            destination = "";

            if (RoomEngine.DoorEngine == null) { return false; }

            // Get a cached reference to the DataBehaviour on the agent
            MotorCapacity mc = references.GetCachedComponent<MotorCapacity>();
            if (mc == null || mc.mdata == null || mc.IA == null) { return false; }

            // feed the current room
            mc.mdata.current_room_id = mc.IA.GetRealRoom();
            if (string.IsNullOrEmpty(mc.mdata.current_room_id)) { return false; }
            current_room = mc.mdata.current_room_id;
            if (string.IsNullOrEmpty(mc.mdata.destination_room_id)) { return true; } // no destination, so we are good :D
            destination = mc.mdata.destination_room_id;
            return true;
        }
    }
}