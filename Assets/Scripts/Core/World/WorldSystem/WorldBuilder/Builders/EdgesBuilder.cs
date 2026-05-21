using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EdgesBuilder : TilemapBuilder
{
    [Header("Edges Tiles")]
    [SerializeField] private TileBase L_edge;
    [SerializeField] private TileBase R_edge;

    // MAIN TILEMAP GENERATION
    protected override void GenerateTilemap(Tilemap tilemap, WorldChunkVisualizer room)
    {
        // we get the edges from the WallsBuilder
        WallsBuilder walls_builder = GetComponent<WallsBuilder>();
        if (walls_builder == null) { Debug.LogError("(EdgesBuilder) No WallsBuilder found on " + name); return; }
        walls_builder.GetEdges(out List<Vector3Int> L_edges_positions, out List<Vector3Int> R_edges_positions);

        // we convert those positions to tilemap's grid positions and we set the tiles
        foreach (var pos in L_edges_positions) { tilemap.SetTile(pos, L_edge); }
        foreach (var pos in R_edges_positions) { tilemap.SetTile(pos, R_edge); }
    }
}