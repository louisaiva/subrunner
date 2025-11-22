using UnityEngine;

public class Shelf : Chest
{
    // CHEST
    public override bool is_open { get => true; }
    public override bool is_moving { get => false; }
}