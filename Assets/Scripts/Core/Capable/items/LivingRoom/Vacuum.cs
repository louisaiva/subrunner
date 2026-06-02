using UnityEngine;

public class Vacuum : Item, Usable
{
    private bool on = false;

    public string UseLabel => on ? "turn off" : "turn on";

    public void Use(Capable user)
    {
        AnimPlayer AnimPlayer = Visual as AnimPlayer;
        if (AnimPlayer == null) { Debug.LogWarning("Vacuum has no AnimPlayer!"); return; }

        AnimPlayer.Show();
        on = !on;
        if (on)
        {
            Debug.Log("(Vacuum) turned on");
            AnimPlayer.Play("on");
        }
        else
        {
            Debug.Log("(Vacuum) turned off");
            AnimPlayer.Play("off");
        }
    }
}