using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderCapacity : Capacity
{

    private List<CircleCollider2D> circles_colliders = new List<CircleCollider2D>();
    private List<BoxCollider2D> box_colliders = new List<BoxCollider2D>();

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        base.LoadData(data);

        if (data is not ColliderCapacityData ccdata) { return; }

        // we load the BOXES
        if (ccdata.boxes != null && ccdata.boxes.Count > 0)
        {
            foreach (ColliderData collider_data in ccdata.boxes)
            {
                Collider2D new_collider = ColliderBank.Instance.LoadCollider(collider_data, this.transform);
                if (new_collider == null) { continue; }
                box_colliders.Add(new_collider as BoxCollider2D);
                if (collider_data.UsedForPathfinding)
                {
                    new_collider.isTrigger = !WorldBuilder.IsWorking;
                }
            }
        }

        // we load the CIRCLES
        if (ccdata.circles != null && ccdata.circles.Count > 0)
        {
            foreach (ColliderData collider_data in ccdata.circles)
            {
                Collider2D new_collider = ColliderBank.Instance.LoadCollider(collider_data, this.transform);
                if (new_collider == null) { continue; }
                circles_colliders.Add(new_collider as CircleCollider2D);
                if (collider_data.UsedForPathfinding)
                {
                    new_collider.isTrigger = !WorldBuilder.IsWorking;
                }
            }
        }
    }
    public override void UnloadData()
    {
        // we unload the alive colliders
        box_colliders.Where(c => c != null).ToList().ForEach(c => ColliderBank.Instance.UnloadCollider(c.gameObject));
        circles_colliders.Where(c => c != null).ToList().ForEach(c => ColliderBank.Instance.UnloadCollider(c.gameObject));
        box_colliders.Clear();
        circles_colliders.Clear();
        base.UnloadData();
    }


    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        ColliderCapacityData static_data = new ColliderCapacityData(base.GetStaticData())
        {
            boxes = new List<BoxData>(),
            circles = new List<CircleData>()
        };

        // we get the colliders
        List<Collider2D> colliders = transform.GetComponentsInChildren<Collider2D>(includeInactive: true).ToList();

        // and get the colliders data
        foreach (Collider2D collider in colliders)
        {
            if (collider is BoxCollider2D box_collider)
            {
                static_data.boxes.Add((BoxData)ColliderBank.GetColliderData(box_collider));
            }
            else if (collider is CircleCollider2D circle_collider)
            {
                static_data.circles.Add((CircleData)ColliderBank.GetColliderData(circle_collider));
            }
        }

        return static_data;
    }
}


[Serializable] public class ColliderCapacityData : CapacityData
{
    public List<BoxData> boxes;
    public List<CircleData> circles;



    // CONSTRUCTOR
    public ColliderCapacityData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new ColliderCapacityData(base.Duplicate() as CapacityData)
        {
            boxes = new List<BoxData>(this.boxes),
            circles = new List<CircleData>(this.circles)
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - boxes : {boxes.Count}\n";
        foreach (BoxData box_data in boxes)
        {
            details += $"     - {box_data.GetDetails()}\n";
        }
        details += $"  - circles : {circles.Count}\n";
        foreach (CircleData circle_data in circles)
        {
            details += $"     - {circle_data.GetDetails()}\n";
        }
        return base.GetDetails() + details;
    }
}