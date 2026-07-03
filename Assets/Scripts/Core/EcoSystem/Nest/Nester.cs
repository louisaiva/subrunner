using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Nester : Capable
{
    

    public bool CanHost(Capable capable)
    {
        if (!Loaded) { return false; }
        if (!TryGetCapacity(out NestCapacity nc)) { return false; }
        return nc.ndata.CanHostEntity(capable.ID);
    }

    public void Host(Capable capable)
    {
        if (!TryGetCapacity(out NestCapacity nc)) { return; }
        nc.ndata.ReceiveEntity(manually:true);

        // then we despawn the entity which will destroy it !
        CapableEngine.Instance.DespawnCapable(capable.data);
    }

}