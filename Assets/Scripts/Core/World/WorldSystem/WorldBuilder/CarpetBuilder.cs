using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarpetBuilder : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform tilemap_parent;
    [SerializeField] private Tilemap tilemap_prefab;
    [SerializeField] private TileBase carpet_tile;
    private Dictionary<string, Tilemap> tilemap_instances = new Dictionary<string, Tilemap>();

    public Tilemap Build(WorldRoomVisualizer room)
    {
        // we check if we already have a tilemap for this room, else we create one
        if (tilemap_instances.TryGetValue(room.name, out Tilemap tilemap_instance))
        {
            tilemap_instance.ClearAllTiles();
        }
        else
        {
            tilemap_instance = Instantiate(tilemap_prefab, tilemap_parent);
            tilemap_instance.name = $"carpet_{room.name}";
            tilemap_instances[room.name] = tilemap_instance;
        }

        // we calculate all the positions of the tiles we need to create the carpet
        List<Vector2> tile_positions = calculate_tiles_positions(room);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in tile_positions)
        {
            Vector3Int cell_pos = tilemap_instance.WorldToCell(pos);
            tilemap_instance.SetTile(cell_pos, carpet_tile);
        }

        return tilemap_instance;
    }
    
    private List<Vector2> calculate_tiles_positions(WorldRoomVisualizer room)
    {
        List<Vector2> positions = new List<Vector2>();

        // we go through all the links of the room
        foreach (var link in room.Links)
        {
            // we get the positions of the two cells connected by the link
            Vector2 pos_a = link.CellA.WorldPosition;
            Vector2 pos_b = link.CellB.WorldPosition;

            // we calculate the direction from a to b
            Vector2 direction = (pos_b - pos_a).normalized;

            // we calculate the perpendicular direction to the link
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            // we add tiles in a line between the two cells, and a few tiles on the sides to create a carpet effect
            float distance = Vector2.Distance(pos_a, pos_b);
            int tile_count = Mathf.CeilToInt(distance / 0.5f);
            for (int i = 0; i <= tile_count; i++)
            {
                Vector2 tile_position = pos_a + direction * i * 0.5f;
                positions.Add(tile_position);
                positions.Add(tile_position + perpendicular * 0.5f);
                positions.Add(tile_position - perpendicular * 0.5f);
            }
        }
        return positions;
    }
}