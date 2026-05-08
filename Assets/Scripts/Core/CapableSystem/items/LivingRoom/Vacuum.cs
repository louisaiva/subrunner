using UnityEngine;

public class Vacuum : Item, Usable
{
    private bool on = false;

    public string UseLabel => on ? "turn off" : "turn on";

    public void Use(Capable user)
    {
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