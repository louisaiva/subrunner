using UnityEngine;

public class Spawner : Capable
{
    // ON INTERACT
    public void OnInteract()
    {
        if (Can("spawn")) { Do("spawn"); }
        else { Debug.Log("Spawner has been interacted !"); }
    }

}