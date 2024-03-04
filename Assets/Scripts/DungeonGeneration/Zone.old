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
        
        // PlaceTilemaps(area);

        // on place la zone dans l'Area
        foreach (Transform child in objects_parent)
        {
            // if (!isInsideRoom(child.position, area)) {continue;}

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
        

        // on place le sol
        for (int x = tm_bounds.xMin; x < tm_bounds.xMax; x++)
        {
            bool destroyed_bg_tile = false;
            bool destroyed_fg_tile = false;

            for (int y = tm_bounds.yMin; y < tm_bounds.yMax; y++)
            {
                TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));

                // on place le tile dans le monde
                if (tile != null)
                {
                    Vector2Int tile_position = new Vector2Int(tm_origin.x + x, tm_origin.y + y);
                    world.PlaceTile(tile, tile_position, "gd");

                    // on regarde si on a une tile sur bg
                    if (world.GetTile(tile_position.x,tile_position.y, "bg") != null)
                    {
                        world.PlaceTile(null, tile_position, "bg");

                        // on détruit les tiles de fg qui sont au dessus
                        Vector2Int tile_position_fg = tile_position + new Vector2Int(0, 4);
                        world.PlaceTile(null, tile_position_fg, "fg");
                        tile_position_fg = tile_position + new Vector2Int(0, 5);
                        world.PlaceTile(null, tile_position_fg, "fg");

                        // on vérifie si c'est bien une tile de mur externe (pas de tile de sol en y+1) ou de mur interne (tile de sol en y+1)
                        if (world.GetTile(tile_position.x,tile_position.y+1, "gd") == null)
                        {
                            destroyed_bg_tile = true;
                        }
                    }
                    if (world.GetTile(tile_position.x,tile_position.y, "fg") != null)
                    {
                        world.PlaceTile(null, tile_position, "fg");

                        if (world.GetTile(tile_position.x,tile_position.y-3, "fg") != null)
                        {
                            destroyed_fg_tile = true;
                        }

                        // on détruit les tiles de bg qui sont en dessous
                        Vector2Int tile_position_bg = tile_position + new Vector2Int(0, -4);
                        world.PlaceTile(null, tile_position_bg, "bg");

                    }
                }
                else
                {
                    if (x == tm_bounds.xMin)
                    {
                        // on met une tile de sol en x-1
                        Vector2Int tile_position = new Vector2Int(tm_origin.x + x-1, tm_origin.y + y);
                        world.SetLayerTile(tile_position.x,tile_position.y,1, "gd");

                        if (y == tm_bounds.yMin)
                        {
                            // on met une tile de sol en x-1 et y-1
                            world.SetLayerTile(tile_position.x,tile_position.y+1,1, "gd");
                        }
                        else if (y == tm_bounds.yMax-1)
                        {
                            // on met une tile de sol en x-1 et y+1
                            world.SetLayerTile(tile_position.x,tile_position.y-1,1, "gd");
                        }
                    }
                    else if (x == tm_bounds.xMax-1)
                    {
                        Vector2Int tile_position = new Vector2Int(tm_origin.x + x+1, tm_origin.y + y);
                        world.SetLayerTile(tile_position.x,tile_position.y,1, "gd");
                        
                        if (y == tm_bounds.yMin)
                        {
                            // on met une tile de sol en x+1 et y-1
                            world.SetLayerTile(tile_position.x,tile_position.y+1,1, "gd");
                        }
                        else if (y == tm_bounds.yMax-1)
                        {
                            // on met une tile de sol en x+1 et y+1
                            world.SetLayerTile(tile_position.x,tile_position.y-1,1, "gd");
                        }
                    }
                    world.SetLayerTile(tm_origin.x + x,tm_origin.y + y,1, "gd");
                }
            }

            if (destroyed_bg_tile)
            {
                // on place une tile sur bg en yMax
                Vector2Int tile_position = new Vector2Int(tm_origin.x + x, tm_origin.y + tm_bounds.yMax);
                world.SetLayerTile(tile_position.x,tile_position.y,1, "bg");

                // on met une tile de gd
                world.SetLayerTile(tile_position.x,tile_position.y,1, "gd");

                // on détruit les tiles de fg qui sont au dessus et on en met une à y+4
                for (int y = tm_bounds.yMax; y < tm_bounds.yMax + 4; y++)
                {
                    Vector2Int tile_position_fg = new Vector2Int(tm_origin.x + x, tm_origin.y + y);
                    world.PlaceTile(null, tile_position_fg, "fg");
                }
                world.SetLayerTile(tile_position.x,tile_position.y+4,1, "fg");
            }

            if (destroyed_fg_tile)
            {
                // on place une tile sur fg en yMin
                Vector2Int tile_position = new Vector2Int(tm_origin.x + x, tm_origin.y + tm_bounds.yMin-1);
                world.SetLayerTile(tile_position.x,tile_position.y,1, "fg");

                // on met une tile de gd
                world.SetLayerTile(tile_position.x,tile_position.y,1, "gd");
            }
        }
    }

    /* private bool isInsideRoom(Vector3 position, Area area)
    {
        return area.getBounds().Contains(position);
    } */

}