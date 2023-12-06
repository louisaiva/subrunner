using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Tilemaps;


public class TilemapVisualiser : MonoBehaviour
{
    private Tilemap floorTilemap;

    [SerializeField]
    private TileBase floorTile;

    [SerializeField]
    private TileBase roomTile;

    [SerializeField]
    private TileBase corridorTile;



    public void Start()
    {
        // on récupère le tilemap
        floorTilemap = transform.Find("visu").GetComponent<Tilemap>();

        // on récupère le tile
        floorTile = Resources.Load<TileBase>("sprites/environments/sector_1/gd_1_0") as TileBase;
        roomTile = Resources.Load<TileBase>("sprites/environments/sector_1/fg_1_2") as TileBase;
        corridorTile = Resources.Load<TileBase>("sprites/environments/sector_1/gd_1_0") as TileBase;

    }

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    public void PaintRoomsTiles(IEnumerable<Vector2Int> roomsPositions)
    {
        PaintTiles(roomsPositions, floorTilemap, roomTile);
    }

    public void PaintCorridorsTiles(IEnumerable<Vector2Int> corridorsPositions)
    {
        PaintTiles(corridorsPositions, floorTilemap, corridorTile);
    }

    // Paint single tile

    public void PaintTiles(IEnumerable<Vector2Int> floorPositions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in floorPositions)
        {
            PaintSingleTile(position, tilemap, tile);
        }
    }

    public void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase tile)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
    }

}