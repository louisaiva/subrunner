using System.Collections;
using UnityEngine;

public class UI_SlottablePool : UI_Pool
{
    [Header("Components")]
    protected UI_Slottable slottable;
    public bool use_ingame_drop = false;


    protected override void Awake()
    {
        base.Awake();
        if (slottable == null) { slottable = GetComponent<UI_Slottable>(); }
        if (slottable == null) { Debug.LogError($"(UI_SlottablePool) missing slottable on {name}"); }
    }

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        if (log) { Debug.Log($"(UI_SlottablePool) enabling slottable pool : {name}"); }
        slottable.Enable(ingame: use_ingame_drop);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        if (log) { Debug.Log($"(UI_SlottablePool) disabling slottable pool : {name}"); }
        slottable.Disable();
        yield break;
    }
}
