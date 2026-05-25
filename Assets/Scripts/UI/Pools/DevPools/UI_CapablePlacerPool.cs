using System.Collections;
using UnityEngine;

public class UI_CapablePlacerPool : UI_SlottablePool
{
    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();

        // Debug.Log($"[UI_CapablePlacerPool] enabling, we also enable worldplacer");
        // we enable worldplacer
        WorldPlacer.LazyInstance.Enable();
    }
    protected override IEnumerator disable_coroutine()
    {
        // Debug.Log($"[UI_CapablePlacerPool] disabling, we also disable worldplacer");
        
        // we disable worldplacer
        WorldPlacer.LazyInstance.Disable();

        yield return base.disable_coroutine();
    }
}