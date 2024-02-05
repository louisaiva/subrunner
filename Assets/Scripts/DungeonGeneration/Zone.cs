using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class Zone : MonoBehaviour
{

    // une Zone applique des règles de génération particulière à des Area.
    // place des items, etc.
    // s'applique à la génération du monde
    // peut modifier les tilemaps

    protected World world;
    // protected Area area;
    protected Transform objects_parent;

    void Start()
    {
        // on récupère le monde
        world = GameObject.Find("/world").GetComponent<World>();

        // on récupère le parent des objets
        objects_parent = transform.Find("obj");
    }

    public void PlaceZone(Area area)
    {
        // on place la zone dans l'Area
        foreach (Transform child in transform.Find("obj"))
        {
            area.PlaceObject(child.gameObject,child.position);
        }
    }

}