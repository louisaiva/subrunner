using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DoorCaller : UI_TimelineCapableCaller
{
    [Header("Door Caller")]
    [SerializeField] private string room1;
    [SerializeField] private string room2; // rooms names to gather the door

    public override bool ConnectCapable()
    {
        Door door = RoomEngine.LazyInstance.DoorEngine.GetDoorBetweenRooms(room1, room2);
        if (door == null) { Debug.LogError("(UI_DoorCaller) No door found between rooms '" + room1 + "' and '" + room2 + "'"); return false; }
        capable = door;
        Debug.Log("(UI_TimelineCapableCaller) Connected door with id '" + door.ID + "'");
        return true;
    }

    // OPEN / CLOSE
    public void OpenDoor()
    {
        if (capable == null) { Debug.LogError("(UI_DoorCaller) No capable connected"); return; }
        if (capable is not Door door) { Debug.LogError("(UI_DoorCaller) Connected capable is not a door"); return; }
        door.Open();
    }
    public void CloseDoor()
    {
        if (capable == null) { Debug.LogError("(UI_DoorCaller) No capable connected"); return; }
        if (capable is not Door door) { Debug.LogError("(UI_DoorCaller) Connected capable is not a door"); return; }
        door.Close();
    }
}