using System.Collections;
using UnityEngine;

public class UI_WorldBuilderPool : UI_SlottablePool
{
    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();
        WorldBuilder.StaticInstance?.gameObject.SetActive(true);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        WorldBuilder.StaticInstance?.gameObject.SetActive(false);
        yield return base.disable_coroutine();
        yield break;
    }
}