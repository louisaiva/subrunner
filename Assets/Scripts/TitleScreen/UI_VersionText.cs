using UnityEngine;

public class UI_VersionText : MonoBehaviour
{

    [SerializeField] private bool showPrototype = false;

    [Header("Components")]
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private void Start()
    {
        if (text == null) { return; }

        // on met à jour le texte
        text.text = AppManager.Instance.GetFullVersion(showPrototype);
    }

}