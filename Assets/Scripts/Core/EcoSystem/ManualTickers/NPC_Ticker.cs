using System.Collections.Generic;
using UnityEngine;

public class NPC_Ticker : ManualSpeciesTicker
{
    public override string species => "npc";
    public override bool handle_entity_receiving_each_frame => true;
    [SerializeField] private int trash_treshold = 50;

    public override void Tick(Species spec, List<NestData> nests)
    {
        if (CapableEngine.TrashEngine.GetQuantityOfTrash(LevelEngine.LazyInstance.CurrentLevelID) < trash_treshold) { return; }

        // then we spawn all NPC from the nests
        foreach (NestData nest in nests) { nest.SpawnThemAll(); }
    }
}