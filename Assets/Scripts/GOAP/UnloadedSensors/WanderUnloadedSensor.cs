using CrashKonijn.Agent.Core;

// INTERFACES
public interface Sensor
{
    ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget);
}
public interface UnloadedSensor : Sensor
{
    // this is a marker interface for detectors that can be used in unloaded mode (without an agent or references)
}
public interface LoadedSensor : Sensor
{
    // this is a marker interface for detectors that require an agent and references to function
}

// WANDER UNLOADED SENSOR
public class WanderUnloadedSensor : UnloadedSensor
{
    public ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
    {
        // the wander unloaded sensor does nothing, it just returns null as a target
        return null;
    }
}
