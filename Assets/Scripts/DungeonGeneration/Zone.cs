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
    public Sector sector;
    protected Vector3 anchor = new Vector3(4,4,0);
    [SerializeField] protected Vector2Int size;
    [SerializeField] protected ZoneManager bank;
    [SerializeField] protected Transform objects_parent;



    protected void Awake()
    {
        // on récupère le monde
        // world = GameObject.Find("/world").GetComponent<World>();

        // on récupère la banque de zones
        bank = GameObject.Find("/world").GetComponent<ZoneManager>();
    }


    // initialise la zone
    public void INIT(Sector sector, Transform parent, Vector2Int size, Vector2 position)
    {
        this.sector = sector;
        this.size = size;

        transform.SetParent(parent);
        transform.localPosition = new Vector3(position.x,position.y,0) + anchor;
        SetRandomZone();

        // on applique la size à la ground_light
        GameObject ground_light = transform.Find("ground_light").gameObject;
        ground_light.GetComponent<GroundLight>().size = size;
    }

    public virtual void HANDMADE_INIT(Sector sector)
    {
        this.sector = sector;

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
        if (zone == null)
        {
            Debug.Log("Zone " + size + " not found");
            return;
        }

        objects_parent = zone.transform.Find("obj");
        gameObject.name = objects_parent.name;
    }
    public void SetZoneFromPath(string path)
    {
        // on set la zone depuis le fichier prefab en fonction de sa taille
        GameObject zone = Resources.Load<GameObject>(path);
        if (zone == null) { return; }

        objects_parent = zone.transform.Find("obj");
        gameObject.name = objects_parent.name;
    }
    public void SetZone(GameObject zone)
    {
        // on set la zone directement
        if (zone == null) { return; }
        objects_parent = zone.transform.Find("obj");
        gameObject.name = objects_parent.name;
    }


    // crée la zone (récupère les objets)
    public virtual void GENERATE()
    {
        if (objects_parent == null) { return; }

        // Debug.Log("(Zone) GENERATE " + size + " at " + transform.position);

        Vector3 lil_anchor = new Vector3(size.x > 1 ? size.x/4f : 0 , size.y > 1 ? size.y/4f : 0,0);

        if (size.y == 7f && size.x == 8f)
        {
            lil_anchor = new Vector3(2f,2f,0);
        }

        // on place les objets dans notre transform en conservant leur localposition
        foreach (Transform child in objects_parent)
        {
            // on instancie l'objet
            Vector3 pos = child.localPosition;
            GameObject obj = Instantiate(child.gameObject,transform);
            obj.transform.localPosition = pos + lil_anchor;
        }
    }


    // GETTERS
    public Vector2Int GetSize() { return size; }
}
