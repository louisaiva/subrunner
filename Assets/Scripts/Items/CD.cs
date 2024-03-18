using UnityEngine;

public class CD : FileHolder
{

    // unity functions
    public override void Start()
    {
        base.Start();

        // on met un max de 1 fichier
        this.max_files = 1;
    }

}