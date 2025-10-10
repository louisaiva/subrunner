using UnityEngine;
using TMPro;

public class UI_LvlHandler : MonoBehaviour
{
    
    [Header("UI Level Handler Parameters")]
    [SerializeField] private string levelPrefix = "level ";
    [HideInInspector] private TextMeshProUGUI levelText;

    // [Header("Splitted Level Prefix")]
    // [SerializeField] private TextMeshProUGUI levelPrefixText;

    // AWAKE & START
    private void Awake()
    {
        if (!levelText) { levelText = GetComponent<TextMeshProUGUI>(); }
    }

    private void Update()
    {
        if (!Perso.Instance) { return; }
        string lvl = Perso.Instance.level.ToString();

        // on met à jour le niveau
        // GetComponent<TextMeshProUGUI>().text = "level " + lvl;
        levelText.text = levelPrefix + lvl;
    }
}