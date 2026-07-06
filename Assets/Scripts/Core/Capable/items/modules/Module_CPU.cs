using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// CPU module
/// </summary>
public class Module_CPU : Module
{
    public List<Core> Cores = new List<Core>();

    protected override void apply_upgrade()
    {
        // we update the number of cores
        int new_cores = Mathf.RoundToInt(get_upgrade_effect("cores"));
        while (Cores.Count < new_cores)
        {
            Cores.Add(new Core());
        }
        while (Cores.Count > new_cores)
        {
            Cores.RemoveAt(Cores.Count - 1);
        }

        // update the speed
        float speed = get_upgrade_effect("process speed") / 100f;
        Cores.ForEach(core => core.Speed = speed);
    }
}