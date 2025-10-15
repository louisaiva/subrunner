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
        // update running hacks
        for (int i = 0; i < hack_infos.Count; i++)
        {
            Hack hack = hack_infos[i].hack;
            if (hack == null || hack_infos[i] == null) { continue; }

            // check the state of the hack
            if (hack.state == ProcessusState.Failed || hack.state == ProcessusState.Completed)
            {
                // we remove the hack info
                hack_infos[i].GetComponent<Transitioner>().HideAndDestroy();
                hack_infos.RemoveAt(i);
                update_title();
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

        if (hack_infos.Count == 0)
        {
            title_text.text = "no running hacks";
            return;
        }

        title_text.text = $"{hack_infos.Count} running hacks";
    }
}