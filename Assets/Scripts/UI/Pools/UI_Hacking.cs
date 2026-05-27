using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UI_Hacking : UI_Pool
{
    // public bool log_enabling = false;

    /* public override bool Available
    {
        get
        {
            if (in_transition) { return false; }
            if (!Controller.Perso.Alive) { return false; }
            if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return false; }
            return true;
        }
    } */

    // [Header("Transition parameters")]
    // public float final_timescale = 0.5f;
    // public float bg_final_alpha = 0.5f;

    // START
    /* private void Start()
    {
        // on met le callback de pour afficher ui_hacking
        Controller.Perso.OnDeviceGranted += HandleDeviceGranted;
        Controller.Perso.OnDeviceRemoved += HandleDeviceRemoved;
    } */

    // ON PERSO DEVICE CHANGED
    /* private void HandlePersoDeviceChanged(Device new_device)
    {
        if (new_device == null)
        {
            // on cache hacking
            UI_Manager.Instance.UnstackFromHUD("hacking");
            return;
        }

        // on affiche hacking
        UI_Manager.Instance.StackOnHUD("hacking");
    }
    */
   
    // DEVICE
    public void HandleDeviceRemoved(Device old_device)
    {
        

        // on cache hacking
        // UI_Manager.Instance.UnstackFromHUD("hacking", override_transition: true);

        // ! todo gaffe pcq vu que le UI_Manager n'autorise pas les transitions quand y'en a déjà une en cours,
        // todo bah ça risque de bug quand on passe d'un laptop à un computer et qu'on se trouve dans le hud
        // todo (pour le moment ça pose pas de pb vu qu'on passe tt le temps par l'inventaire ou alors c'est juste recup un laptop)

        // todo : update, j'ai mis un parametre override transition par contre ça peut casser les transitions d'avant,
        // todo : et donc laisser des ui_pool partiellement affichées
        // ex si on level up et que 0.1s plus tard on meurt alors game over va s'afficher MAIS va interrompre
        // la coroutine du level up ce qui ne l'arrete pas proprement et donc level_up_ui_pool.show_coroutine va se terminer proprement,
        // mais UI_Manager aura oublié que lvl up est affiché
    }
    public void HandleDeviceGranted(Device new_device)
    {
        // on affiche hacking
        // UI_Manager.Instance.StackOnHUD("hacking", override_transition: true);
    }
}