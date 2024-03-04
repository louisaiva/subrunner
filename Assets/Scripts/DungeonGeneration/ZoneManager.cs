using UnityEngine;
using System.Collections.Generic;
// using System.Linq;
using System.IO;


public class ZoneManager : MonoBehaviour
{
    
    // bank of zones
    public Dictionary<Vector2Int,List<GameObject>> zones = new Dictionary<Vector2Int,List<GameObject>>();
    private string prefabs_path = "Assets/Resources/prefabs/zones/";

    // Start is called before the first frame update
    void Awake()
    {
        // load all folders from the prefabs folder
        foreach (string path in Directory.GetDirectories(prefabs_path))
        {
            // get the size of the zone
            string[] parts = path.Split('/');
            string[] size = parts[parts.Length-1].Split('x');
            try
            {
                int.Parse(size[0]);
                int.Parse(size[1]);
            }
            catch
            {
                continue;
            }

            Vector2Int zone_size = new Vector2Int(int.Parse(size[0]),int.Parse(size[1]));

            // load all prefabs from the folder
            List<GameObject> zone_prefabs = new List<GameObject>();
            foreach (string file in Directory.GetFiles(path))
            {
                if (file.EndsWith(".prefab"))
                {
                    zone_prefabs.Add(Resources.Load<GameObject>(file));
                }
            }

            // add the zone to the bank
            zones.Add(zone_size,zone_prefabs);
        }
    }

    public GameObject GetZone(Vector2Int size)
    {
        // we check if the size exists in the bank
        if (!zones.ContainsKey(size))
        {
            Debug.LogWarning("ZoneManager.GetZone: size " + size + " not found in the bank");
            return null;
        }

        // get a random zone from the bank
        List<GameObject> zone_prefabs = zones[size];
        return zone_prefabs[Random.Range(0,zone_prefabs.Count)];
    }


}