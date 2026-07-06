using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HDD : UI_Pool/* , Slottable */
{

    [Header("Disk")]
    [SerializeField] private StoreCapacity disk;
    [SerializeField] private TextMeshProUGUI disk_letter;

    [Header("Files")]
    [SerializeField] private GameObject filename_prefab;
    [SerializeField] private Transform files_parent;
    [SerializeField] private TextMeshProUGUI data_text;

    [Header("UI_Texts")]
    [SerializeField] private List<UI_Text> ui_texts;

    [Header("capacity slider")]
    [SerializeField] private RectTransform capacity_slider;
    [SerializeField] private int capacity_slider_max_width = 330;
    [SerializeField] private Color capacity_slider_empty_color = Color.green;
    [SerializeField] private Color capacity_slider_full_color = Color.red;
    [SerializeField] private TextMeshProUGUI capacity_text;

    [Header("Temp Slider")]
    [SerializeField] private RectTransform temp_slider;
    [SerializeField] private int temp_slider_max_height = 72;
    [SerializeField] private TextMeshProUGUI temp_text;
    [SerializeField] private Color temp_slider_empty_color = Color.green;
    [SerializeField] private Color temp_slider_full_color = Color.red;
    [SerializeField] private TempCapacity temperer;

    [Header("Components")]
    [SerializeField] private UI_Slottable slottable;

    // DISK SETTING
    public void SetDisk(StoreCapacity disk)
    {
        this.disk = disk;
        if (disk == null) { return; }
        if (log) { Debug.Log($"(UI_HDD) setting disk {disk.name}"); }
        disk_letter.text = disk.DiskLetter;


        // we clear the files parent
        foreach (UI_Text text in ui_texts) { Destroy(text.gameObject); }
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

        // we update the capacity slider
        capacity_slider.sizeDelta = new Vector2(
            capacity_slider_max_width * (disk.SpaceUsed / (float)disk.Capacity),
            capacity_slider.sizeDelta.y);
        capacity_slider.GetComponent<Image>().color = Color.Lerp(
            capacity_slider_empty_color,
            capacity_slider_full_color,
            disk.SpaceUsed / (float)disk.Capacity);
        capacity_text.text = get_space_left(disk);

        // we update the temp slider
        temperer = disk.Capable.GetCapacity<TempCapacity>();
        update_temp_slider();
    }
    private void create_filename(File file)
    {
        UI_Text text = Instantiate(filename_prefab, files_parent).GetComponent<UI_Text>();
        // text.GetComponent<TextMeshProUGUI>().text = file.name + file.extension;
        text.GetComponent<UI_HDD_File>().SetFile(file);
        ui_texts.Add(text);
    }
    private string get_space_left(StoreCapacity disk)
    {
        return get_bytes_string_from_int(disk.SpaceLeft) + " free of " + get_bytes_string_from_int(disk.Capacity);
    }
    private string get_bytes_string_from_int(int bytes)
    {
        if (bytes < 1000) { return $"{bytes} bytes"; }
        else if (bytes < 1_000_000) { return $"{(bytes / 1_000f).ToString("f1")} KB"; }
        else if (bytes < 1_000_000_000) { return $"{(bytes / 1_000_000f).ToString("f1")} MB"; }
        else { return $"{(bytes / 1_000_000_000f).ToString("f1")} GB"; }
    }

    // UPDATE
    private void Update()
    {
        if (!Showed) { return; }
        if (temperer == null) { return; }
        update_temp_slider();
    }
    private void update_temp_slider()
    {
        temp_slider.sizeDelta = new Vector2(
            temp_slider.sizeDelta.x,
            temp_slider_max_height * (temperer.Temp / 100f));
        temp_slider.GetComponent<Image>().color = Color.Lerp(
            temp_slider_empty_color,
            temp_slider_full_color,
            temperer.Temp / temperer.MaxTemp);
        temp_text.text = $"{Mathf.RoundToInt(temperer.Temp)}°";
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

    // ENABLING
    protected override IEnumerator enable_coroutine()
    {
        // on active le navigator
        // UI_Navigator.Instance.Enable(this);
        slottable.Enable(ingame: false);
        yield break;
    }
    protected override IEnumerator disable_coroutine()
    {
        // on désactive le navigator
        // UI_Navigator.Instance.Disable(this);
        slottable.Disable();
        yield break;
    }

    // SLOTTABLE
    /* public List<UI_Slot> GetSlots()
    {
        if (log) { Debug.Log($"(UI_HDD) getting slots"); }
        List<UI_Slot> slots = new List<UI_Slot>();

        // we add the ui_texts
        foreach (UI_Text text in ui_texts)
        {
            // if (text == null || text.GetComponent<UI_Text>() == null) { continue; }
            slots.Add(text);
        }

        return slots;
    }
    public bool IsYourSlot(UI_Slot slot)
    {
        if (slot.transform.parent == files_parent) { return true; }
        return false;
    } */
    // public Vector2 SavedPosition { get; private set; } = Vector2.zero;

}