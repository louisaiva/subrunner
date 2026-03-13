using System.Collections;
using UnityEngine;

public class OnOffCapacity : Capacity
{

    // public float powering_on_duration = 0.5f;
    // public float powering_off_duration = 0.5f;

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
        Capable.AnimPlayer.Play(powering_on_animation);
        Capable.AnimPlayer.AddToPile(idle_on_animation);
        if (idle_off_animation != "idle") { Capable.AnimPlayer.StopPlaying(idle_off_animation); }

        // on attend la fin de l'anim
        while (Capable.AnimPlayer.current_capacity == powering_on_animation) { yield return null; }

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
        Capable.AnimPlayer.Play(powering_off_animation);
        Capable.AnimPlayer.AddToPile(idle_off_animation);
        if (idle_on_animation != "idle") { Capable.AnimPlayer.StopPlaying(idle_on_animation); }

        // on attend la fin de l'anim
        while (Capable.AnimPlayer.current_capacity == powering_off_animation) { yield return null; }

        // on eteint l'ordi
        Onnable.IsOn = false;
        Onnable.IsMoving = false;

        if (log) { Debug.Log("(Computer) " + Capable.name + " powered off !!"); }
    }

    /* protected virtual void open()
    {
        // on supprime les invokes de l'ouverture si il y en a
        close_capacity?.CancelCloseInvoke();

        // on ouvre le coffre
        (capable as Openable).Onnable.IsMoving = true;

        // on joue l'animation
        capable.anim_player.Play("open", duration_override: opening_duration);
        Invoke("success_open", opening_duration);

        // on fait les vérifications pour les portes
        if (capable is Door)
        {
            // on reset le layer à fg & order in layer à 1
            capable.AnimPlayer.Renderer.sortingLayerName = "fg";
            capable.AnimPlayer.Renderer.sortingOrder = 1;
        }

        if (debug) { Debug.Log(capable.name + " is opening..."); }
    }
    protected virtual void success_open()
    {
        // on ouvre le coffre
        (capable as Openable).is_open = true;
        (capable as Openable).Onnable.IsMoving = false;

        // on joue l'animation
        capable.anim_player.AddToPile("idle_open");

        // on fait les vérifications pour les portes
        if (capable is Door)
        {
            // on reset le layer à main & order in layer a -1
            capable.AnimPlayer.Renderer.sortingLayerName = "main";
            capable.AnimPlayer.Renderer.sortingOrder = -1;
        }

        if (debug) { Debug.Log(capable.name + " is open !"); }
    } */
}