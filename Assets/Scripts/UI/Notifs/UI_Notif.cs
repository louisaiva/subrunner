#pragma warning disable 4014
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Notif : MonoBehaviour
{
    [Header("Tweening & durations")]
    private Transitioner transitioner;
    [SerializeField] private float time_to_live = 3f;

    [Header("Items/Files settings")]
    [SerializeField] private TextMeshProUGUI new_text;
    [SerializeField] private TextMeshProUGUI element_name;
    [SerializeField] private Image element_icon;

    public Item Item = null;
    public File File = null;

    // INIT
    public void Init(Item item)
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }

        set_item(item);

        // show the notif
        transitioner.Show();

        // hide after a while
        Invoke("HideAndDestroy", time_to_live);
    }
    public void Init(File file)
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }
        set_file(file);

        // show the notif
        transitioner.Show();

        // hide after a while
        Invoke("HideAndDestroy", time_to_live);
    }
    public void HideAndDestroy() => transitioner.HideAndDestroy();

    // SETUP
    private void set_item(Item item)
    {
        new_text.text = "new item : ";

        // get the icon
        Sprite icon = ItemBank.Instance.GetSprite(item);
        element_icon.sprite = icon;
        element_name.text = item.Reference;
        element_name.color = item.Color;

        Item = item;
    }
    private void set_file(File file)
    {
        new_text.text = "new file : ";

        // get the icon
        element_icon.sprite = file.icon;
        element_name.text = file.name;

        File = file;
    }

    // add another item
    private int notif_count = 1;
    public void AddDuplicate(bool is_item=true)
    {
        notif_count++;
        new_text.text = (is_item ? "new item x" : "new file x") + notif_count + " : ";

        // we reset the time to live
        CancelInvoke("HideAndDestroy");
        Invoke("HideAndDestroy", time_to_live);
    }
}