using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class HandMadeSector : Sector
{
    [Header("Hand Made Sector")]
    // [SerializeField] private Vector2 tile_start_pos;
    [SerializeField] private Tilemap fg_tm;
    [SerializeField] private Tilemap bg_tm;
    [SerializeField] private Tilemap gd_tm; 
    [SerializeField] private Vector2Int area_start; // position de la première handmade area (en bas à gauche)
    [SerializeField] private Vector2Int wh_handmade; // taille de la zone handmade (en area)


    [Header("spawn position")]
    [SerializeField] private bool is_spawn_sector = false;
    [SerializeField] private Vector2 spawn_pos;

    // unity functions
    new void Awake()
    {
        base.Awake();

        // on récupère les tilemaps
        bg_tm = transform.Find("bg_sector").gameObject.GetComponent<Tilemap>();
        fg_tm = transform.Find("fg_sector").gameObject.GetComponent<Tilemap>();
        gd_tm = transform.Find("gd_sector").gameObject.GetComponent<Tilemap>();
    }


    // init functions
    public void initHashets()
    {
        // on remplit les hashsets en fonction des tilemaps et d'area_size
        bg_tm.CompressBounds();
        fg_tm.CompressBounds();
        gd_tm.CompressBounds();

        // on récupère le point max de chaque tilemap
        float bg_max_x = bg_tm.localBounds.max.x;
        float bg_max_y = bg_tm.localBounds.max.y;
        float fg_max_x = fg_tm.localBounds.max.x;
        float fg_max_y = fg_tm.localBounds.max.y;
        float gd_max_x = gd_tm.localBounds.max.x;
        float gd_max_y = gd_tm.localBounds.max.y;

        // et donc le point max du secteur
        float max_x = Mathf.Max(bg_max_x, fg_max_x, gd_max_x);
        float max_y = Mathf.Max(bg_max_y, fg_max_y, gd_max_y);

        // print("(HandMadeSector) max_x : " + max_x + " max_y : " + max_y);

        // on multiplie par 2 pour avoir la taille du secteur en tiles (chaque tile fait 0.5 unité)
        float w_tiles = max_x*2;
        float h_tiles = max_y*2;

        // print("(HandMadeSector) w_tiles : " + w_tiles + " h_tiles : " + h_tiles);

        // on divise par area_size pour avoir le nombre d'area
        int w_area = Mathf.CeilToInt(w_tiles / area_size.x);
        int h_area = Mathf.CeilToInt(h_tiles / area_size.y);
        wh_handmade = new Vector2Int(w_area, h_area);

        // print("(HandMadeSector) w_area : " + w_area + " h_area : " + h_area + " / w_area : " + w_tiles / area_size.x + " h_area : " + h_tiles / area_size.y);

        // on ajoute les tiles dans le hashset en partant de 1,1
        // -> comme ça on a un hashet avec un contour de 1 tile dans toutes les directions
        for (int i = 1; i < w_area+1; i++)
        {
            for (int j = 1; j < h_area+1; j++)
            {
                rooms.Add(new Vector2Int(i, j));
            }
        }


        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on calcule la position SW du secteur
        x = 0;
        y = 0;

        // on calcule la taille du secteur avec le contour de 1 tile
        w = w_area +2;
        h = h_area +2;

        // on calcule la position de la première handmade area
        area_start = new Vector2Int(1, 1);

    }

    public void LAUNCH()
    {
        // we delete our tilemaps
        Destroy(bg_tm.gameObject);
        Destroy(fg_tm.gameObject);
        Destroy(gd_tm.gameObject);

        // we move the sector to the right position
        transform.position = new Vector3(((x +area_start.x)* area_size.x)/2, ((y +area_start.y)* area_size.y)/2, 0);
    }

    // SECTOR GETTERS
    public override string getAreaType(Vector2Int pos)
    {
        if (pos.x >= area_start.x && pos.x < area_start.x + wh_handmade.x && pos.y >= area_start.y && pos.y < area_start.y + wh_handmade.y)
        {
            return "handmade";
        }
        return base.getAreaType(pos);
    }

    // GETTERS
    public List<Tilemap> GetTilemaps()
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        tilemaps.Add(fg_tm);
        tilemaps.Add(bg_tm);
        tilemaps.Add(gd_tm);
        return tilemaps;
    }
    
    public TileBase[] getTiles(string tm = "bg")
    {
        BoundsInt bounds = new BoundsInt(0,0, 0, w * area_size.x, h * area_size.y, 1);
        // on récupère les tiles
        if (tm == "bg")
        {
            return bg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "fg")
        {
            return fg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "gd")
        {
            return gd_tm.GetTilesBlock(bounds);
        }

        return null;
    }
    public TileBase[] getHandmadeTiles(string tm = "bg")
    {
        BoundsInt bounds = new BoundsInt(0, 0, 0, wh_handmade.x * area_size.x, wh_handmade.y * area_size.y, 1);
        // on récupère les tiles
        if (tm == "bg")
        {
            return bg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "fg")
        {
            return fg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "gd")
        {
            return gd_tm.GetTilesBlock(bounds);
        }

        return null;
    }

    public bool isSpawnSector()
    {
        return is_spawn_sector;
    }

    public Vector2Int getAreaStart()
    {
        return area_start;
    }

    public Vector2 getSpawnPos()
    {
        return new Vector2(spawn_pos.x + ((x + area_start.x) * area_size.x)/2f, spawn_pos.y + ((y + area_start.y) * area_size.y)/2f);
    }

    public BoundsInt getHandmadeBounds()
    {
        return new BoundsInt((x + area_start.x) * area_size.x, (y + area_start.y) * area_size.y, 0, wh_handmade.x * area_size.x, wh_handmade.y * area_size.y, 1);
    }

    
}