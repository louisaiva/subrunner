using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Perso perso;
    public List<Room> rooms = new List<Room>();

    private void Start()
    {
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère les rooms
        foreach (Transform child in transform)
        {
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                rooms.Add(room);
            }
        }

        foreach (Room room in rooms)
        {
            room.HideLights();
        }

        perso.current_room = findPersoRoom();
        perso.current_room.ShowLights();
    }

    private void Update()
    {
        perso.current_room = findPersoRoom();
    }

    private Room findPersoRoom()
    {
        // définit la room du perso en faisant un raycast
        // get the position of the perso
        Vector2 perso_position = perso.transform.position;
        
        // we check if the perso is in a room
        foreach (Room room in rooms)
        {
            if (room.roomCollider.OverlapPoint(perso_position))
            {
                return room;
            }
        }

        return null;
    }
}