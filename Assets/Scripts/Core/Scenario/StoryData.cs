using System;
using UnityEngine;

[Serializable] public class StoryData
{
    public bool intro_done = false;
    public bool met_qwin = false;
    public bool ate_pasta = false;

    public StoryData()
    {
        intro_done = false;
        met_qwin = false;
        ate_pasta = false;
    }
}