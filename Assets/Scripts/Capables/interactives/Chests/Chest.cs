using UnityEngine;

public class Chest : Capable, Interactable
{
    // ON INTERACT
    public void OnInteract()
    {
        if (Can("open_close"))
        {
            Do("open_close");
        }
    }

}