using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LifeXPHandler : MonoBehaviour
{

    // GAMEOBJECTS
    public GameObject perso;


    // life filling bar
    public GameObject life_fill;
    private float life_fill_max_width = 192;
    private float life_fill_height = 16;

    // xp filling bar
    public GameObject xp_fill; // exploits points = experience points
    private float xp_fill_max_width = 182;
    private float xp_fill_height = 8;


    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le fill des life
        life_fill = transform.Find("life_fill").gameObject;

        // on récupère le fill des xp
        xp_fill = transform.Find("xp_fill").gameObject;

    }

    // Update is called once per frame
    void Update()
    {

        // ! on check si le perso est mort
        if (!perso || !perso.GetComponent<Perso>().isAlive()) { return; }

        // on met à jour le fill de vie
        update_life_fill();

        // on met à jour le fill de xp
        update_xp_fill();

    }

    void update_life_fill()
    {

        // on récupère les infos du perso
        int max_life = perso.GetComponent<Perso>().max_vie;
        float life = perso.GetComponent<Perso>().vie;

        // on met à jour la taille du fill
        float life_percent = life / max_life;
        float life_width = life_fill_max_width * life_percent;

        // on met à jour la taille du fill
        life_fill.GetComponent<RectTransform>().sizeDelta = new Vector2(life_width, life_fill_height);
    }

    void update_xp_fill()
    {

        // on récupère les infos du perso
        int max_xp = perso.GetComponent<Perso>().xp_to_next_level;
        float xp = (float) perso.GetComponent<Perso>().xp;

        // on met à jour la taille du fill
        float xp_percent = xp / max_xp;
        float xp_width = xp_fill_max_width * xp_percent;

        // on met à jour la taille du fill
        xp_fill.GetComponent<RectTransform>().sizeDelta = new Vector2(xp_width, xp_fill_height);
    }
}
