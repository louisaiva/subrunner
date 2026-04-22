using System.Collections;
using UnityEngine;

public class UI_PopupInputText : UI_SlottablePool
{
    [SerializeField] private UI_InputText inputTextSlot;
    public UI_InputText InputText => inputTextSlot;

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        yield return base.enable_coroutine();
        inputTextSlot.SelectInput();
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        inputTextSlot.DeselectInput();
        yield return base.disable_coroutine();
        yield break;
    }

}