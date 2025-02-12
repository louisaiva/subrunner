using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [Header("Wall")]
    public List<SpriteRenderer> walls;
    public List<SpriteRenderer> ceilings;

    [Header("SeeThrough")]
    [SerializeField] private bool seeing_through = false;
    [SerializeField] private float un_see_through_delay = 0.5f;

    [Header("Materials")]
    public Material walls_base;
    public Material walls_see_through;
    public Material ceilings_base;
    public Material ceilings_see_through;


    [Header("Debug")]
    public bool debug = false;


    // START
    public void Start()
    {
        // we find the walls and ceilings
        find_walls_n_ceilings();

        // we set the material of the walls and ceilings
        un_see_through();
    }
    public void find_walls_n_ceilings()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "walls")
            {
                foreach (Transform obj in child)
                {
                    SpriteRenderer wall = obj.GetComponent<SpriteRenderer>();
                    if (wall != null)
                    {
                        walls.Add(wall);
                    }
                }
            }
            else if (child.name == "ceilings")
            {
                foreach (Transform obj in child)
                {
                    SpriteRenderer ceiling = obj.GetComponent<SpriteRenderer>();
                    if (ceiling != null)
                    {
                        ceilings.Add(ceiling);
                    }
                }
            }
        }
    }


    // SEE THROUGH
    public void SeeThrough()
    {
        // we cancel the invoke
        CancelInvoke("un_see_through");

        // we are passing behind the wall, we need to see through it
        if (seeing_through) { return; }

        // we change all the walls and ceilings materials to see through
        foreach (SpriteRenderer wall in walls)
        {
            wall.material = walls_see_through;
        }
        foreach (SpriteRenderer ceiling in ceilings)
        {
            ceiling.material = ceilings_see_through;
        }

        seeing_through = true;
        if (debug) { Debug.Log("(Wall) " + name + " : SeeThrough"); }
    }
    public void UnSeeThrough()
    {
        Invoke("un_see_through", un_see_through_delay);
    }
    private void un_see_through()
    {
        if (!seeing_through) { return; }

        // we are not passing behind the wall anymore, we need to see it
        foreach (SpriteRenderer wall in walls)
        {
            wall.material = walls_base;
        }
        foreach (SpriteRenderer ceiling in ceilings)
        {
            ceiling.material = ceilings_base;
        }

        seeing_through = false;

        if (debug) { Debug.Log("(Wall) " + name + " : UnSeeThrough"); }
    }

}