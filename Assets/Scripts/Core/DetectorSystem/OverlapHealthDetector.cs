using System.Collections.Generic;
using UnityEngine;

public class OverlapHealthDetector : MonoBehaviour, HealthDetector
{
    [Header("Logs")]
    public bool logs_detection = false;

    public T FindClosestCapableOfType<T>(CapableData looker_data) where T : Capable
    {
        throw new System.NotImplementedException();
    }
    public HealthCapacity FindClosestHealthCapacity(IAData iadata)
    {
        // get the potential healths
        List<HealthCapacity> potential_healths = DetectPotentialHealths(iadata.position, iadata.id, iadata.social_data);
        if (potential_healths.Count == 0) { return null; }

        // we find the closest health
        HealthCapacity closest_health = null;
        float closest_distance = float.MaxValue;
        foreach (HealthCapacity health in potential_healths)
        {
            float distance = Vector3.Distance(health.gameObject.transform.position, iadata.position);

            if (!(distance < closest_distance))
                continue;

            closest_health = health;
            closest_distance = distance;
        }
        return closest_health;
    }


    // OLD OVERLAPPING DETECTION
    public List<HealthCapacity> DetectPotentialHealths(Vector2 position, string ia_id, SocialData sdata)
    {
        // we do an overlap to detect healths
        Collider2D[] results = Physics2D.OverlapCircleAll(position,
            sdata.range_detection,
            LayerMask.GetMask("Beings"));
        if (results.Length == 0) { return new List<HealthCapacity>(); }

        string log = "";

        // we convert those into healths & check few things
        List<HealthCapacity> potential_healths = new List<HealthCapacity>();
        foreach (Collider2D collider in results)
        {
            // we check if we find the parent capable
            Capable capable = collider.transform.parent.GetComponent<Capable>();
            if (capable == null && collider.transform.parent.parent != null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
            if (capable == null) { continue; }

            // we check if the capable has a health capacity
            if (!capable.TryGetCapacity(out HealthCapacity health)) { continue; }

            // and that it's alive
            if (!health.Alive) { continue; }

            // exclude friendly healths
            string skin = capable.Skin;
            if (sdata.friendly_skins.Contains(skin)) { continue; }
            if (!sdata.prey_skins.Contains(skin)) { continue; }

            // we add the health to the list of potential healths
            potential_healths.Add(health);
            if (logs_detection)
            {
                log += $"  - {health.data.id} at position {health.transform.position} with distance {Vector3.Distance(health.transform.position, position)} \n";
            }
        }

        if (logs_detection) { Debug.Log("(OverlapHealthDetector) " + ia_id + " detected " + potential_healths.Count + " potential healths in range of " + sdata.range_detection + " : \n" + log); }

        return potential_healths;
    }
}