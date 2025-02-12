using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Rooms")]
    public List<Room> rooms = new List<Room>();
    [SerializeField] private Room elevator_room;
    public Room RoomConnectedToElevator {
        get
        {
            if (elevator_room == null)
            {
                Debug.LogWarning("(Level) No elevator room found in " + name + ", please set it manually in the inspector");
                return rooms[0];
            }
            return elevator_room;
        } }

    [Header("Beings")]
    public Transform being_parent;

    [Header("Debug")]
    public bool loaded = false;

    // START
    public void Start()
    {
        if (loaded) { return; }

        // on récupère les rooms
        foreach (Transform child in transform)
        {
            // check is activeSelf
            if (!child.gameObject.activeSelf) { continue;}

            // check if it has a child
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                rooms.Add(room);
                if (!room.loaded) { room.Awake(); }
            }
        }

        // on trie les rooms par priorité
        rooms.Sort((b,a) => a.FindPriority.CompareTo(b.FindPriority));
        Debug.Log(name + " level sorted rooms by priority");

        // on récupère le parent des beings
        being_parent = transform.Find("beings");

        loaded = true;
    }

    // SHOW / HIDE
    public void Hide()
    {
        foreach (Room room in rooms)
        {
            room.Hide();
        }
    }
}