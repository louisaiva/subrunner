using UnityEngine;
using System.Collections.Generic;

public class KeyManager : MonoBehaviour
{
    
    // cette classe stocke les paires de doors / keys et les répartit à la génération du monde afin d'assurer une progression metroidvania

    [Header("Doors & Keys")]
    private Dictionary<string,string> doors_and_keys = new Dictionary<string, string>()
                                                            {
                                                                {"auto","badge"},
                                                                {"hackable","NOOD_OS"},
                                                                {"button","carbon_shoes"}
                                                            };
    [SerializeField] private Dictionary<string, GameObject> doors = new Dictionary<string, GameObject>();
    [SerializeField] private Dictionary<string, GameObject> doorsLR = new Dictionary<string, GameObject>();
    [SerializeField] private Dictionary<string, GameObject> keys = new Dictionary<string, GameObject>();


    [Header("Pathvania")]
    [SerializeField] private List<string> pathvania = new List<string>()
                                                    {
                                                        "auto",
                                                        "hackable",
                                                        "button"
                                                    };

    // BANKS
    private ZoneManager bank_zone;

    void Awake()
    {
        // on récup le manager de zones
        bank_zone = GameObject.Find("/world").GetComponent<ZoneManager>();

        // on récupère les portes et les clés
        foreach ( KeyValuePair<string, string> door_and_key in doors_and_keys)
        {
            // ! attention door_and_key.Key est le nom de la porte, door_and_key.Value est le nom de la clé
            doors.Add(door_and_key.Key,bank_zone.GetZoneByName(door_and_key.Key+"_door"));
            doorsLR.Add(door_and_key.Key,bank_zone.GetZoneByName(door_and_key.Key+"_side_door"));
            keys.Add(door_and_key.Value,bank_zone.GetZoneByName(door_and_key.Value));
        }

        // on affiche les portes et les clés
        /* foreach (KeyValuePair<string, GameObject> door in doors)
        {
            Debug.Log("KeyManager.Awake: door : " + door.Key);
        }
        foreach (KeyValuePair<string, GameObject> door in doorsLR)
        {
            Debug.Log("KeyManager.Awake: door : " + door.Key);
        }
        foreach (KeyValuePair<string, GameObject> key in keys)
        {
            Debug.Log("KeyManager.Awake: key : " + key.Key);
        } */
    }

    public void applyPathVania(List<Sector> sectors)
    {
        // this function applies the pathvania to the specific sector
        // takes the doors and apply them to the sector, based on its reachability
        // then place one key in a random reachibility-1 sector
        
        // on sauvegarde les différents secteurs par reachability
        Dictionary<int, List<Sector>> reachability_sectors = new Dictionary<int, List<Sector>>();

        foreach(Sector sect in sectors)
        {
            // on ajoute sa reachability à la liste
            int reachability = sect.reachability;
            if (reachability > pathvania.Count) { reachability = pathvania.Count; }

            if (reachability_sectors.ContainsKey(reachability))
            {
                reachability_sectors[reachability].Add(sect);
            }
            else
            {
                reachability_sectors.Add(reachability, new List<Sector>(){sect});
            }


            // on place les bonnes portes
            List<ZoneDoor> sector_doors = sect.getDoorZones();
            foreach(ZoneDoor door in sector_doors)
            {
                // on récupère la reachability de la porte
                int door_reachability = door.reachability;
                if (door_reachability > pathvania.Count)
                {
                    // Debug.LogWarning("KeyManager.applyPathVania: door_reachability > pathvania.Count");
                    door_reachability = pathvania.Count;
                }

                // on applique la zone correspondante
                string door_name = pathvania[door_reachability-1];
                if (door.is_vertical)
                {
                    door.SetZone(doors[door_name]);
                }
                else
                {
                    door.SetZone(doorsLR[door_name]);
                }
            }
        }

        // on place les clés
        for (int r=1; r<pathvania.Count+1; r++)
        {
            // on récupère les secteurs de reachability r-1
            List<Sector> sects = reachability_sectors[r-1];
            if (sects.Count == 0) { continue; }

            // on récupère toutes les zones centrales
            List<Zone> central_zones = getUsableZones(sects);

            // affiche les zones centrales
            foreach (Zone z in central_zones)
            {
                Debug.Log("KeyManager.applyPathVania: central zone : " + z.name);
            }

            // on choisit une zone centrale aléatoire
            Zone zone = central_zones[Random.Range(0,central_zones.Count)];
            zone.SetZone(keys[doors_and_keys[pathvania[r-1]]]);
        }
    }


    // INTERNAL GETTERS
    private List<Zone> getUsableZones(List<Sector> sectors)
    {
        // cette fonction récupère toutes les zones utilisables
        List<Zone> zones = new List<Zone>();
        foreach(Sector sect in sectors)
        {
            zones.AddRange(sect.getAvailableCentralZones());
        }
        return zones;
    }
}