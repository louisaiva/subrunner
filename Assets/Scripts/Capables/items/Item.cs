using UnityEngine;
/// <summary>
/// Item is a Movable that can be grabbed by other Capables with GrabCapacity.
/// </summary>
public class Item : Movable
{

    public bool Grabbed { get; set; }

    [Header("Grabbable")]
    [SerializeField] private Collider2D grab_collider;

    

}