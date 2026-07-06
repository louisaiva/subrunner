#pragma warning disable 4014
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Notif : MonoBehaviour
{
    [Header("Tweening & durations")]
    protected Transitioner transitioner;
    [SerializeField] protected float time_to_live = 3f;
    [SerializeField] protected string notif_text = "test cedric";
    public string NotifText { get { return notif_text; } set { notif_text = value; } }

    [Header("Items/Files settings")]
    [SerializeField] protected TextMeshProUGUI tmp;


    // INIT
    public virtual void Init(string text)
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }

        tmp.text = text;

        // show the notif
        transitioner.Show();

        // hide after a while
        Invoke("HideAndDestroy", time_to_live);
    }
    public virtual void HideAndDestroy() => transitioner.HideAndDestroy();

    protected int notif_count = 1;
    public virtual void AddDuplicate()
    {
        notif_count++;
        tmp.text = notif_text + " " + notif_count;

        // we reset the time to live
        CancelInvoke("HideAndDestroy");
        Invoke("HideAndDestroy", time_to_live);
    }
}