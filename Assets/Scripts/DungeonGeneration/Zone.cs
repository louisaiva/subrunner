using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class Zone : MonoBehaviour
{

    // une Zone applique des règles de génération particulière à des Area.
    // place des items, etc.
    // s'applique à la génération du monde


    // protected World world;
    protected ZoneManager bank;
    protected Transform objects_parent;
    public Vector3 anchor = new Vector3(4,4,0);

    public Vector2Int size;

    void Awake()
    {
        // on récupère le monde
        // world = GameObject.Find("/world").GetComponent<World>();

        // on récupère la banque de zones
        bank = GameObject.Find("/world").GetComponent<ZoneManager>();
    }


    // initialise la zone
    public void INIT(Vector2Int size, Vector2 position, Transform parent)
    {
        this.size = size;
        transform.SetParent(parent);
        transform.localPosition = new Vector3(position.x,position.y,0) + anchor;
        SetRandomZone();

        // on applique la size à la ground_light
        GameObject ground_light = transform.Find("ground_light").gameObject;
        ground_light.GetComponent<GroundLight>().size = size;
    }

    public void SetRandomZone()
    {
        // on load la zone depuis le fichier prefab en fonction de sa taille
        // string path = prefabs_path + size.x + "x" + size.y;
        GameObject zone = bank.GetZone(size);
        if (zone == null) { return; }

        objects_parent = zone.transform.Find("obj");
    }

    public void SetZone(string path)
    {
        // on set la zone depuis le fichier prefab en fonction de sa taille
        GameObject zone = Resources.Load<GameObject>(path);
        if (zone == null) { return; }

        objects_parent = zone.transform.Find("obj");
    }


    // crée la zone (récupère les objets)
    public void GENERATE()
    {
        if (objects_parent == null) { return; }

        // on place les objets dans notre transform en conservant leur localposition
        foreach (Transform child in objects_parent)
        {
            Vector3 pos = child.localPosition;
            child.SetParent(transform);
            child.localPosition = pos;
        }
    }

}
