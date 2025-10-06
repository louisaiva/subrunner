using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HackInfo : MonoBehaviour
{
    [Header("UI_HackInfo")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI cores_cost;
    [HideInInspector] public Hack hack;

    [Header("Progress")]
    [SerializeField] private RectTransform progress;
    [SerializeField] private float max_progress_width = 150f;
    [SerializeField] private Color completed_color = Color.green;
    [SerializeField] private Color failed_color = Color.red;
    [SerializeField] private Color waiting_color = Color.yellow;

    // START
    public void Init(Hack hack)
    {
        this.hack = hack;
        cores_cost.text = hack.cost.ToString();
        cores_cost.gameObject.SetActive(hack.cost > 1);
        progress.sizeDelta = new Vector2(0, progress.sizeDelta.y);
        label.text = hack.name;
    }

    // UPDATE
    void Update()
    {
        if (hack == null) { return; }
        progress.sizeDelta = new Vector2(hack.progress / 100f * max_progress_width, progress.sizeDelta.y);

        if (hack.state == ProcessusState.Failed)
        {
            progress.GetComponent<Image>().color = failed_color;
        }
        else if (hack.state == ProcessusState.Completed)
        {
            progress.GetComponent<Image>().color = completed_color;
        }
        else if (hack.state == ProcessusState.Waiting)
        {
            progress.GetComponent<Image>().color = waiting_color;
        }
    }
}