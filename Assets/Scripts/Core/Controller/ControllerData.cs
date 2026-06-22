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
    public string container_id;
    public string last_sofa_id;


    // UpdatePlayerLevelRoomChunk
    public void UpdatePlayerLevelRoomChunk(Loggable<SaveEngine> slog = null)
    {
        if (!ChunkEngine.HasInstance)
        {
            slog?.Error("No ChunkEngine instance, cannot update player level/room/chunk in controller data.");
            return;
        }

        if (string.IsNullOrEmpty(controlled_capable_id))
        {
            slog?.Error("No controlled capable id in controller data, cannot update player level/room/chunk.");
            return;
        }

        Container potential_container = null;
        if (!ChunkEngine.LazyInstance.TryGetCapableChunk(controlled_capable_id, out ChunkData chunk))
        {
            // maybe the capable is in a container, so we try to find the container and get its chunk

            if (Controller.Capable == null) {}
            else if (!Controller.Capable.TryGetCapacity(out SitCapacity sitter)) {}
            else if (sitter.CurrentSofa == null) {}
            else
            {
                Container container = sitter.CurrentSofa;
                if (!ChunkEngine.LazyInstance.TryGetCapableChunk(container.ID, out chunk))
                {
                    slog?.Error($"Could not find chunk for container id '{container.ID}', cannot update player level/room/chunk in controller data.");
                    return;
                }
                potential_container = container;
            }

            if (potential_container == null)
            {
                slog?.Error($"Could not find chunk for capable id '{controlled_capable_id}', cannot update player level/room/chunk in controller data.");
                return;
            }
        }

        player_chunk = chunk.id;
        player_room = chunk.room_id;

        // get level of room
        if (!LevelEngine.LazyInstance.TryGetRoomLevel(player_room, out LevelData level))
        {
            slog?.Warning($"Could not find level for room id '{player_room}', cannot update player level in controller data.");
            return;
        }

        // updates container
        if (potential_container != null) { container_id = potential_container.ID; }
        else { container_id = null; }

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
            container_id = this.container_id,
            last_sofa_id = this.last_sofa_id
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $" - capable template : {capable_template}\n" +
                         $" - controlled capable id : {controlled_capable_id}\n";
        details += $" - stack capable ids : {(stack_capable_ids == null ? "null" : (stack_capable_ids.Count == 0 ? "none" : string.Join(", ", stack_capable_ids)))}\n";
        details += $" - player level : {player_level}\n";
        details += $" - player room : {player_room}\n";
        details += $" - player chunk : {player_chunk}\n";
        details += $" - container id : {container_id}";
        details += $" - last sofa id : {last_sofa_id}";
        return details;
    }
}