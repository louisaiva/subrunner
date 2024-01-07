using UnityEngine;

public class HackinrayHooverManager : MonoBehaviour
{
    // on récupère le line renderer
    private LineRenderer line_renderer;

    // targets
    private GameObject perso;
    [SerializeField] private GameObject target;

    // unity functions
    void Start()
    {
        // on récupère le line renderer
        line_renderer = GetComponent<LineRenderer>();

        // on récupère le perso
        perso = transform.parent.gameObject;
    }

    void Update()
    {
        if (!line_renderer.enabled) { return;}

        // on met à jour la position du line renderer
        line_renderer.SetPosition(0, perso.transform.Find("center").position);
        line_renderer.SetPosition(1, target.transform.Find("hack_point").position);
    }

    // functions
    public void setTarget(GameObject target)
    {
        this.target = target;
    }

    // showing
    public void show()
    {
        // on active le line renderer
        line_renderer.enabled = true;
    }

    public void hide()
    {
        // on désactive le line renderer
        line_renderer.enabled = false;
    }
}