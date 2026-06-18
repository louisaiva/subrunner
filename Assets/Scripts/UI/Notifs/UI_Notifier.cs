using System.Collections.Generic;
using UnityEngine;

public class UI_Notifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_Notif text_notif_prefab;
    [SerializeField] private UI_ItemNotif notifPrefab;
    [SerializeField] private Transform notifParent;

    private List<UI_Notif> active_notifs = new List<UI_Notif>();

    // CALLBACKS
    public void SetCallbacks(Capable capable)
    {
        // set controller's capable control callbacks
        capable.Inventory.OnItemGrabbed += CreateItemNotif;

        if (capable is not Hacker hacker) { return; }

        // set handle device callbacks
        hacker.OnDeviceGranted += handle_device_granted;
        hacker.OnDeviceRemoved += handle_device_removed;
    }
    public void RemoveCallbacks(Capable capable)
    {
        // remove controller's capable control callbacks
        capable.Inventory.OnItemGrabbed -= CreateItemNotif;

        if (capable is not Hacker hacker) { return; }

        // remove handle device callbacks
        hacker.OnDeviceGranted -= handle_device_granted;
        hacker.OnDeviceRemoved -= handle_device_removed;
    }

    // HANDLE DEVICE
    private void handle_device_granted(Device new_device)
    {
        new_device.OnFileWritten += CreateFileNotif;

        // we create files notif for all files in the device
        List<File> files = new_device.GetFiles();
        for (int i = 0; i < files.Count; i++) { CreateFileNotif(files[i]); }
    }
    private void handle_device_removed(Device old_device)
    {
        old_device.OnFileWritten -= CreateFileNotif;
    }

    // CREATE NOTIF
    public void CreateItemNotif(Item item)
    {
        // checks if we have an existing notif with the same item ref
        for (int i = active_notifs.Count - 1; i >= 0; i--)
        {
            UI_Notif existing_notif = active_notifs[i];
            if (existing_notif == null) { active_notifs.RemoveAt(i); continue; } // clean null entries
            if (existing_notif is not UI_ItemNotif uiin) { continue; } // skip text notifs
            if (uiin.Item == null) { continue; } // skip file notifs
            if (uiin.Item.Reference != item.Reference) { continue; } // skip items with wrong reference

            // found existing notif, restart its timer
            uiin.AddDuplicate();
            return;
        }

        // create a new one
        UI_ItemNotif notif = Instantiate(notifPrefab, notifParent);
        notif.Init(item);
        active_notifs.Add(notif);
    }
    public void CreateFileNotif(File file)
    {
        UI_ItemNotif notif = Instantiate(notifPrefab, notifParent);
        notif.Init(file);
        active_notifs.Add(notif);
    }
    public void Notify(string text = "game saved !")
    {
        // checks if we have an existing notif with the same item ref
        UI_Notif notif = null;
        for (int i = active_notifs.Count - 1; i >= 0; i--)
        {
            notif = active_notifs[i];
            if (notif == null) { active_notifs.RemoveAt(i); continue; } // clean null entries
            if (notif is UI_ItemNotif) { continue; } // skip item/file notifs
            if (notif.NotifText != text) { continue; } // skip items with wrong reference

            // found existing notif, restart its timer
            notif.AddDuplicate();
            return;
        }

        notif = Instantiate(text_notif_prefab, notifParent);
        notif.Init(text);
        active_notifs.Add(notif);
    }
}