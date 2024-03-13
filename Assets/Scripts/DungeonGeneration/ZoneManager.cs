using UnityEngine;
using System.Collections.Generic;
// using System.Linq;
using System.IO;


public class ZoneManager : MonoBehaviour
{

    [Header("Bank")]
    private string prefabs_path = "Assets/Resources/prefabs/zones/";
    private bool is_loaded = false;



    [Header("Zones")]
    public Dictionary<Vector2Int, List<GameObject>> zones = new Dictionary<Vector2Int, List<GameObject>>();
    public Dictionary<Vector2Int, List<GameObject>> doors = new Dictionary<Vector2Int, List<GameObject>>();
    public List<GameObject> legendary_zones = new List<GameObject>();
    public List<GameObject> keys_zones = new List<GameObject>();





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
        

        // on récupère les légendaires
        foreach (string file in Directory.GetFiles(prefabs_path + "legendary/"))
        {
            if (file.EndsWith(".prefab"))
            {
                legendary_zones.Add(Resources.Load<GameObject>(file.Replace("Assets/Resources/","").Replace(".prefab","").Replace("\\","/")));
            }
        }

        // on récupère les portes
        List<GameObject> front_doors = new List<GameObject>();
        List<GameObject> side_doors = new List<GameObject>();
        foreach (string file in Directory.GetFiles(prefabs_path + "doors/"))
        {
            if (file.EndsWith(".prefab"))
            {
                GameObject door = Resources.Load<GameObject>(file.Replace("Assets/Resources/","").Replace(".prefab","").Replace("\\","/"));
                door.name += "_door";

                if (file.Contains("side"))
                {
                    side_doors.Add(door);
                }
                else
                {
                    front_doors.Add(door);
                }
            }
        }
        // add doors to the bank
        doors.Add(new Vector2Int(2, 1), front_doors);
        doors.Add(new Vector2Int(1, 2), side_doors);


        // on récupère les clés
        foreach (string file in Directory.GetFiles(prefabs_path + "keys/"))
        {
            if (file.EndsWith(".prefab"))
            {
                keys_zones.Add(Resources.Load<GameObject>(file.Replace("Assets/Resources/", "").Replace(".prefab", "").Replace("\\", "/")));
            }
        }

        is_loaded = true;
    }


    // GETTERS
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

    public List<GameObject> GetLegendaryZones()
    {
        if (!is_loaded) { Awake(); }
        return legendary_zones;
    }

    public GameObject GetDoor(Vector2Int size,string type="")
    {
        // on load les portes si ce n'est pas déjà fait
        if (!is_loaded) { Awake(); }

        // on vérifie que la taille existe dans la banque
        if (!doors.ContainsKey(size))
        {
            Debug.LogWarning("ZoneManager.GetDoor: size "+size+" not found in the bank");
            return null;
        }

        // on récupère les portes de la taille
        List<GameObject> door_prefabs = doors[size];

        // on filtre les portes par type
        if (type != "")
        {
            door_prefabs = door_prefabs.FindAll(door => door.name.Contains(type));
        }

        GameObject door = door_prefabs[Random.Range(0, door_prefabs.Count)];
        // Debug.Log("ZoneManager.GetDoor: " + size + " found in the bank : " + door.name);

        // on retourne une porte aléatoire dans la liste finale
        return door;
    }


    // PRECISE GETTERS
    public GameObject GetZoneByName(string name)
    {
        if (!is_loaded) { Awake(); }

        // on cherche dans les zones
        foreach (Vector2Int size in zones.Keys)
        {
            foreach (GameObject zone in zones[size])
            {
                if (zone.name == name)
                {
                    return zone;
                }
            }
        }

        // on cherche dans les légendaires
        foreach (GameObject zone in legendary_zones)
        {
            if (zone.name == name)
            {
                return zone;
            }
        }

        // on cherche dans les portes
        foreach (Vector2Int size in doors.Keys)
        {
            foreach (GameObject door in doors[size])
            {
                if (door.name == name)
                {
                    return door;
                }
            }
        }

        // on cherche dans les clés
        foreach (GameObject key in keys_zones)
        {
            if (key.name == name)
            {
                return key;
            }
        }

        Debug.LogWarning("ZoneManager.GetZoneByName: zone "+name+" not found in the bank");
        return null;
    }

}