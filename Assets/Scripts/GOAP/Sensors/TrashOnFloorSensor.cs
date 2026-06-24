using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;

namespace subrunner.goap
{
    /// <summary>
    /// this is a global sensor EVEN IF we want each
    /// level to be treated separetly (a npc
    /// will only detect trash in the current level). This is
    /// intended because this sensor will always only be called
    /// for the loaded entities, which means entities that
    /// are in the current level, so no need to check the mob's level
    /// since we know it is the current one !
    /// </summary>
    public class TrashOnFloorSensor : GlobalWorldSensorBase 
    {
        public override void Created() { }

        public override SenseValue Sense()
        {
            // we check if we have some trash on the floor of the current level !
            string level_id = LevelEngine.Instance.CurrentLevelID;
            if (level_id == null) { return new SenseValue(0);}
            if (CapableEngine.TrashEngine == null) { return new SenseValue(0); }
            return new SenseValue(CapableEngine.TrashEngine.GetQuantityOfTrash(level_id));
        }
    }
}