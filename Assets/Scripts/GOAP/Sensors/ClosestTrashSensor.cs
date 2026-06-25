using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ClosestTrashSensor : LocalTargetSensorBase
    {

        private static TrashDetector _detector;
        private static TrashDetector detector
        {
            get
            {
                if (_detector == null)
                {
                    _detector = GameObject.FindFirstObjectByType<WorldTrashDetector>();
                }
                return _detector;
            }
        }

        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            // Get a cached reference to the IA on the agent & Capacity
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia.JustLoaded)
            {
                // we check that we have a valid target, if yes we return it (it was loaded when the MotorCapacity loaded the MotorData' local world data)
                if (target != null && target is CapableTarget)
                {
                    if (Logger.LazyInstance.LOG_CLOSEST_TRASH_SENSOR) { Debug.Log($"(ClosestFoodSensor - Sense) {ia.data.id} just loaded and has an existing target : {target}. We keep it."); }
                    return target;
                }
            }

            // ensure that the agent has no left actions in the plan
            if (!ia.TryGetCapacity(out MotorCapacity mc)) { return target; }
            // Debug.Log($"(WanderLoadedSensor - Sense) {ia.data.id} has current plan :      {mc.GetPendingActionsDetails()}");
            if (mc.StillHasPendingActions())
            {
                if (Logger.LazyInstance.LOG_CLOSEST_TRASH_SENSOR) { Debug.Log($"(ClosestFoodSensor - Sense) {agent} still has pending actions !! We don't sense"); }
                return target;
            }


            // Debug.Log($"(ClosestFoodSensor) {ia.name} is sensing closest food...");
            if (!ia.TryGetCapacity(out InteractCapacity capacity)) { return null; }
            if (capacity.data is not InteractData idata || idata == null) { return null; }

            // find the closest food
            ItemData closest_trash = detector.FindClosestInteractableTrash(ia.data, idata, force_loaded:true);
            Capable closestTrash = closest_trash?.Capable;
            if (closestTrash == null) { return null; }

            // now we try to extract the room at the position
            if (RoomEngine.Instance.TryGetCapableRoom(closestTrash.ID, out RoomData room))
            {
                mc.mdata.destination_room_id = room.id;
                // if (Logger.LazyInstance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WanderLoadedSensor) {agent} has a new room destination {room.id}"); }
            }

            // If the target is already set, we update it
            if (target is CapableTarget captarg)
            {
                return captarg.SetCapable(closestTrash);
            }
            return new CapableTarget(closestTrash);
        }
    }
}