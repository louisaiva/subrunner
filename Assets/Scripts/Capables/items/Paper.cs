using UnityEngine;


public class Paper : Item, Usable
{

    [Header("Paper parameters")]
    public GameObject prefab = null;
    public PoolTransitionSettings transition_settings = PoolTransitionSettings.InMenuDefault;


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
        if (Controller.Instance.Capable == Holder)
        {
            Use(Controller.Instance.Capable);
        }
    }
}