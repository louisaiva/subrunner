#pragma warning disable 4014
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RunningHacksViewer : MonoBehaviour, Startable
{
    [Header("HackInfo")]
    [SerializeField] private GameObject hack_info_prefab;
    [SerializeField] private Transform hack_info_container;
    [SerializeField] private List<UI_HackInfo> hack_infos = new List<UI_HackInfo>();

    // [Header("HacksInfoWaiting")]

    // [Header("HackCapacity")]
    // private System.Action<int> update_cores_count_callback;
    // [SerializeField] private UI_LaptopItemSlot laptop_item_slot;
    // [SerializeField] private HackCapacity hacker;

    [Header("Components")]
    // [SerializeField] private Device device;
    [SerializeField] private UI_HacksWaitingInfo hacks_waiting_info;
    [SerializeField] private TextMeshProUGUI title_text;


    [Header("Logs")]
    [SerializeField] private bool log = true;

    // INIT START
    public void InitStart()
    {
        if (title_text == null)
        {
            Debug.LogError("(UI_RunningHacksViewer) title_text is not assigned! Please assign it in the inspector.");
            return;
        }

        // GameObject.Find("/perso").GetComponent<Perso>().OnDeviceGranted += HandleDeviceChanged;
        hacks_waiting_info.Init();

        // Perso.Instance.OnDeviceGranted += HandleDeviceGranted;
        // Perso.Instance.OnDeviceRemoved += HandleDeviceRemoved;
        update_title();
    }

    // DEVICE
    public void HandleDeviceRemoved(Device old_device)
    {
        update_title();
        if (old_device.Hacker == null) { return; }

        // we remove old device callback
        old_device.Hacker.OnExploitRun -= createHackInfo;
    }
    public void HandleDeviceGranted(Device new_device)
    {
        update_title();
        if (new_device.Hacker == null)
        {
            if (log) { Debug.LogWarning("(UI_RunningHacksViewer) No HackCapacity found in the new device, can't set callback"); }
            return;
        }

        // we register new device callback
        new_device.Hacker.OnExploitRun += createHackInfo;
    }

    // CREATE HACK INFO
    private void createHackInfo(Hack hack)
    {
        UI_HackInfo hack_info = Instantiate(hack_info_prefab, hack_info_container).GetComponent<UI_HackInfo>();
        hack_info.name = $"hack_info_{hack.name}";

        hack_info.Init(hack);
        hack_infos.Add(hack_info);

        update_title();
    }

    // UPDATE
    private void Update()
    {
        update_title();

        // update running hacks
        for (int i = 0; i < hack_infos.Count; i++)
        {
            bool to_remove = false;

            // checking if we need to remove the hack info
            if (hack_infos[i] == null) { to_remove = true; }
            else if (hack_infos[i].State == ProcessusState.Completed || hack_infos[i].State == ProcessusState.Failed) { to_remove = true; }

            // if the hack is waiting we remove it from the waiting info
            if (hack_infos[i].State == ProcessusState.Waiting)
            {
                to_remove = true;

                // preparing for hiding the hack_info
                float duration = -99f;
                if (hacks_waiting_info.gameObject.activeSelf == false) { duration = 0f; } // if the waiting info is not visible we hide instantly the hack_info
                hack_infos[i].GetComponent<Transitioner>().HideAndDestroy(duration);
                
                // adding it to the waiting info
                hacks_waiting_info.AddHack(hack_infos[i].hack);
            }

            // removing it if needed
            if (to_remove)
            {
                hack_infos.RemoveAt(i);
                i--; // adjust index after removal
                continue;
            }
        }
    }
    private void update_title()
    {
        if (Perso.Instance.Device == null)
        {
            title_text.text = "no device";
            return;
        }

        if (Perso.Instance.Device.Hacker == null)
        {
            title_text.text = "no module:hack";
            return;
        }

        int running_hacks_count = hack_infos.Count + hacks_waiting_info.Count;

        if (running_hacks_count == 0)
        {
            title_text.text = "no running hacks";
            return;
        }

        title_text.text = $"{running_hacks_count} running hacks";
    }
}