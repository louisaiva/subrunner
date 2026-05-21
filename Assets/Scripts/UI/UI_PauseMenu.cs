using System.Collections;
using UnityEngine;

public class UI_PauseMenu : UI_Pool
{
    [Header("Slottable")]
    [SerializeField] private Transform slots_parent;
    [SerializeField] private UI_Slottable slottable;

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator
        slottable.Enable(ingame: false);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        slottable.Disable();
        yield break;
    }

    // EVENTS
    protected override void on_added_to_stack()
    {
        GameManager.State = GameState.Paused;
    }
    protected override void on_removed_from_stack()
    {
        GameManager.State = GameState.Gaming;
    }
}