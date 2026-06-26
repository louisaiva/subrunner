using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class GoToRoomSensor : GoToRoomSensor<WorldKeyBase, WorldKeyBase, TargetKeyBase, TargetKeyBase, IGoal>
    {
        public static string GetNextRoomAlongPath(string start, string dest, out DoorData door)
        {
            door = null;
            if (string.Equals(start, dest)) { return start; }
            door = RoomEngine.DoorEngine.GetNextDoorAlongPath(start, dest);
            if (door == null)
            {
                Debug.LogError($"(GoToRoomSensor) Could not find the next door data along path between {start} & {dest}");
                return null;
            }

            // we get the room id that is NOT current_room
            return door.room1_id == start ?
                            door.room2_id :
                            door.room1_id;
        }
    }

    public class GoToRoomSensor<SameT,AccessibleT, RoomT, DoorT, GoalT> : MultiSensorBase
        where SameT : WorldKeyBase
        where AccessibleT : WorldKeyBase
        where RoomT : TargetKeyBase
        where DoorT : TargetKeyBase
        where GoalT : IGoal
    {
        // protected virtual System.Type GoalType => typeof(/* IGoal */ WanderGoal); // set wander goal as default
        private static bool log_created = true;


        // The Created method is called when the sensor is created
        // This can be used to gather references to objects in the scene
        public override void Created()
        {
            if (!log_created) { return; }

            Debug.Log($"(GoToRoomSensor) Created a MultiSensor {this.GetType().GetFriendlyName()} : "
                + $"     SameT : '{typeof(SameT).Name}'     "
                + $"///     AccessibleT : '{typeof(AccessibleT).Name}'     "
                + $"///     RoomT : '{typeof(RoomT).Name}'     "
                + $"///     DoorT : '{typeof(DoorT).Name}'     "
                + $"///     GoalT : '{typeof(GoalT).Name}'     "
            );
        }


        public GoToRoomSensor()
        {
            this.AddLocalWorldSensor<SameT>((agent, references) =>
            {
                if (!extract_current_and_destination_room(references, out string room, out string destination, "is same room")) { return 0; }
                if (string.IsNullOrEmpty(destination)) { return 1; } // we have no destination, we suggest we are in same room
                return room == destination ? 1 : 0;
            });



            this.AddLocalWorldSensor<AccessibleT>((agent, references) =>
            {
                if (!extract_current_and_destination_room(references, out string room, out string destination, "accessible")) { return 0; }
                if (string.IsNullOrEmpty(destination)) { return 1; } // we have no destination, we suggest we are in same room
                if (string.Equals(room,destination)) { return 1; }
                if (RoomEngine.DoorEngine.IsNextRoomAccessibleAlongPath(room, destination)) { return 1; }
                return 0;
            });



            this.AddLocalTargetSensor<RoomT>((agent, references, target) =>
            {
                if (!extract_current_and_destination_room(references, out string current_room, out string destination, "get next room")) { return null; }
                if (string.IsNullOrEmpty(destination))
                {
                    if (Logger.LazyInstance.LOG_GTR_SENSOR)
                    {
                        Debug.LogWarning($"(GoToRoomSensor) No destination found !! Can't set next room target of type {typeof(RoomT).Name} :///");
                    }
                    return null;
                }
                if (string.Equals(current_room, destination))
                {
                    if (Logger.LazyInstance.LOG_GTR_SENSOR)
                    {
                        Debug.LogWarning($"(GoToRoomSensor) Already in destination room ! Can't set next room target of type {typeof(RoomT).Name} :///");
                    }
                    return null;
                }
                
                string other_room_id = GoToRoomSensor.GetNextRoomAlongPath(current_room, destination, out DoorData next_door);
                if (next_door == null) { return null; }
                if (!next_door.GetPositionInsideRoom(other_room_id, out Vector2 position)) { return null; }

                if (Logger.LazyInstance.LOG_GTR_SENSOR)
                {
                    Debug.Log($"(GoToRoomSensor) Get Next Room Sensor successfully found the position of the next room to go ! moving along {current_room} --> {other_room_id}");
                }

                // we successfully found the position of the door that is slightly in the next room !
                if (target is PositionTarget pos_target)
                {
                    return pos_target.SetPosition(position);
                }
                return new PositionTarget(position);
            });

            this.AddLocalTargetSensor<DoorT>((agent, references, target) =>
            {
                // if the next room is not accessible we need to Interact with the door, so we
                // first need to target the position of the door in the current room
                if (!extract_current_and_destination_room(references, out string current_room, out string destination, "get door")) { return null; }
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

        // This method is equal to the Update method of a local sensor.
        // It can be used to cache data, like gathering a list of all pears in the scene.
        public override void Update() { }
        private bool extract_current_and_destination_room(IComponentReference references, out string current_room, out string destination, string sensor)
        {
            // Get a cached reference to the DataBehaviour on the agent
            MotorCapacity mc = references.GetCachedComponent<MotorCapacity>();
            if (mc == null || mc.mdata == null) { current_room = ""; destination = ""; return false; }
            bool found = mc.mdata.ExtractCurrentAndDestinationRooms(typeof(GoalT), out current_room, out destination);
            if (found)
            {
                Debug.Log($"(GoToRoomSensor - {typeof(GoalT).GetFriendlyName()}) Sensor '{sensor}' extracted rooms : '{current_room}' -> '{destination}'");
                return true;
            }
            Debug.LogWarning($"(GoToRoomSensor - {typeof(GoalT).GetFriendlyName()}) Sensor '{sensor}' COULD NOT extract rooms : '{current_room}' -> '{destination}'");
            return false;
        }
    }
}