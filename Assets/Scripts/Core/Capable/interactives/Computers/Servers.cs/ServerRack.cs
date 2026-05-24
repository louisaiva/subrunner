using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerRack : Computer, Chestable
{
    public string ChestType { get; } = "server";

    /* protected override void Start()
    {
        base.Start();

        // we randomly activate our servers
        Transform servers_parent = transform.Find("servers");
        if (servers_parent == null)
        {
            Debug.LogWarning($"(ServerRack) {name} has no child named 'servers', cannot randomly activate servers");
            return;
        }
        for (int i = 0; i < servers_parent.childCount; i++)
        {
            Transform server = servers_parent.GetChild(i);
            server.gameObject.SetActive(Random.value > 0.5f);
        }
    } */


    // ON INTERACT / HOVER LOST
    public override void OnInteract(Capable interactor)
    {
        base.OnInteract(interactor);

        if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(ServerRack) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        }

        // only if the interactor is controlled
        if (interactor != Controller.LazyInstance.Capable || chest_pool.IsShown(this)) { return; }

        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // we show the inventory UI
        chest_pool.ShowChest(this);
    }
    public override void OnHoverLost(Capable interactor)
    {
        base.OnHoverLost(interactor);

        // if we are not shown we do nothing
        if (!UI_Manager.Instance.TryGetPool(out UI_ChestPool chest_pool))
        {
            Debug.LogError($"(ServerRack) {name} cannot find UI_ChestPool to show chest inventory");
            return;
        }
        if (!chest_pool.IsShown(this)) { return; }

        // if there is no more controlled interactors we hide the ui inventory
        for (int i = 0; i < interactors.Count; i++)
        {
            if (interactors[i] == Controller.LazyInstance.Capable) { return; } // we still have the controlled interactor so we dont hide the ui
        }

        // we hide the inventory UI
        chest_pool.HideChest();
        Interactor = null; // we reset the interactor
    }
    public async void ExitHover()
    {
        await System.Threading.Tasks.Task.Yield(); // wait a bit to avoid issues with OnHoverLost called just after
        OnHoverLost(Controller.LazyInstance.Capable);
    }



}