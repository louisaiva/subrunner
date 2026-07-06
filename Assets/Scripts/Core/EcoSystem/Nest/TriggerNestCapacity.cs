using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This child class is only to have a specific TriggerNestCapacity prefab
/// that the CapacityBank will load instead of loading the NestCapacity prefab.
/// This allows us to easily add a rigidbody on the TriggerNestCapacity prefab
/// (required for having the trigger working) and so we don't have to make
/// other specific weirdy things like saving rigidbody data and loading them
/// etc etc shitty stuff
/// </summary>
public class TriggerNestCapacity : NestCapacity {}
[Serializable] public class TriggerNestData : NestData
{
    public TriggerNestData(CapacityData parent) : base(parent) { }
}