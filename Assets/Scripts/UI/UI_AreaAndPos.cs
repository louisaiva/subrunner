using UnityEngine;
using TMPro;

public class UI_AreaAndPos : MonoBehaviour {

    // private Perso perso;
    // private Minimap minimap;
    private OldWorld world;

    // unity functions
    void Awake()
    {
        // on récupère le perso
        // perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère la minimap
        // minimap = GameObject.Find("/perso/minimap").GetComponent<Minimap>();

        // on récupère le world
        world = GameObject.Find("/world").GetComponent<OldWorld>();
    }

    void Update()
    {
    
        // on récupère l'area_name au niveau du perso
        string area_name = world.getPersoAreaName();

        // on met à jour le texte de l'area
        transform.Find("area_name").GetComponent<TextMeshProUGUI>().text = area_name;

        // on met à jour la local tile pos
        transform.Find("tile_pos").GetComponent<TextMeshProUGUI>().text = world.getPersoAreaPos() + " " + world.getPersoTilePos();
    }
}