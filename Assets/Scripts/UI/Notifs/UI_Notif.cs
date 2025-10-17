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


    // START
    private void Awake()
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }
    }

    public void Init(Item item)
    {
        set_item(item);

        // show the notif
        transitioner.Show();

        // hide after a while
        Invoke("HideAndDestroy", time_to_live);
    }
    public void Init(File file)
    {
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
    }
    private void set_file(File file)
    {
        new_text.text = "new file : ";

        // get the icon
        element_icon.sprite = file.icon;
        element_name.text = file.name;
    }
}