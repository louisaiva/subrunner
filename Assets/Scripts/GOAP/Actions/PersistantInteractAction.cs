using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    /// <summary>
    /// persistant mean we use the destination room id to keep this action through all the map !
    /// sensors don't sense if the destination room is set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistantInteractAction<T> : InteractAction<T> where T : Interactable
    {
        public override void End(IMonoAgent agent, Data data)
        {
            // here we can reset the room destination in the motor data !
            if (data.ia.TryGetCapacity(out MotorCapacity mc))
            {
                if (mc.mdata != null) { mc.mdata.destination_room_id = ""; }
            }
        }
    }
}