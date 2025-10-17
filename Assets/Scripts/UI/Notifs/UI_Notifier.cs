using System.Collections.Generic;
using UnityEngine;

public class UI_Notifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_Notif notifPrefab;
    [SerializeField] private Transform notifParent;

    [Header("Callbacks")]
    private System.Action<Item> createItemCallback;
    private System.Action<File> createFileCallback;

    // AWAKE
    private void Awake()
    {
        // create the callbacks
        createItemCallback = (Item item) => { CreateItemNotif(item); };
        createFileCallback = (File file) => { CreateFileNotif(file); };

        // set controller's capable control callbacks
        Perso.Instance.Inventory.OnItemGrabbed += createItemCallback;

        // set handle device callbacks
        Perso.Instance.OnDeviceGranted += handle_device_granted;
        Perso.Instance.OnDeviceRemoved += handle_device_removed;
    }

    // HANDLE DEVICE
    private void handle_device_granted(Device new_device)
    {
        new_device.OnFileWritten += createFileCallback;

        // we create files notif for all files in the device
        List<File> files = new_device.GetFiles();
        for (int i = 0; i < files.Count; i++) { CreateFileNotif(files[i]); }
    }
    private void handle_device_removed(Device old_device)
    {
        old_device.OnFileWritten -= createFileCallback;
    }

    // CREATE NOTIF
    public void CreateItemNotif(Item item)
    {
        UI_Notif notif = Instantiate(notifPrefab, notifParent);
        notif.Init(item);
    }
    public void CreateFileNotif(File file)
    {
        UI_Notif notif = Instantiate(notifPrefab, notifParent);
        notif.Init(file);
    }
}