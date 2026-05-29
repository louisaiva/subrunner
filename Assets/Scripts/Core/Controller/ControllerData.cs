using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class ControllerData
{
    // capable stack
    public string capable_template;
    public string controlled_capable_id;
    public List<string> stack_capable_ids;

    // player data
    public string player_level;
    public string player_room;
    public string player_chunk;

    // last spawn
    public string last_sofa_id;

    // UpdatePlayerLevelRoomChunk
    public void UpdatePlayerLevelRoomChunk(Loggable<SaveEngine> slog = null)
    {
        if (!ChunkEngine.HasInstance)
        {
            slog?.Warning("No ChunkEngine instance, cannot update player level/room/chunk in controller data.");
            return;
        }

        if (string.IsNullOrEmpty(controlled_capable_id))
        {
            slog?.Warning("No controlled capable id in controller data, cannot update player level/room/chunk.");
            return;
        }

        if (!ChunkEngine.LazyInstance.TryGetCapableChunk(controlled_capable_id, out ChunkData chunk))
        {
            slog?.Warning($"Could not find chunk for capable id '{controlled_capable_id}', cannot update player level/room/chunk in controller data.");
            return;
        }

        player_chunk = chunk.id;
        player_room = chunk.room_id;

        // get level of room
        if (!LevelEngine.LazyInstance.TryGetRoomLevel(player_room, out LevelData level))
        {
            slog?.Warning($"Could not find level for room id '{player_room}', cannot update player level in controller data.");
            return;
        }

        player_level = level.id;
        slog?.Log($"Updated player level/room/chunk in controller data : level '{player_level}', room '{player_room}', chunk '{player_chunk}'.");
    }

    // DUPLICATE
    public ControllerData Duplicate()
    {
        return new ControllerData
        {
            capable_template = this.capable_template,
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
        string details = $" - capable template : {capable_template}\n" +
                         $" - controlled capable id : {controlled_capable_id}\n" +
                         $" - stack capable ids : {string.Join(", ", stack_capable_ids)}\n" +
                         $" - player level : {player_level}\n" +
                         $" - player room : {player_room}\n" +
                         $" - player chunk : {player_chunk}\n" +
                         $" - last sofa id : {last_sofa_id}";
        return details;
    }
}