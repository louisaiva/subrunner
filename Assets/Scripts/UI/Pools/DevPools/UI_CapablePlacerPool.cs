using System.Collections;
using UnityEngine;

public class UI_CapablePlacerPool : UI_SlottablePool
{
    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        Debug.Log($"[UI_CapablePlacerPool] enabling, we also enable worldplacer");
        // we ensure worldplacer is enable
        if (!WorldPlacer.LazyInstance.gameObject.activeSelf)
        {
            WorldPlacer.LazyInstance.gameObject.SetActive(true);
            WorldPlacer.LazyInstance.Status = WorldPlacerStatus.PlacingObject;
        }
    }
    protected override IEnumerator disable_coroutine()
    {
        Debug.Log($"[UI_CapablePlacerPool] disabling, we also disable worldplacer");
        // we disable worldplacer
        if (WorldPlacer.LazyInstance.gameObject.activeSelf)
        {
            WorldPlacer.LazyInstance.gameObject.SetActive(false);
            WorldPlacer.LazyInstance.Status = WorldPlacerStatus.NotWorking;
        }

        yield return base.disable_coroutine();
    }
}