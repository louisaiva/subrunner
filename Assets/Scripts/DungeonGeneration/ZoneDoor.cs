using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class ZoneDoor : Zone
{
    public override void INIT(Vector2Int size, Vector2 position, Transform parent)
    {
        this.size = size;
        transform.SetParent(parent);
        transform.localPosition = new Vector3(position.x, position.y, 0) + anchor;
        // SetRandomZone();
        SetRandomDoor();


        // on applique la size à la ground_light
        GameObject ground_light = transform.Find("ground_light").gameObject;
        ground_light.GetComponent<GroundLight>().size = size;
    }


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
}