using UnityEngine;
using System.Collections.Generic;

public class KeyManager : MonoBehaviour
{
    
    // cette classe stocke les paires de doors / keys et les répartit à la génération du monde afin d'assurer une progression metroidvania

    [Header("Doors & Keys")]
    private Dictionary<string,string> keys_and_doors_names = new Dictionary<string, string>()
                                                            {
                                                                {"NOOD_OS","hackable_door"}
                                                            };
    [SerializeField] private Dictionary<GameObject, GameObject> keys_and_doors = new Dictionary<GameObject, GameObject>();

    // BANKS
    private ZoneManager bank_zone;

    void Awake()
    {
        // on récup le manager de zones
        bank_zone = GameObject.Find("/world").GetComponent<ZoneManager>();

        // on récupère les portes et les clés
        // keys_and_doors.Add(bank_zone.GetDoor("small", "key"), bank_zone.GetDoor("small", "door"));
        foreach ( KeyValuePair<string, string> key_and_door in keys_and_doors_names )
        {
            GameObject key = bank_zone.GetZoneByName(key_and_door.Key);
            GameObject door = bank_zone.GetZoneByName(key_and_door.Value);

            keys_and_doors.Add(key, door);
        }
    }

}