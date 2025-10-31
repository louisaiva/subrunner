using System.Collections;
using UnityEngine;

public class UI_SlottablePool : UI_Pool
{
    [Header("Components")]
    public UI_Slottable slottable;
    public bool ingame = false;

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        slottable.Enable(ingame: ingame, starting_slot: true);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        slottable.Disable();
        yield break;
    }
}
