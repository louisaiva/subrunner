using UnityEngine;

public class Level : MonoBehaviour
{
    public Perso perso;
    public Transform CurrentPersoRoom;

    private void Start()
    {
        perso = GameObject.Find("/perso").GetComponent<Perso>();
    }

    public Transform GetPersoCurrentRoom()
    {
        return CurrentPersoRoom;
    }

    public void SetPersoCurrentRoom(Transform room)
    {
        CurrentPersoRoom = room;
    }
}