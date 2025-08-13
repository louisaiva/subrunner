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
    [SerializeField] private List<Transitioner> hack_infos = new List<Transitioner>();
    [SerializeField] private List<Hack> associated_hacks = new List<Hack>();

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
        Transitioner hack_info = Instantiate(hack_info_prefab, hack_info_container).GetComponent<Transitioner>();
        hack_info.name = $"hack_info_{hack.exploit.name}";
        hack_info.transform.Find("name").GetComponent<TextMeshProUGUI>().text = "> " + hack.exploit.name;
        hack_info.transform.Find("details/cost/nb").GetComponent<TextMeshProUGUI>().text = hack.exploit.cores_cost.ToString();
        hack_info.transform.Find("details/cost/nb").gameObject.SetActive(hack.exploit.cores_cost > 1);

        hack_infos.Add(hack_info);
        associated_hacks.Add(hack);
        transitioner.Show();

        update_hack_info(hack_info, hack);
    }

    // UPDATE
    private void Update()
    {
        // update gameObject
        if (associated_hacks.Count == 0) { transitioner.Hide(); return; }

        // update running hacks
        for (int i = 0; i < associated_hacks.Count; i++)
        {
            Hack hack = associated_hacks[i];
            if (hack == null || hack_infos[i] == null) { continue; }

            // else we update the hack info
            update_hack_info(hack_infos[i], hack);

            // check the state of the hack
            if (hack.state == HackState.Failed || hack.state == HackState.Overflowed)
            {
                // we remove the hack info
                change_progress_color_and_fade(hack_infos[i], Color.red);
                associated_hacks.RemoveAt(i);
                hack_infos.RemoveAt(i);
                i--; // adjust index after removal
                continue;
            }
            else if (hack.state == HackState.Completed)
            {
                // we remove the hack info
                change_progress_color_and_fade(hack_infos[i], Color.green);
                associated_hacks.RemoveAt(i);
                hack_infos.RemoveAt(i);
                i--; // adjust index after removal
                continue;
            }
        }
    }

    // UPDATE LOW
    private void update_hack_info(Transitioner hack_info, Hack hack)
    {
        TextMeshProUGUI hack_info_progress_text = hack_info.transform.Find("details/progress/nb").GetComponent<TextMeshProUGUI>();
        hack_info_progress_text.text = hack.progress.ToString("F0");
    }
    private void change_progress_color_and_fade(Transitioner hack_info, Color color)
    {
        TextMeshProUGUI hack_info_progress_text = hack_info.transform.Find("details/progress/nb").GetComponent<TextMeshProUGUI>();
        hack_info_progress_text.color = color;
        hack_info.transform.Find("details/progress").GetComponent<TextMeshProUGUI>().color = color;
        hack_info.HideAndDestroy();
    }
}
