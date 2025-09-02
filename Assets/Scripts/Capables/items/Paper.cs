using UnityEngine;

// todo store a PoolTransitionSettings to choose if we want the paper can_be_hidden = true
// todo or can_be_canceled if we need to show some IF

public class Paper : Item, Usable
{


    [Header("Paper parameters")]
    public GameObject prefab = null;


    // USABLE
    public string UseLabel { get; set; } = "read";
    public void Use(Capable user)
    {
        // we check if the item is grabbed
        if (!Grabbed) { return; }

        // we show the ui_paper pool
        (UI_Manager.Instance.GetPool("paper") as UI_Paper).SetPaper(this);
        UI_Manager.Instance.SwitchTo("paper");
    }

    // on grabbed
    protected override async void on_grabbed()
    {
        base.on_grabbed();

        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        // we show the paper if we are grabbed by the perso
        if (Holder is Perso perso) { Use(perso); }
    }
}