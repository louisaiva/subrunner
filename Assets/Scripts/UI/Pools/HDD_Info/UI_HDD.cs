using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HDD : UI_Pool, I_UI_Slottable
{

    [Header("Disk")]
    [SerializeField] private StoreCapacity disk;

    [Header("Components")]
    [SerializeField] private GameObject filename_prefab;
    [SerializeField] private Transform files_parent;
    [SerializeField] private TextMeshProUGUI data_text;

    [Header("UI_Texts")]
    [SerializeField] private List<GameObject> ui_texts;

    // DISK SETTING
    public void SetDisk(StoreCapacity disk)
    {
        this.disk = disk;
        if (disk == null) { return; }
        if (log) { Debug.Log($"(UI_HDD) setting disk {disk.name}"); }

        // we clear the files parent
        foreach (GameObject text in ui_texts) { Destroy(text); }
        ui_texts.Clear();

        // we clear the empty_disk file
        if (files_parent.Find("empty_disk") != null)
        {
            Destroy(files_parent.Find("empty_disk").gameObject);
        }

        // we get the files of the storecapacity
        List<File> files = disk.Files;
        if (files.Count == 0)
        {
            data_text.text = "no data";
            GameObject text = Instantiate(filename_prefab, files_parent);
            text.GetComponent<TextMeshProUGUI>().text = "empty disk";
            text.name = "empty_disk";
            Destroy(text.GetComponent<UI_Text>());
            return;
        }

        foreach (File file in files)
        {
            if (log) { Debug.Log($"(UI_HDD) creating filename {file.name}"); }
            create_filename(file);
        }
    }
    private void create_filename(File file)
    {
        GameObject text = Instantiate(filename_prefab, files_parent);
        // text.GetComponent<TextMeshProUGUI>().text = file.name + file.extension;
        text.GetComponent<UI_HDD_File>().SetFile(file);
        ui_texts.Add(text);
    }

    // SHOW FILE DATA
    public void ShowFileData(File file)
    {
        if (file == null) { return; }
        if (log) { Debug.Log($"(UI_HDD) showing data for file {file.name}"); }

        // we create the text
        string data = "file name : \n\n\t" + file.name + file.extension + "\n\n";
        data += "data : \n\n\t" + file.data;

        // we set the data text
        data_text.text = data;
    }

    // SHOW HIDE
    public override async Awaitable Show(float duration, List<GameObject> dont_show = null)
    {
        await base.Show(duration, dont_show);

        // on active le navigator
        UI_XboxNavigator.Instance.Enable(this);
    }
    public override async Awaitable Hide(float duration, List<GameObject> dont_hide = null)
    {
        // on désactive le navigator
        UI_XboxNavigator.Instance.Disable(this);

        await base.Hide(duration, dont_hide);
    }

    // SLOTTABLE
    public List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator)
    {
        if (log) { Debug.Log($"(UI_HDD) getting slots"); }
        List<GameObject> slots = new List<GameObject>();

        // we add the ui_texts
        foreach (GameObject text in ui_texts)
        {
            if (text == null || text.GetComponent<UI_Text>() == null) { continue; }
            slots.Add(text);
        }

        return slots;
    }
    public bool IsYourSlot(GameObject slot)
    {
        if (slot.transform.parent == files_parent) { return true; }
        return false;
    }
    public Vector2 SavedPosition { get; private set; } = Vector2.zero;

}