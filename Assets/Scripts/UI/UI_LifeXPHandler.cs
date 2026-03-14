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

    // ON ENABLE
    private async void OnEnable()
    {
        // on désactive / reactive les cutout image de la barre de vie pour régler un léger bug
        life_fill.GetComponent<CutoutMaskUI>().enabled = false;
        xp_fill.GetComponent<CutoutMaskUI>().enabled = false;
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        life_fill.GetComponent<CutoutMaskUI>().enabled = true;
        xp_fill.GetComponent<CutoutMaskUI>().enabled = true;
    }

    // UPDATE
    private void Update()
    {
        // on met à jour le fill de life
        update_life_fill();

        // on met à jour le fill de xp
        update_xp_fill();

    }

    // UPDATE FILL LOW METHODS
    private void update_life_fill()
    {
        // on récupère les infos du perso
        float life_percent = Perso.Instance.LifePourcent;

        // on met à jour la taille du fill
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
