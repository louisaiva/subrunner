using UnityEngine;

/// <summary>
/// OpenCapacity is a capacity that allows to open itself. can be used for chests or doors for example.
/// </summary>

public class OpenCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            if (Capable is not Openable openable)
            {
                Debug.LogWarning("Capable " + Capable.ID + " is not Openable !");
                return false;
            }

            if (openable.is_moving) { return false; }
            if (openable.is_open) { return false; }
            return true;
        }
    }

    [Header("Open parameters")]
    public float opening_duration = 0.5f;
    public string open_anim = "open";
    public string hover_open_anim = "hover";
    public string idle_open_anim = "idle_open";

    private CloseCapacity _close_capacity;
    private CloseCapacity close_capacity
    {
        get
        {
            if (data == null) { return null; }
            if (_close_capacity == null) { _close_capacity = GetSiblingCapacity<CloseCapacity>(); }
            return _close_capacity;
        }
    }

    // OPENING
    public virtual void Open(bool play_anim_and_sound = true)
    {
        // on supprime les invokes de l'ouverture si il y en a
        close_capacity?.CancelCloseInvoke();

        // on ouvre le coffre
        (Capable as Openable).is_moving = true;

        // on joue l'animation
        if (play_anim_and_sound)
        {
            Capable.AnimPlayer.Play(open_anim, duration_override: opening_duration);

            // on joue le son
            AudioEngine.Instance.Play("open", Capable.Skin, Capable.gameObject);
        }
        
        // on joue l'animation
        Invoke("success_open", opening_duration);

        if (log) { Debug.Log(Capable.name + " is opening..."); }
    }
    protected virtual void success_open()
    {
        // on ouvre le coffre
        if (Capable == null)
        {
            Debug.LogError("(OpenCapacity) Can't open because our Capable is null");
            return;
        }
        if (Capable is not Openable openable)
        {
            Debug.LogError("(OpenCapacity) Can't open because our Capable is not openable : " + Capable.name + $" (loaded ? {Capable.Loaded})");
            return;
        }
        openable.is_open = true;
        openable.is_moving = false;

        // on joue l'animation
        Capable.AnimPlayer.AddToPile(idle_open_anim);
        GetSiblingCapacity<HoverCapacity>()?.ChangeAnimation(hover_open_anim);

        if (log) { Debug.Log(Capable.ID + " is open !"); }
    }

    public virtual void OpenInstantly()
    {
        // on supprime tous les invokes si on en a
        close_capacity?.CancelCloseInvoke();
        CancelOpenInvoke();

        // on ouvre direct
        success_open();
    }

    // CancelInvoke
    public void CancelOpenInvoke()
    {
        if (log) { Debug.Log("(OpenCapacity) " + Capable.name + " CancelInvoke success_open"); }
        CancelInvoke("success_open");
    }










    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not OpenCapacityData open_data) { return; }
        this.opening_duration = open_data.opening_duration;
        this.open_anim = open_data.open_anim;
        this.hover_open_anim = open_data.hover_open_anim;
        this.idle_open_anim = open_data.idle_open_anim;
    }
    public override void UnloadData()
    {
        base.UnloadData();
        _close_capacity = null;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        return new OpenCapacityData(base.GetStaticData())
        {
            opening_duration = this.opening_duration,
            open_anim = this.open_anim,
            hover_open_anim = this.hover_open_anim,
            idle_open_anim = this.idle_open_anim
        };
    }
}

public class OpenCapacityData : CapacityData
{
    public float opening_duration = 0.5f;
    public string open_anim = "open";
    public string hover_open_anim = "hover";
    public string idle_open_anim = "idle_open";


    // CONSTRUCTOR
    public OpenCapacityData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new OpenCapacityData(base.Duplicate() as CapacityData)
        {
            opening_duration = this.opening_duration,
            open_anim = this.open_anim,
            hover_open_anim = this.hover_open_anim,
            idle_open_anim = this.idle_open_anim
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - opening duration: {opening_duration}\n";
        details += $"  - open anim: {open_anim}\n";
        details += $"  - hover open anim: {hover_open_anim}\n";
        details += $"  - idle open anim: {idle_open_anim}\n";
        return base.GetDetails() + details;
    }

}