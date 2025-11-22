using System.Collections;
using UnityEngine;

public class UI_SlottablePool : UI_Pool
{
    [Header("Components")]
    public UI_Slottable slottable;
    public bool use_ingame_drop = false;

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        slottable.Enable(ingame: use_ingame_drop);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        slottable.Disable();
        yield break;
    }
}
