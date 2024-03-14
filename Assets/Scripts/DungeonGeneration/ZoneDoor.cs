using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class ZoneDoor : Zone
{

    [Header("ZoneDoor")]
    public int reachability = 0;
    public bool is_vertical = false;
    private KeyManager key_manager;

    // unity awake
    new void Awake()
    {
        base.Awake();
        // on récupère le manager de zones
        key_manager = GameObject.Find("/world").GetComponent<KeyManager>();
    }

    // init 
    public void INIT(Sector sector, Transform parent,Vector2Int size, Vector2 position,bool verticality)
    {
        this.sector = sector;
        this.size = size;
        transform.SetParent(parent);
        transform.localPosition = new Vector3(position.x, position.y, 0) + anchor;
        this.is_vertical = verticality;
        

        // on applique la reachability
        // this.reachability = reachability;
        SetRandomDoor();


        // on applique la size à la ground_light
        GameObject ground_light = transform.Find("ground_light").gameObject;
        ground_light.GetComponent<GroundLight>().size = size;
    }

    public override void HANDMADE_INIT(Sector sector)
    {
        this.sector = sector;

        // SetRandomDoor();
        SetReachableDoor();
    }


    // generate
    public override void GENERATE()
    {
        if (objects_parent == null)
        {
            Debug.LogWarning("(ZoneDoor) GENERATE : objects_parent is null (size : " + size + ")");
            return;
        }

        // on place les objets dans notre transform en conservant leur localposition
        foreach (Transform child in objects_parent)
        {
            // on instancie l'objet
            Vector3 pos = child.localPosition;
            GameObject obj = Instantiate(child.gameObject, transform);
            obj.transform.localPosition = pos;
        }

        // Debug.Log("(Zone) GENERATE " + size + " at " + transform.position);
    }


    // main functions
    public void SetRandomDoor()
    {
        // on load la zone depuis le fichier prefab en fonction de sa taille
        GameObject zone = bank.GetDoor(size,"auto");
        if (zone == null)
        {
            Debug.Log("Door " + size + " not found");
            return;
        }

        objects_parent = zone.transform.Find("obj");
        gameObject.name = objects_parent.name;
    }

    public void SetReachableDoor()
    {
        // set the door based on its reachability
        GameObject zone = key_manager.GetDoor(reachability, is_vertical);
        if (zone == null)
        {
            Debug.Log("Door " + reachability + " not found");
            return;
        }

        objects_parent = zone.transform.Find("obj");
        gameObject.name = objects_parent.name;
    }

}