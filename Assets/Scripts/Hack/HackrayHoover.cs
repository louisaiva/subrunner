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
        // on vérifie si on a pas déjà un target
        if (this.target != null)
        {
            if (this.target != perso)
            {
                // on désactive l'outline
                this.target.GetComponent<I_Hackable>().unOutlineMe();
            }
            else
            {
                // on désactive l'outline
                this.hacker.GetComponent<I_Hackable>().unOutlineMe();
            }
        }

        // on active l'outline
        target.GetComponent<I_Hackable>().outlineMe();

        // on set le nouveau target
        SetHackerAndTarget(perso, target);
    }

    public void removeTarget()
    {
        // on désactive l'outline
        if (this.target != null)
        {
            if (this.target != perso)
            {
                // on désactive l'outline
                this.target.GetComponent<I_Hackable>().unOutlineMe();
            }
            else
            {
                // on désactive l'outline
                this.hacker.GetComponent<I_Hackable>().unOutlineMe();
            }
        }

        // on set le nouveau target
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