using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CoresViewer : MonoBehaviour, Awakable
{
    [Header("Cores")]
    [SerializeField] private List<Image> cores = new List<Image>();
    [SerializeField] private List<Image> used_cores = new List<Image>();
    public int CoresCount => cores.Count;
    [SerializeField] private Color free_core_color = Color.green;
    [SerializeField] private Color used_core_color = Color.red;
    [SerializeField] private GameObject core_prefab;
    [SerializeField] private Transform cores_container;

    [Header("Laptop")]
    [SerializeField] private UI_LaptopItemSlot laptop_item_slot;
    [SerializeField] private Laptop laptop;

    [Header("Components")]
    private TextMeshProUGUI label;
    private RectTransform rect;

    [Header("Logs")]
    [SerializeField] private bool log = true;

    // INIT AWAKE
    public void InitAwake()
    {
        if (laptop_item_slot == null)
        {
            Debug.LogError("(UI_CoresViewer) laptop_item_slot is not assigned! Please assign it in the inspector.");
            return;
        }

        laptop_item_slot.OnItemChanged += HandleLaptopChanged;

        label = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
    }

    // LAPTOP
    private void HandleLaptopChanged(List<Item> items)
    {
        if (laptop != null)
        {
            laptop.OnCoresChange -= UpdateCoresCount;
            laptop.OnCoresFreedOrUsed -= update_free_used_cores;
        }

        if (items == null || items.Count == 0 || !(items[0] is Laptop))
        {
            laptop = null;
            return;
        }

        laptop = items[0] as Laptop;
        laptop.OnCoresChange += UpdateCoresCount;
        laptop.OnCoresFreedOrUsed += update_free_used_cores;
        UpdateCoresCount(laptop.MaxCores);
    }

    // CORES MANAGEMENT
    public void UpdateCoresCount(int cores_count)
    {
        if (log) { Debug.Log($"(UI_CoresViewer) Updating cores from {CoresCount} to {cores_count}"); }

        // modify the text
        if (cores_count > 0) { label.text = "cpu cores"; }
        else { label.text = "no processor ://"; }


        // modify the height : 48 + 48*ceiltoint(cores/8)
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 48 + 48 * Mathf.CeilToInt(cores_count / 8f));



        // get the number of cores to remove
        int cores_diff = cores_count - CoresCount;
        if (cores_diff == 0) { return; }

        // else if we create some
        if (cores_diff > 0) { create_cores(cores_diff); return; }

        // else we remove some
        List<Image> free_cores = new List<Image>(cores);
        free_cores.RemoveAll(core => used_cores.Contains(core));
        for (int i = 0; i < Mathf.Abs(cores_diff); i++)
        {
            if (cores.Count == 0)
            {
                if (log) { Debug.LogWarning("(UI_CoresViewer) tried to remove a core but there are none left!"); }
                continue;
            }
            Destroy(free_cores[free_cores.Count - 1].gameObject);
            cores.Remove(free_cores[free_cores.Count - 1]);
        }
    }
    private void create_cores(int nb = 1)
    {
        for (int i = 0; i < nb; i++)
        {
            GameObject core = Instantiate(core_prefab, cores_container);
            cores.Add(core.GetComponent<Image>());
        }
    }

    // UPDATE FREE / USED CORES
    private void update_free_used_cores(int nb)
    {
        if (nb > 0)
        {
            if (log) { Debug.Log($"(UI_CoresViewer) Using {nb} cores"); }
            for (int i = 0; i < nb; i++)
            {
                Image core = get_random_free_core();
                use_core(core);
            }
        }
        else if (nb < 0)
        {
            if (log) { Debug.Log($"(UI_CoresViewer) Freeing {Mathf.Abs(nb)} cores"); }
            for (int i = 0; i < Mathf.Abs(nb); i++)
            {
                Image core = used_cores[0];
                free_core(core);
            }
        }
    }
    private void free_core(Image core)
    {
        if (core == null || !used_cores.Contains(core)) { return; }
        core.color = free_core_color;
        used_cores.Remove(core);
    }
    private void use_core(Image core)
    {
        if (core == null || !cores.Contains(core)) { return; }
        core.color = used_core_color;
        used_cores.Add(core);
    }
    private Image get_random_free_core()
    {
        List<Image> free_cores = new List<Image>(cores);
        free_cores.RemoveAll(core => used_cores.Contains(core));
        if (free_cores.Count == 0) { return null; }
        return free_cores[Random.Range(0, free_cores.Count)];
    }
    private void free_them_all()
    {
        if (log) { Debug.Log($"(UI_CoresViewer) Freeing all cores"); }
        foreach (Image core in used_cores)
        {
            free_core(core);
        }
        used_cores.Clear();
    }
}
