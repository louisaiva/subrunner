#pragma warning disable 4014
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HackInfo : MonoBehaviour
{
    [Header("UI_HackInfo")]
    [SerializeField] private TextMeshProUGUI label;
    private Transitioner transitioner;
    
    [Header("Cores")]
    [SerializeField] private Image cores;
    [SerializeField] private TextMeshProUGUI cores_cost;
    [HideInInspector] public Hack hack;

    [Header("Progress")]
    [SerializeField] private ProcessusState state;
    public ProcessusState State { get { return state; } }
    [SerializeField] private RectTransform progress;
    private Image progress_image;
    [SerializeField] private float max_progress_width = 150f;

    [Header("Colors")]
    [SerializeField] private Color completed_color = Color.green;
    [SerializeField] private Color failed_color = Color.red;
    [SerializeField] private Color waiting_color = Color.yellow;


    // START
    public void Init(Hack hack)
    {
        if (transitioner == null) { transitioner = GetComponent<Transitioner>(); }
        if (progress_image == null) { progress_image = progress.GetComponent<Image>(); }
        this.hack = hack;

        // init cores
        cores_cost.text = hack.cost.ToString();
        cores_cost.gameObject.SetActive(hack.cost > 1);
        cores.color = failed_color;
        cores_cost.color = failed_color; // failed color but actually it's not failed yet just started -> will be red, means we use x cores

        // init progress
        progress.sizeDelta = new Vector2(0, progress.sizeDelta.y);
        label.text = hack.name;
        state = hack.state;

        // on affiche le hack info
        transitioner.Show();
    }

    // UPDATE
    void Update()
    {
        if (hack == null) { return; }
        progress.sizeDelta = new Vector2(hack.progress / 100f * max_progress_width, progress.sizeDelta.y);

        if (hack.state == state) { return; }
    
        // the state just changed, we update few things
        state = hack.state;
        OnHackStateChanged();
    }
    private void OnHackStateChanged()
    {
        if (hack.state == ProcessusState.Failed)
        {
            progress_image.color = failed_color;
            transitioner.HideAndDestroy();
        }
        else if (hack.state == ProcessusState.Completed)
        {
            progress_image.color = completed_color;
            transitioner.HideAndDestroy();
        }
        else if (hack.state == ProcessusState.Waiting)
        {
            progress_image.color = waiting_color;

            // we set the right cores settings provided_cores_count
            cores_cost.text = hack.provided_cores_count.ToString();
            cores_cost.gameObject.SetActive(hack.provided_cores_count > 1);

            // and the right cost color
            cores.color = waiting_color;
            cores_cost.color = waiting_color;
        }
    }
}