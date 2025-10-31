using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_HDD_File : UI_Text
{
    [Header("File")]
    [SerializeField] private File file;

    [Header("Components")]
    private UI_HDD ui_hdd;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        ui_hdd = GetComponentInParent<UI_HDD>();
    }

    // SET FILE
    public void SetFile(File file)
    {
        this.file = file;
        if (file == null) { return; }
        if (log) { Debug.Log($"(UI_HDD_File) setting file {file.name}"); }

        // we set the text
        GetComponent<TextMeshProUGUI>().text = file.name + file.extension;
    }

    // INTERFACE FUNCTIONS
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (file == null) { return; }
        if (log) { Debug.Log($"(UI_HDD_File) hovering file {file.name}"); }

        ui_hdd.ShowFileData(file);
    }
}