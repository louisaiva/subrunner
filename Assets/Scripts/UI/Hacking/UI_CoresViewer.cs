using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CoresViewer : MonoBehaviour, Awakable
{

    [Header("CoreInfo")]
    [SerializeField] private GameObject core_info_prefab;
    [SerializeField] private Transform core_info_container;
    [SerializeField] private List<UI_CoreInfo> core_infos = new List<UI_CoreInfo>();

    [Header("ProcessCapacity")]
    // [SerializeField] private UI_LaptopItemSlot laptop_item_slot;
    [SerializeField] private ProcessCapacity processor;

    [Header("Components")]
    [SerializeField] private Device device;
    [SerializeField] private UI_Resizer resizer;
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private Image no_cores_image;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // INIT AWAKE
    public void InitAwake()
    {
        GameObject.Find("/perso").GetComponent<Perso>().OnDeviceChanged += HandleDeviceChanged;
        resizer.Resize(0);
        update_title();
    }

    // LAPTOP
    private void HandleDeviceChanged(Device new_device)
    {
        // we remove old device callback
        if (device != null)
        {
            if (processor != null) { processor.OnCoresNumberChanged -= update_cores_count; }
        }

        // if the next is null then we null everything
        if (new_device == null)
        {
            processor = null;
            device = null;
            clear_core_infos();
            return;
        }

        // otherwise we have a new device, we get the cpu & register callback
        device = new_device;
        processor = device.Processor;
        if (processor == null)
        {
            if (log) { Debug.LogWarning("(UI_RunningHacksViewer) No ProcessCapacity found in the device."); }
            return;
        }
        processor.OnCoresNumberChanged += update_cores_count;
        update_cores_count(processor.MaxCores);
    }




    // CORE INFOS MANAGEMENT
    private void update_cores_count(int new_core_count)
    {
        if (processor == null) { return; }
        List<Core> cores = processor.Cores;

        // we check if we have already enough core_count
        if (core_infos.Count < new_core_count)
        {
            int to_create = new_core_count - core_infos.Count;
            for (int i = 0; i < to_create; ++i)
            {
                create_core_info(cores[core_infos.Count], core_infos.Count);
            }
        }
        else if (core_infos.Count > new_core_count) // we have too many core_info
        {
            int to_remove = core_infos.Count - new_core_count;
            for (int i = 0; i < to_remove; ++i)
            {
                Destroy(core_infos[core_infos.Count - 1].gameObject);
                core_infos.RemoveAt(core_infos.Count - 1);
            }
        }

        // we recall Init() on them
        for (int i = 0; i < core_infos.Count; ++i)
        {
            core_infos[i].Init(cores[i]);
        }

        // resize
        resizer.Resize(new_core_count);

        // and refresh the parent layout group
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());

        update_title(new_core_count);
    }
    private void create_core_info(Core core, int index)
    {
        UI_CoreInfo core_info = Instantiate(core_info_prefab, core_info_container).GetComponent<UI_CoreInfo>();
        core_info.name = $"core_info_{index}";

        core_info.Init(core);
        core_infos.Add(core_info);
    }
    private void clear_core_infos() { update_cores_count(0); }

    // TITLE
    private void update_title(int cores_count = 0)
    {
        if (device == null || cores_count == 0)
        {
            title_text.text = "no cores";
            no_cores_image.gameObject.SetActive(true);
            return;
        }
        title_text.text = cores_count + " cores";
        no_cores_image.gameObject.SetActive(false);
    }
}