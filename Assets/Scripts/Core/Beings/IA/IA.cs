using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using subrunner.goap;
using UnityEngine;

/// <summary>
/// IA is a class that represents an AI being in the game.
/// it is a Capable of course so it has Capacities & Effects,
/// BUT also have Behaviors which define how it behaves in the game world.
/// </summary>

public class IA : Movable
{
    [Header("IA")]
    // public float exploration_radius = 3f; // the radius of exploration for the IA
                                          // todo must be part of an IAData class or struct that influence a curiosity parameter
    [SerializeField] private string base_tag = "IA";
    public string BaseTag => base_tag;


    [Header("Components")]
    private GoToBehaviour _mover;
    public GoToBehaviour Mover
    {
        get
        {
            if (_mover is null)
            {
                _mover = transform.Find("brain/goto")?.GetComponent<GoToBehaviour>();
                if (_mover == null) { _mover = GetCapacity<MotorCapacity>()?.transform.Find("goto").GetComponent<GoToBehaviour>(); }
            }
            return _mover;
        }
    }
    private Brain _brain;
    public Brain Brain
    {
        get
        {
            if (_brain == null) { _brain = transform.Find("brain")?.GetComponent<Brain>(); }
            return _brain;
        }
    }
    private bool just_loaded = false; // flag to prevent the sensors to sense on the first goal resolve after loading -> so we keep the loaded world state
    private int frame_counter_since_loaded = 0;
    public bool JustLoaded { get { return just_loaded; } }

    [Header("Social Data")]
    public SocialData SocialData;

    [Header("Logs")]
    public bool log_actions = false;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we set the tag
        gameObject.tag = base_tag;
    }

    // LATE UPDATE
    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!just_loaded) { return; }

        // if we just loaded we wait for x frames elapsed to clear the flag
        frame_counter_since_loaded++;
        if (frame_counter_since_loaded < 3) { return; }

        // clear the just loaded flag
        just_loaded = false;
        frame_counter_since_loaded = 0;
    }

    // LOAD DATA / UNLOAD DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        // we set the social data
        if (data is not IAData ia_data) { return; }
        SocialData = ia_data.social_data;

        // we set just loaded to true
        just_loaded = true;
    }
    public override void UnloadData()
    {
        // we reset the social data
        SocialData = new SocialData();

        // and components (so next load will load the new ones)
        _mover = null;
        
        base.UnloadData();
    }

    // GET STATIC DATA
    public override ICapableData GetStaticData()
    {
        IAData static_data = new IAData((CapableData)base.GetStaticData());

        // set the social data
        if (SocialData != null) { static_data.social_data = SocialData.Duplicate(); }

        return static_data;
    }
}

// IA DATA
[Serializable] public class IAData : CapableData
{
    public SocialData social_data = new SocialData();
    
    // CONSTRUCTOR
    public IAData(CapableData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new IAData(base.Duplicate() as CapableData)
        {
            social_data = this.social_data.Duplicate()
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        if (social_data != null) { details += $"  - social_data : {social_data.GetDetails()}"; }
        else { details += $"  - social_data : null\n"; }
        return base.GetDetails() + details;
    }
}

[Serializable] public class SocialData
{
    public float exploration_radius = 3f;
    public float range_detection = 15f;
    public List<string> friendly_skins = new List<string>();
    public List<string> dangerous_skins = new List<string>();
    public List<string> prey_skins = new List<string>();

    public SocialData Duplicate()
    {
        return new SocialData
        {
            exploration_radius = this.exploration_radius,
            range_detection = this.range_detection,
            friendly_skins = new List<string>(friendly_skins),
            dangerous_skins = new List<string>(dangerous_skins),
            prey_skins = new List<string>(prey_skins)
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = "social data : \n";
        details += $"  - exploration_radius : {exploration_radius}\n";
        details += $"  - range_detection : {range_detection}\n";
        details += $"  - friendly_skins : {string.Join(", ", friendly_skins)}\n";
        details += $"  - dangerous_skins : {string.Join(", ", dangerous_skins)}\n";
        details += $"  - prey_skins : {string.Join(", ", prey_skins)}\n";
        return details;
    }
}