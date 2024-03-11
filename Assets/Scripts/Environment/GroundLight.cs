using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundLight : MonoBehaviour
{
    
    [Header("Ground Light")]
    [SerializeField] private Dictionary<string,Sprite> ground_lights;
    [SerializeField] private List<Transform> lights_go;
    [SerializeField] private GameObject light_prefab;

    [Header("Size")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2 old_size;


    private void Start()
    {
        // set prefab
        light_prefab = Resources.Load<GameObject>("prefabs/objects/lights/ground_light_solo");

        // load sprites
        List<Sprite> sprites = Resources.LoadAll<Sprite>("spritesheets/environments/lights/ground_light").ToList();
        ground_lights = new Dictionary<string,Sprite>();
        ground_lights.Add("LU",sprites[0]);
        ground_lights.Add("U",sprites[1]);
        ground_lights.Add("RU",sprites[2]);
        ground_lights.Add("L",sprites[3]);
        // ground_lights.Add("C",sprites[4]);
        ground_lights.Add("R",sprites[4]);
        ground_lights.Add("LD",sprites[5]);
        ground_lights.Add("D",sprites[6]);
        ground_lights.Add("RD",sprites[7]);

        lights_go = new List<Transform>();

        // create ground
        CreateGround();
    }

    private void Update()
    {
        if (old_size.x != width || old_size.y != height)
        {
            Clear();
            CreateGround();
        }

        old_size = new Vector2(width, height);
    }

    private void Clear()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreateGround()
    {
        // Debug.Log("(GroundLight) CreateGround: " + width + ", " + height);

        // create ground
        for (int x = 0; x < width; x++)
        {
            string type = "";

            if (x == 0)
            {
                type = "L";
            }
            else if (x == width - 1)
            {
                type = "R";
            }

            for (int y = 0; y < height; y++)
            {

                if (y == 0)
                {
                    AddLightTile(x, y, type+"D");
                }
                else if (y == height - 1)
                {
                    AddLightTile(x, y, type + "U");
                }
                else if (type != "")
                {
                    AddLightTile(x, y, type);
                }

                // Debug.Log("(GroundLight) CreateGround: " + x + ", " + y + " (" + type + ")");
            }
        }
    }

    private void AddLightTile(int x, int y, string type)
    {
        // create light
        GameObject light_go = Instantiate(light_prefab, new Vector3(x, y, 0), Quaternion.identity);
        light_go.transform.SetParent(transform);
        light_go.transform.localPosition = new Vector3(x/2f, y/2f, 0);
        light_go.GetComponent<SpriteRenderer>().sprite = ground_lights[type];
        lights_go.Add(light_go.transform);

        // on cache le sprite (test)
        light_go.GetComponent<SpriteRenderer>().enabled = false;

        // Debug.Log("(GroundLight) AddLightTile: " + x + ", " + y + " (" + type + ")");
    }


    // GETTERS & SETTERS
    public Vector2Int size
    {
        get
        {
            return new Vector2Int(width, height);
        }
        set
        {
            width = value.x;
            height = value.y;
        }
    }

}