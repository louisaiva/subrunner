#pragma warning disable 4014
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RunningHacksViewer : MonoBehaviour, Awakable
{
    [Header("HackInfo")]
    [SerializeField] private GameObject hack_info_prefab;
    [SerializeField] private Transform hack_info_container;
    [SerializeField] private List<UI_HackInfo> hack_infos = new List<UI_HackInfo>();

    [Header("HacksInfoWaiting")]
    [SerializeField] private UI_HacksWaitingInfo hacks_waiting_info;

    [Header("HackCapacity")]
    // [SerializeField] private UI_LaptopItemSlot laptop_item_slot;
    [SerializeField] private HackCapacity hacker;

    [Header("Components")]
    [SerializeField] private Device device;
    [SerializeField] private TextMeshProUGUI title_text;

    [Header("Logs")]
    [SerializeField] private bool log = true;

    // INIT AWAKE
    public void InitAwake()
    {
        if (title_text == null)
        {
            Debug.LogError("(UI_RunningHacksViewer) title_text is not assigned! Please assign it in the inspector.");
            return;
        }

        GameObject.Find("/perso").GetComponent<Perso>().OnDeviceChanged += HandleDeviceChanged;
        hacks_waiting_info.Init();
    }
    private void Start()
    {
        update_title();
    }

    // LAPTOP
    private void HandleDeviceChanged(Device new_device)
    {
        // we remove old device callbacks
        if (device != null)
        {
            if (hacker != null) { hacker.OnExploitRun -= createHackInfo; }
        }

        // if the next is null then we null everything
        if (new_device == null)
        {
            hacker = null;
            device = null;
            update_title();
            return;
        }

        // otherwise we have a new device, we get components and register callbacks
        device = new_device;
        hacker = device.Hacker;
        if (hacker == null)
        {
            if (log) { Debug.LogWarning("(UI_RunningHacksViewer) No HackCapacity found in the device."); }
            update_title();
            return;
        }

        update_title();
        hacker.OnExploitRun += createHackInfo;
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
        if (device == null)
        {
            title_text.text = "no device";
            return;
        }

        if (hacker == null)
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