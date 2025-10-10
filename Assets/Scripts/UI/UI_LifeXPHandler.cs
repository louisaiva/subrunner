using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LifeXPHandler : MonoBehaviour
{
    // life filling bar
    public RectTransform life_fill;
    private float life_fill_max_width = 192;
    private float life_fill_height = 16;

    // xp filling bar
    public RectTransform xp_fill;
    private float xp_fill_max_width = 182;
    private float xp_fill_height = 8;


    // AWAKE & START
    private void Awake()
    {
        if (life_fill == null || xp_fill == null)
        {
            life_fill = transform.Find("life_fill").GetComponent<RectTransform>();
            xp_fill = transform.Find("xp_fill").GetComponent<RectTransform>();
        }
    }
    private void Start()
    {
        // on récupère la taille des fills
        life_fill_max_width = life_fill.sizeDelta.x;
        life_fill_height = life_fill.sizeDelta.y;

        xp_fill_max_width = xp_fill.sizeDelta.x;
        xp_fill_height = xp_fill.sizeDelta.y;

        // on met à jour les fills
        update_life_fill();
        update_xp_fill();
    }

    // UPDATE
    private void Update()
    {
        // ! on check si le perso est mort
        if (!Perso.Instance || !Perso.Instance.Alive)
        {
            // on met à zero
            life_fill.sizeDelta = new Vector2(0, life_fill_height);
            return;
        }

        // on met à jour le fill de life
        update_life_fill();

        // on met à jour le fill de xp
        update_xp_fill();

    }

    // UPDATE FILL LOW METHODS
    private void update_life_fill()
    {

        // on récupère les infos du perso
        int max_life = Perso.Instance.max_life;
        float life = Perso.Instance.life;

        // on met à jour la taille du fill
        float life_percent = life / max_life;
        float life_width = life_fill_max_width * life_percent;

        // on met à jour la taille du fill
        life_fill.sizeDelta = new Vector2(life_width, life_fill_height);
    }
    private void update_xp_fill()
    {

        // on récupère les infos du perso
        int max_xp = Perso.Instance.xp_to_next_level;
        float xp = (float) Perso.Instance.xp;

        // on met à jour la taille du fill
        float xp_percent = xp / max_xp;
        float xp_width = xp_fill_max_width * xp_percent;

        // on met à jour la taille du fill
        xp_fill.sizeDelta = new Vector2(xp_width, xp_fill_height);
    }
}
