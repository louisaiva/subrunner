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
    [SerializeField] private UI_LaptopItemSlot laptop_item_slot;
    [SerializeField] private HackCapacity hacker;

    [Header("Components")]
    private Transitioner transitioner;

    [Header("Logs")]
    [SerializeField] private bool log = true;

    // INIT AWAKE
    public void InitAwake()
    {
        if (laptop_item_slot == null)
        {
            Debug.LogError("(UI_RunningHacksViewer) laptop_item_slot is not assigned! Please assign it in the inspector.");
            return;
        }

        laptop_item_slot.OnItemChanged += HandleLaptopChanged;

        transitioner = GetComponent<Transitioner>();
    }

    // LAPTOP
    private void HandleLaptopChanged(List<Item> items)
    {
        if (hacker != null)
        {
            hacker.OnExploitRun -= createHackInfo;
        }

        if (items == null || items.Count == 0 || !(items[0] is Laptop))
        {
            hacker = null;
            return;
        }

        hacker = (items[0] as Laptop).GetCapacity<HackCapacity>();
        hacker.OnExploitRun += createHackInfo;
    }

    // CREATE HACK INFO
    private void createHackInfo(Hack hack)
    {
        UI_HackInfo hack_info = Instantiate(hack_info_prefab, hack_info_container).GetComponent<UI_HackInfo>();
        hack_info.name = $"hack_info_{hack.name}";

        hack_info.Init(hack);
        hack_infos.Add(hack_info);
        transitioner.Show();
    }

    // UPDATE
    private void Update()
    {
        // update gameObject
        if (hack_infos.Count == 0) { transitioner.Hide(); return; }

        // update running hacks
        for (int i = 0; i < hack_infos.Count; i++)
        {
            Hack hack = hack_infos[i].hack;
            if (hack == null || hack_infos[i] == null) { continue; }

            // check the state of the hack
            if (hack.state == HackState.Failed || hack.state == HackState.Overflowed || hack.state == HackState.Completed)
            {
                // we remove the hack info
                hack_infos[i].GetComponent<Transitioner>().HideAndDestroy();
                hack_infos.RemoveAt(i);
                i--; // adjust index after removal
                continue;
            }
        }
    }
}
