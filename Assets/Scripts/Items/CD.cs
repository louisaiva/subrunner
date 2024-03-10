using UnityEngine;

public class CD : FileHolder
{
    
    // unity functions
    protected new void Awake()
    {
        base.Awake();

        // on met un max de 1 fichier
        this.max_files = 1;
    }

}