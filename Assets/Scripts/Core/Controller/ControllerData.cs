using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class ControllerData
{
    // capable stack
    public string controlled_capable_id;
    public List<string> stack_capable_ids;

    // player data
    public string player_level;
    public string player_room;
    public string player_chunk;

    // last spawn
    public string last_sofa_id;

    // DUPLICATE
    public ControllerData Duplicate()
    {
        return new ControllerData
        {
            controlled_capable_id = this.controlled_capable_id,
            stack_capable_ids = new List<string>(this.stack_capable_ids),
            player_level = this.player_level,
            player_room = this.player_room,
            player_chunk = this.player_chunk,
            last_sofa_id = this.last_sofa_id
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $" - controlled capable id : {controlled_capable_id}\n" +
                         $" - stack capable ids : {string.Join(", ", stack_capable_ids)}\n" +
                         $" - player level : {player_level}\n" +
                         $" - player room : {player_room}\n" +
                         $" - player chunk : {player_chunk}\n" +
                         $" - last sofa id : {last_sofa_id}";
        return details;
    }
}