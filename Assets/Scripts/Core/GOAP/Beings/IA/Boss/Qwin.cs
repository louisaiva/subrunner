using System;
using System.Linq;
using UnityEngine;

public class Qwin : IA
{
    // y'a r mdr
    protected override void Start()
    {
        base.Start();
        try
        {
            World.LazyInstance.data.story_data.met_qwin = true;
        }
        catch (Exception e) { Debug.LogError("(Qwin) Qwin Started but we could not set the world story data flag because : " + e);}
    }
}