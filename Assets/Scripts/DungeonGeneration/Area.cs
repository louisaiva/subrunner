using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;


public class Area : MonoBehaviour
{
    [Header("Area")]    
    public int x;
    public int y;
    public int w = 16;
    public int h = 16;

    public string type; // exemple : room_U, corridor_LR, sas_ND, ...

    [Header("Sector")]
    protected Sector sector;
    protected Vector2Int area_size = new Vector2Int(16, 16);

    [Header("Neighbors")]
    protected Area[] neighbors = new Area[4]; // 0: top, 1: right, 2: bottom, 3: left

    [Header("Ceilings")]
    protected Dictionary<string, GameObject> ceilings = new Dictionary<string, GameObject>();


    // init functions
    public void init(int x, int y, int w, int h, string type, Sector sector)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.type = type;
        this.sector = sector;

        // on met le bon nom
        this.gameObject.name = "(" + x + ":" + y + ") " + type;

        // on met le bon parent
        this.transform.parent = sector.transform.Find("areas");

        // on met la bonne position
        int real_x = (sector.x + x) * area_size.x /2;
        int real_y = (sector.y + y) * area_size.y /2;
        this.transform.position = new Vector3(real_x, real_y, 0);
    }

    public void GENERATE()
    {

    }


}