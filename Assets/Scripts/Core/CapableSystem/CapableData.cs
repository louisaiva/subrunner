using System.Collections.Generic;
using UnityEngine;

public class CapableData
{
    public string id;
    public Vector2 position;
    public Vector2 orientation;
    // public Vector2 inputs; ????

    // ANIM PLAYER
    // todo maybe we can have a AnimPlayerData class ?
    // - skin
    // - layers
    // - List<AnimCapacityPriority>
    public string skin;

    // INVENTORY
    public List<string> capable;

    // CAPACITIES
    public List<string> capacities_ids;

    // EFFECTS
    public List<Effect> effects;
    public List<float> effects_ttl; // time to live for each effect, in seconds
}