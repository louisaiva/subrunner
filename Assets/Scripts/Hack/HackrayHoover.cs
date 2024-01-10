using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
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
        print("set target");
        SetHackerAndTarget(perso, target);
    }

    // showing
    public void show()
    {
        // on active le sprite rrenderer
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void hide()
    {
        // on désactive le sprite rrenderer
        GetComponent<SpriteRenderer>().enabled = false;
    }
}