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

    // START
    public void Init(Hack hack)
    {
        this.hack = hack;
        cores_cost.text = hack.exploit.cores_cost.ToString();
        cores_cost.gameObject.SetActive(hack.exploit.cores_cost > 1);
        progress.sizeDelta = new Vector2(0, progress.sizeDelta.y);
        label.text = hack.exploit.name;
    }

    // UPDATE
    void Update()
    {
        if (hack == null) { return; }
        progress.sizeDelta = new Vector2(hack.progress / 100f * max_progress_width, progress.sizeDelta.y);

        if (hack.state == HackState.Failed || hack.state == HackState.Overflowed)
        {
            progress.GetComponent<Image>().color = failed_color;
        }
        else if (hack.state == HackState.Completed)
        {
            progress.GetComponent<Image>().color = completed_color;
        }
    }
}