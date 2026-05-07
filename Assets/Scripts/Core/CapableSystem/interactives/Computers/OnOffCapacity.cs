using System.Collections;
using UnityEngine;

public class OnOffCapacity : Capacity
{
    

    [Header("On Off Animations")]
    [SerializeField] private string powering_on_animation = "onnin";
    [SerializeField] private string powering_off_animation = "offin";
    [SerializeField] private string idle_on_animation = "idle_on";
    [SerializeField] private string idle_off_animation = "idle";


    public Onnable Onnable => Capable as Onnable;


    // POWERING ON
    public virtual void PowerOn()
    {
        // checks if it is already powering on or already on
        if (!Onnable.IsMoving && Onnable.IsOn) { if (log) { Debug.Log("(Computer) " + Capable.name + " is already powered on..."); } return; }
        if (Onnable.IsMoving && !Onnable.IsOn) { if (log) { Debug.Log("(Computer) " + Capable.name + " is already powering on..."); } return; }

        // we start the powering on coroutine
        Capable.StopAllCoroutines();
        Capable.StartCoroutine(power_on());
    }
    public virtual async void PowerOff(float delay = 0f)
    {
        // checks if it is already powering off or already off
        if (!Onnable.IsMoving && !Onnable.IsOn) { if (log) { Debug.Log("(Computer) " + Capable.name + " is already powered off..."); } return; }
        if (Onnable.IsMoving && Onnable.IsOn) { if (log) { Debug.Log("(Computer) " + Capable.name + " is already powering off..."); } return; }

        if (delay != 0f) { await System.Threading.Tasks.Task.Delay((int)(delay * 1000)); }

        // we start the powering on coroutine
        Capable.StopAllCoroutines();
        Capable.StartCoroutine(power_off());
    }
    protected virtual IEnumerator power_on()
    {
        if (log) { Debug.Log("(Computer) " + Capable.name + " is powering on"); }

        // on allume l'ordi
        Onnable.IsOn = false;
        Onnable.IsMoving = true;
        AnimPlayer.Play(powering_on_animation);
        AnimPlayer.AddToPile(idle_on_animation);
        if (idle_off_animation != "idle") { AnimPlayer.StopPlaying(idle_off_animation); }

        // on attend la fin de l'anim
        while (AnimPlayer.IsShowing(powering_on_animation)) { yield return null; }

        // on allume l'ordi
        Onnable.IsOn = true;
        Onnable.IsMoving = false;
        if (log) { Debug.Log("(Computer) " + Capable.name + " powered on !!"); }
    }
    protected virtual IEnumerator power_off()
    {
        // if we wait here it will still stop all coroutines directly which is a problem -> better to wait in PowerOff()
        // if (delay > 0f) { yield return new WaitForSeconds(delay); }

        if (log) { Debug.Log("(Computer) " + Capable.name + " is powering off"); }

        // on éteint l'ordi
        Onnable.IsOn = true;
        Onnable.IsMoving = true;
        AnimPlayer.Play(powering_off_animation);
        AnimPlayer.AddToPile(idle_off_animation);
        if (idle_on_animation != "idle") { AnimPlayer.StopPlaying(idle_on_animation); }

        // on attend la fin de l'anim
        while (AnimPlayer.IsShowing(powering_off_animation)) { yield return null; }

        // on eteint l'ordi
        Onnable.IsOn = false;
        Onnable.IsMoving = false;

        if (log) { Debug.Log("(Computer) " + Capable.name + " powered off !!"); }
    }
}