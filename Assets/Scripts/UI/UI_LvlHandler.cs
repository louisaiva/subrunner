using UnityEngine;
using TMPro;

public class UI_LvlHandler : MonoBehaviour
{
    
    void Update()
    {
        if (!Perso.Instance) { return; }
        string lvl = Perso.Instance.level.ToString();

        // on met à jour le niveau
        GetComponent<TextMeshProUGUI>().text = "level " + lvl;
    }
}