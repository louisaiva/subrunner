
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// CloseCapacity is a capacity that allows to close itself. can be used for chests or doors for example.
/// </summary>

// todo : maybe re-unify the both open & close capacity since it's a one only system it would make sense..
// todo : and we should stop using Do("") and instead call GetCapacity<>().Open() / .Close() it would be better ahah
public class CloseCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            if ((Capable as Openable).is_moving) { return false; }
            if (!(Capable as Openable).is_open) { return false; }
            return true;
        }
    }

    [Header("Close parameters")]
    public float closing_duration = 0.5f;
    public string close_anim = "close";
    public string hover_close_anim = "hover";
    public string idle_open_anim = "idle_open";


    private OpenCapacity _open_capacity;
    private OpenCapacity open_capacity
    {
        get
        {
            if (data == null) { return null; }
            if (_open_capacity == null) { _open_capacity = GetSiblingCapacity<OpenCapacity>(); }
            return _open_capacity;
        }
    }


    // CLOSING
    public virtual void Close()
    {
        // on supprime les invokes de l'ouverture si il y en a
        open_capacity?.CancelOpenInvoke();
        (Capable as Openable).is_moving = true;

        // on joue l'animation
        Capable.AnimPlayer.StopPlaying(idle_open_anim); // ? really useful here ? since we do it again in success_close, check the door's animplayer acp list to check if useful
        Capable.AnimPlayer.Play(close_anim, duration_override: closing_duration);

        // on joue le son
        AudioEngine.Instance.Play("close", Capable.Skin, Capable.gameObject);
        Invoke("success_close", closing_duration);

        if (log) { Debug.Log(Capable.name + " is closing..."); }
    }
    protected virtual void success_close()
    {
        if (Capable == null)
        {
            Debug.LogError("(OpenCapacity) Can't close because our Capable is null");
            return;
        }
        if (Capable is not Openable openable)
        {
            Debug.LogError("(OpenCapacity) Can't close because our Capable is not openable : " + Capable.ID + $" (loaded ? {Capable.Loaded})");
            return;
        }
        openable.is_open = false;
        openable.is_moving = false;

        // on joue l'animation
        GetSiblingCapacity<HoverCapacity>()?.ChangeAnimation(hover_close_anim);
        Capable.AnimPlayer.Play("idle");
        Capable.AnimPlayer.StopPlaying(idle_open_anim);

        // on fait les vérifications pour les portes
        if (Capable is Door door) { door.UpdateIF(door.Orientation); }

        if (log) { Debug.Log(Capable.ID + " is now closed !"); }
    }
    public virtual void CloseInstantly()
    {
        // on supprime tous les invokes si on en a
        open_capacity?.CancelOpenInvoke();
        CancelCloseInvoke();

        // on ferme direct
        success_close();
    }

    // CancelInvoke
    public void CancelCloseInvoke()
    {
        if (log) { Debug.Log("(CloseCapacity) " + Capable.name + " CancelInvoke success_close"); }
        CancelInvoke("success_close");
    }



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);


        if (data is not CloseCapacityData close_data) { return; }
        this.closing_duration = close_data.closing_duration;
        this.close_anim = close_data.close_anim;
        this.hover_close_anim = close_data.hover_close_anim;
        this.idle_open_anim = close_data.idle_open_anim;
    }
    public override void UnloadData()
    {
        _open_capacity = null;
        base.UnloadData();
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        return new CloseCapacityData(base.GetStaticData())
        {
            closing_duration = this.closing_duration,
            close_anim = this.close_anim,
            hover_close_anim = this.hover_close_anim,
            idle_open_anim = this.idle_open_anim
        };
    }
}

public class CloseCapacityData : CapacityData
{
    public float closing_duration = 0.5f;
    public string close_anim = "close";
    public string hover_close_anim = "hover";
    public string idle_open_anim = "idle_open";


    // CONSTRUCTOR
    public CloseCapacityData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new CloseCapacityData(base.Duplicate() as CapacityData)
        {
            closing_duration = this.closing_duration,
            close_anim = this.close_anim,
            hover_close_anim = this.hover_close_anim,
            idle_open_anim = this.idle_open_anim
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - closing duration: {closing_duration}\n";
        details += $"  - close anim: {close_anim}\n";
        details += $"  - hover close anim: {hover_close_anim}\n";
        details += $"  - idle open anim: {idle_open_anim}\n";
        return base.GetDetails() + details;
    }

}