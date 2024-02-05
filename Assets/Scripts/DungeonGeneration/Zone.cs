using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class Zone : MonoBehaviour
{

    // une Zone applique des règles de génération particulière à des Area.
    // place des items, etc.
    // s'applique à la génération du monde
    // peut modifier les tilemaps

    protected World world;
    protected Transform objects_parent;
    protected Transform tm_parent;

    void Start()
    {
        // on récupère le monde
        world = GameObject.Find("/world").GetComponent<World>();

        // on récupère le parent des objets
        objects_parent = transform.Find("obj");
        tm_parent = transform.Find("tm");
    }

    public void PlaceZone(Area area)
    {
        if (world == null) { Start(); }
        
        PlaceTilemaps(area);

        // on place la zone dans l'Area
        foreach (Transform child in objects_parent)
        {
            area.PlaceZoneObject(child.gameObject,child.position);
        }
    }

    public void PlaceTilemaps(Area area)
    {
        if (tm_parent == null) { Start(); }
        if (tm_parent == null) {return;}

        // on récup les bounds de l'area et du tilemap
        BoundsInt area_bounds = area.getBounds();
        Tilemap tm = tm_parent.Find("gd").GetComponent<Tilemap>();
        tm.CompressBounds();
        BoundsInt tm_bounds = tm.cellBounds;
        Vector2Int tm_origin = area.getZoneOrigin();

        // on les compare
        if (tm_bounds.size.x > area_bounds.size.x || tm_bounds.size.y > area_bounds.size.y)
        {
            Debug.LogWarning("(Zone) Tilemap is bigger than the area. It will be cropped.");
        }

        // on parcourt les tiles du tilemap
        for (int x = tm_bounds.xMin; x < tm_bounds.xMax; x++)
        {
            for (int y = tm_bounds.yMin; y < tm_bounds.yMax; y++)
            {
                TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));

                // on place le tile dans le monde
                if (tile != null)
                {
                    Vector2Int tile_position = new Vector2Int(tm_origin.x + x, tm_origin.y + y);
                    world.PlaceTile(null, tile_position, "fg");
                    world.PlaceTile(null, tile_position, "bg");
                    world.PlaceTile(tile, tile_position, "gd");
                }
            }
        }
    }

}