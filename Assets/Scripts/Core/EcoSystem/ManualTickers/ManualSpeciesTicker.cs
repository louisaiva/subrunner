using System.Collections.Generic;
using UnityEngine;

public abstract class ManualSpeciesTicker : MonoBehaviour
{
    public abstract string species { get; }
    public abstract bool handle_entity_receiving_each_frame { get; }
    public abstract void Tick(Species spec, List<NestData> nests);
}