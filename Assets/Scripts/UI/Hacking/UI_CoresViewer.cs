using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CoresViewer : MonoBehaviour, Startable
{

    [Header("CoreInfo")]
    [SerializeField] private GameObject core_info_prefab;
    [SerializeField] private Transform core_info_container;
    [SerializeField] private List<UI_CoreInfo> core_infos = new List<UI_CoreInfo>();

    [Header("ProcessCapacity")]
    private System.Action<int> update_cores_count_callback;

    [Header("Components")]
    [SerializeField] private UI_Resizer resizer;
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private Image no_cores_image;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // INIT START
    public void InitStart()
    {
        Perso.Instance.OnDeviceGranted += HandleDeviceGranted;
        Perso.Instance.OnDeviceRemoved += HandleDeviceRemoved;

        // reset title and all   
        resizer.Resize(0);
        update_title();
    }

    // DEVICE
    private void HandleDeviceRemoved(Device old_device)
    {
        clear_core_infos();

        // we remove old device callback
        if (update_cores_count_callback == null) { return; }
        old_device.Processor.OnCoresNumberChanged -= update_cores_count_callback;
        update_cores_count_callback = null;
    }
    private void HandleDeviceGranted(Device new_device)
    {
        if (new_device.Processor == null) {
            if (log) { Debug.LogWarning("(UI_CoresViewer) No ProcessCapacity found in the device."); }
            return;
        }

        // we create & register the callback and we update directly
        update_cores_count_callback = (int cores_count) => { update_cores_count(new_device.Processor); };
        new_device.Processor.OnCoresNumberChanged += update_cores_count_callback;
        update_cores_count(new_device.Processor);
    }


    // CORE INFOS MANAGEMENT
    private void update_cores_count(ProcessCapacity processor)
    {
        // we get the target core number & cores list
        int new_core_count = 0;
        List<Core> cores = new List<Core>();
        if (processor != null)
        {
            new_core_count = processor.MaxCores;
            cores = processor.Cores;
        }

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
    private void clear_core_infos() { update_cores_count(null); }

    // TITLE
    private void update_title(int cores_count = 0)
    {
        if (cores_count == 0)
        {
            title_text.text = "no cores";
            no_cores_image.gameObject.SetActive(true);
            return;
        }
        title_text.text = cores_count + " cores";
        no_cores_image.gameObject.SetActive(false);
    }
}