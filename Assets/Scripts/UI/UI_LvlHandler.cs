using UnityEngine;
using TMPro;

public class UI_LvlHandler : MonoBehaviour {
    
    // perso
    private GameObject perso;

    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");
    }

    void Update()
    {

        string lvl = perso.GetComponent<Perso>().level.ToString();

        // on met à jour le niveau
        GetComponent<TextMeshProUGUI>().text = "level " + lvl;
    }
}