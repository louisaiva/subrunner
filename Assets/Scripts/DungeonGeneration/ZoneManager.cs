using UnityEngine;
using System.Collections.Generic;
// using System.Linq;
using System.IO;


public class ZoneManager : MonoBehaviour
{
    
    // bank of zones
    public Dictionary<Vector2Int,List<GameObject>> zones = new Dictionary<Vector2Int,List<GameObject>>();
    private string prefabs_path = "Assets/Resources/prefabs/zones/";
    private bool is_loaded = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (is_loaded) { return; }

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
                    zone_prefabs.Add(Resources.Load<GameObject>(file.Replace("Assets/Resources/","").Replace(".prefab","").Replace("\\","/")));
                }
            }

            // add the zone to the bank
            zones.Add(zone_size,zone_prefabs);
        }

        /* string s = "ZoneManager: " + zones.Count + " zones loaded : \n";
        foreach (Vector2Int size in zones.Keys)
        {
            s += size + " : " + string.Join(", ",zones[size].ConvertAll(x => x.name)) + "\n";
        }

        // Debug.Log("ZoneManager: " + zones.Count + " zones loaded : \n" + string.Join(", ",zones.Keys));
        Debug.Log(s); */
        is_loaded = true;
    }

    public GameObject GetZone(Vector2Int size)
    {
        if (!is_loaded)
        {
            Awake();
        }

        // we check if the size exists in the bank
        if (!zones.ContainsKey(size))
        {
            // we get the closest smaller size
            size = GetSmallerClosestSize(size);
            if (size == Vector2Int.zero)
            {
                Debug.LogWarning("ZoneManager.GetZone: size not found in the bank");
                return null;
            }
        }

        // get a random zone from the bank
        List<GameObject> zone_prefabs = zones[size];
        // Debug.Log("ZoneManager.GetZone: " + size + " found in the bank : " + zone.name);
        // return Instantiate(zone_prefabs[Random.Range(0, zone_prefabs.Count)]);
        return zone_prefabs[Random.Range(0, zone_prefabs.Count)];
    }

    private Vector2Int GetSmallerClosestSize(Vector2Int size)
    {
        if (!is_loaded) { Awake(); }

        // we check if the size exists in the bank
        if (zones.ContainsKey(size)) { return size; }

        // we get the closest smaller size
        float min_distance = float.MaxValue;
        Vector2Int closest_size = size;
        foreach (Vector2Int zone_size in zones.Keys)
        {
            if (zone_size.x > size.x || zone_size.y > size.y) { continue; }

            float distance = Vector2Int.Distance(size,zone_size);
            if (distance < min_distance)
            {
                min_distance = distance;
                closest_size = zone_size;
            }
        }

        if (zones.ContainsKey(closest_size))
        {
            return closest_size;
        }
        else
        {
            // Debug.LogWarning("ZoneManager.GetSmallerClosestSize: closest size not found in the bank");
            return Vector2Int.zero;
        }
    }

}