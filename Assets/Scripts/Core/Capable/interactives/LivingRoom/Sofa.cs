using System.Collections.Generic;
using UnityEngine;

public class Sofa : Container, Interactable, Sittable
{

    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.LivingRoom;


    // standing & sitting positions
    [SerializeField] private Vector2 local_sitting_position;
    [SerializeField] private Vector2 local_standing_position;
    public Vector2 WorldSittingPosition { get { return transform.TransformPoint(local_sitting_position); } }
    public Vector2 WorldStandingPosition { get { return transform.TransformPoint(local_standing_position); } }






    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        // check if the interactor is the controlled one
        if (interactor != Controller.Capable) { return; }
        if (!interactor.TryGetCapacity(out SitCapacity sit_capacity)) { return; }


        // we check if we are already sitting on this sofa
        if (sit_capacity.CurrentSofa == this)
        {
            // if we are, we exit the sofa
            sit_capacity.ExitSofa();
            return;
        }

        // we sit on the sofa
        sit_capacity.Sit(this);
    }
    public override void LoadCapablesAccordingly(Capable capable)
    {
        // we sit on the sofa
        capable.GetCapacity<SitCapacity>()?.Sit(this, instant: true);
    }






    // SIBLING TV
    private TV sibling_tv = null;
    public TV SiblingTV
    {
        get
        {
            if (sibling_tv != null) { return sibling_tv; }

            // we do a circle cast to find the closest TV
            sibling_tv = find_sibling_tv();
            return sibling_tv;
        }
    }
    private TV find_sibling_tv()
    {
        float radius = 5f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Interactives"));
        List<TV> tvs = new List<TV>();
        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponentInParent<TV>(includeInactive: true) is not TV tv) { continue; }
            tvs.Add(tv);
        }
        if (tvs.Count == 0) { return null; }

        // return closest one
        TV closest_tv = null;
        float closest_distance = float.MaxValue;
        foreach (TV tv in tvs)
        {
            float distance = Vector2.Distance(transform.position, tv.transform.position);
            if (distance > closest_distance) { continue; }
            closest_distance = distance;
            closest_tv = tv;
        }
        return closest_tv;
    }








    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        if (data is not SofaData sofa_data) { Debug.LogError($"(Sofa) {name} cannot load data because it's not a SofaData"); base.LoadData(data); return; }

        // ! WE WANT TO SET THE LOCAL POSITIONS BEFORE calling base.LoadData()
        // because load data calls load_capable_accordingly which call SitCapacity and so
        // we need to have our position set :D
        local_sitting_position = sofa_data.local_sitting_position;
        local_standing_position = sofa_data.local_standing_position;

        base.LoadData(data);
    }
    public override ICapableData GetStaticData()
    {
        SofaData static_data = new SofaData((CapableData)base.GetStaticData())
        {
            local_sitting_position = this.local_sitting_position,
            local_standing_position = this.local_standing_position
        };

        return static_data;
    }
}

public class SofaData : ContainerData
{
    public Vector2 local_sitting_position;
    public Vector2 local_standing_position;
    // CONSTRUCTOR
    public SofaData() : base() { }
    public SofaData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new SofaData(base.Duplicate() as CapableData)
        {
            local_sitting_position = this.local_sitting_position,
            local_standing_position = this.local_standing_position
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - local_sitting_position: {local_sitting_position}\n";
        details += $"  - local_standing_position: {local_standing_position}\n";
        return details;
    }
}