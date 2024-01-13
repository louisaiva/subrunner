using UnityEngine;


public class HackrayHoover : Hackray
{

    // targets
    private GameObject perso;

    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");
    }

    new void Update()
    {
        updateTransform();
    }

    // functions
    public void setTarget(GameObject target)
    {
        SetHackerAndTarget(perso, target);
    }

    public void removeTarget()
    {
        RemoveHackerAndTarget();
    }

    // showing
    public void show()
    {
        // on active le sprite rrenderer
        sr.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void hide()
    {
        // on désactive le sprite rrenderer
        sr.GetComponent<SpriteRenderer>().enabled = false;
    }
}