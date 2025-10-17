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
            if (!Perso.Instance.Alive) { return false; }
            if (UI_LaptopItemSlot.Instance == null || !UI_LaptopItemSlot.Instance.HasLaptop) { return false; }
            return true;
        }
    } */

    // [Header("Transition parameters")]
    // public float final_timescale = 0.5f;
    // public float bg_final_alpha = 0.5f;

    // START
    private void Start()
    {
        // on met le callback de pour afficher ui_hacking
        Perso.Instance.OnDeviceGranted += HandleDeviceGranted;
        Perso.Instance.OnDeviceRemoved += HandleDeviceRemoved;
    }

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
    private void HandleDeviceRemoved(Device old_device)
    {
        // on cache hacking
        UI_Manager.Instance.UnstackFromHUD("hacking", override_transition: true);

        // ! todo gaffe pcq vu que le UI_Manager n'autorise pas les transitions quand y'en a déjà une en cours,
        // todo bah ça risque de bug quand on passe d'un laptop à un computer et qu'on se trouve dans le hud
        // todo (pour le moment ça pose pas de pb vu qu'on passe tt le temps par l'inventaire ou alors c'est juste recup un laptop)
    }
    private void HandleDeviceGranted(Device new_device)
    {
        // on affiche hacking
        UI_Manager.Instance.StackOnHUD("hacking", override_transition: true);
    }
}