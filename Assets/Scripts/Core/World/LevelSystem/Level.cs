using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("rooms IDs")]
    public string[] rooms_ids;
    private void Start()
    {
        // for now we are a dummy we only tell the RoomSystem to
        // load all the rooms based on their data ids
        RoomSystem.Instance.LoadRooms(rooms_ids);
    }    
}