using System.Collections.Generic;
using UnityEngine;

public class NPC_Ticker : ManualSpeciesTicker
{
    public override string species => "npc";
    public override bool handle_entity_receiving_each_frame => true;
    [SerializeField] private int trash_treshold = 50;
    [SerializeField] private int max_population = 4;

    private float next_spawn_cooldown = 0f;
    private float delay_between_spawns = 30f;

    public override void Tick(Species spec, List<NestData> nests, float delta_tick)
    {
        next_spawn_cooldown -= delta_tick;
        if (next_spawn_cooldown > 0f) { return; }
        if (CapableEngine.TrashEngine.GetQuantityOfTrash(LevelEngine.LazyInstance.CurrentLevelID) < trash_treshold) { return; }
        if (spec.alive_population >= max_population) { return; }

        // gather loaded nests
        foreach (NestData nest in nests)
        {
            if (!nest.Loaded) { continue; }
            if (nest.EntityCount == 0) { continue; }
            nest.SpawnOne();
            next_spawn_cooldown = delay_between_spawns;
            return;
        }
    }
}