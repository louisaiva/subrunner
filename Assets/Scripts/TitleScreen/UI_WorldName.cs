using UnityEngine;

public class UI_WorldName : MonoBehaviour
{

    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    [Header("Components")]
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private void OnEnable()
    {
        if (text == null) { return; }
        if (string.IsNullOrEmpty(World.ID)) { text.text = ""; return; }

        // on met à jour le texte
        text.text = prefix.Grey() + "<b>" + World.ID + "</b>" + suffix.Grey();
    }
}