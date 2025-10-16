#pragma warning disable 4014
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// kind of the same as UI_HackInfo except this one is made for working with multiple hacks
/// at the same time. it has only one progress bar that is not supposed to change since it is mostly for waiting hacks
/// </summary>
public class UI_HacksWaitingInfo : MonoBehaviour
{
    [Header("UI_HacksWaitingInfo")]
    private Transitioner transitioner;
    [HideInInspector] public List<Hack> hacks = new List<Hack>();
    public int Count { get { return hacks.Count; } }

    [Header("Cores")]
    [SerializeField] private Image cores;
    [SerializeField] private TextMeshProUGUI cores_cost;

    [Header("Progress & Colors")]
    [SerializeField] private Image progress;
    [SerializeField] private Color waiting_color = Color.yellow;
    [SerializeField] private Color failed_color = Color.red;

    [Header("Hacks names")]
    [SerializeField] private Transform hacks_names_container;
    [SerializeField] private GameObject hack_name_prefab;
    [SerializeField] private TextMeshProUGUI no_hack_text;
    private Dictionary<string,int> hack_name_counts = new Dictionary<string, int>();
    private Dictionary<string,TextMeshProUGUI> hack_name_texts = new Dictionary<string, TextMeshProUGUI>();

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // START
    public void Init()
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }

        // init cores
        cores.color = waiting_color;
        cores_cost.color = waiting_color; // waiting color means we use x cores

        // init progress
        progress.color = waiting_color;
        no_hack_text.gameObject.SetActive(true);

        // we clear all previous hacks
        hacks.Clear();
        gameObject.SetActive(false);

        if (log) { Debug.Log($"[UI_HacksWaitingInfo] Initialized"); }
    }

    // HACKS
    public void AddHack(Hack hack)
    {
        if (hacks.Contains(hack)) { return; }
        hacks.Add(hack);
        update_cores_display();

        // we hide the no hack text
        no_hack_text.gameObject.SetActive(false);

        // update hacks names
        if (!hack_name_counts.ContainsKey(hack.name))
        {
            hack_name_counts[hack.name] = 0;
            GameObject go = Instantiate(hack_name_prefab, hacks_names_container);
            hack_name_texts[hack.name] = go.GetComponent<TextMeshProUGUI>();
        }

        hack_name_counts[hack.name]++;
        hack_name_texts[hack.name].text = hack.name + (hack_name_counts[hack.name] > 1 ? " x" + hack_name_counts[hack.name] : "");

        if (log) { Debug.Log($"[UI_HacksWaitingInfo] Added hack {hack.name}, now {hacks.Count} hacks waiting"); }

        // on affiche le hack info brutalement (comme ça si c'est bien fait ça transitionne parfaitement avec le UI_HackInfo)
        gameObject.SetActive(true);
        transitioner.Show(0f);
    }
    private void remove_hack(Hack hack)
    {
        if (!hacks.Contains(hack)) { return; }
        hacks.Remove(hack);
        update_cores_display();

        // update hacks names
        hack_name_counts[hack.name]--;
        hack_name_texts[hack.name].text = hack.name + (hack_name_counts[hack.name] > 1 ? " x" + hack_name_counts[hack.name] : "");
        if (hack_name_counts[hack.name] <= 0)
        {
            hack_name_counts.Remove(hack.name);
            Destroy(hack_name_texts[hack.name].gameObject);
            hack_name_texts.Remove(hack.name);
        }

        // sets no hack text to hack.name so last hack name is shown if we fail the hack
        no_hack_text.text = hack.name;

        // if no more hack we hide the whole thing
        if (hacks.Count == 0)
        {
            progress.color = failed_color; // we put the progress bar in red to show that the hacks failed
            no_hack_text.gameObject.SetActive(true);
            transitioner.HideAndDisable();
        }
    }

    // UPDATE
    void Update()
    {
        // we check if one or less hack is done
        for (int i = hacks.Count - 1; i >= 0; i--)
        {
            if (hacks[i].state == ProcessusState.Completed || hacks[i].state == ProcessusState.Failed)
            {
                remove_hack(hacks[i]);
            }
        }
    }

    // LOW UPDATE
    private void update_cores_display()
    {
        int total_cores = 0;
        foreach (Hack hack in hacks)
        {
            total_cores += hack.provided_cores_count;
        }
        cores_cost.text = total_cores.ToString();
        cores_cost.gameObject.SetActive(total_cores > 1);
    }

}