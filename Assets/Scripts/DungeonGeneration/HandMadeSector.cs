using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class HandMadeSector : Sector
{
    [Header("Hand Made Sector")]
    [SerializeField] private Vector2 tile_start_pos;
    [SerializeField] private Tilemap fg_tm;
    [SerializeField] private Tilemap bg_tm;
    [SerializeField] private Tilemap gd_tm; 

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
        float w_tiles = (max_x - tile_start_pos.x)*2;
        float h_tiles = (max_y - tile_start_pos.y)*2;

        // print("(HandMadeSector) w_tiles : " + w_tiles + " h_tiles : " + h_tiles);

        // on divise par area_size pour avoir le nombre d'area
        int w_area = Mathf.CeilToInt(w_tiles / area_size.x);
        int h_area = Mathf.CeilToInt(h_tiles / area_size.y);

        // print("(HandMadeSector) w_area : " + w_area + " h_area : " + h_area + " / w_area : " + w_tiles / area_size.x + " h_area : " + h_tiles / area_size.y);

        // on ajoute les tiles dans le hashset
        for (int i = 0; i < w_area; i++)
        {
            for (int j = 0; j < h_area; j++)
            {
                rooms.Add(new Vector2Int(i, j));
            }
        }

        base.init();


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

    public Vector2 GetStartTile()
    {
        return tile_start_pos;
    }

    public TileBase[] getTiles(string tm = "bg")
    {
        BoundsInt bounds = new BoundsInt((int) tile_start_pos.x*2, (int) tile_start_pos.y*2, 0, w * area_size.x, h * area_size.y, 1);
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

}