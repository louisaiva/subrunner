using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Newtonsoft.Json;

public class World : MonoBehaviour
{

    [Header("Builder")]
    [SerializeField] private TileAreaBuilder builder;

    [Header("World")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    
    [Header("Tilemaps")]
    [SerializeField] private Tilemap fg_tm;
    [SerializeField] private Tilemap bg_tm;
    [SerializeField] private Tilemap gd_tm;

    [Header("Tiles")]
    [SerializeField] private TileBase fg_tile;
    [SerializeField] private TileBase bg_tile;
    [SerializeField] private TileBase gd_tile;

    [Header("Areas")]
    private Vector2Int area_size = new Vector2Int(16, 16);


    void Awake()
    {
        // on récupère les tilemaps
        fg_tm = transform.Find("fg").GetComponent<Tilemap>();
        bg_tm = transform.Find("bg").GetComponent<Tilemap>();
        gd_tm = transform.Find("gd").GetComponent<Tilemap>();

        // on récupère les tiles
        fg_tile = Resources.Load<TileBase>("tilesets/fg_2_rule");
        bg_tile = Resources.Load<TileBase>("tilesets/walls_1_rule");
        gd_tile = Resources.Load<TileBase>("tilesets/gd_2_rule");

        // on récupère le builder
        builder = GameObject.Find("generator").transform.Find("builder").GetComponent<TileAreaBuilder>();

        // on génère le monde
        // Generate();
    }

    // GENERATION
    public void Generate()
    {
        // on choisit un fichier json d'area aléatoire
        string area_json = builder.GetAreaJson("corridor_LR");

        // on set les tiles de l'area
        SetAreaTiles(0, 0, area_json);
    }

    public void PlaceArea(int x, int y, string area_name)
    {
        // on choisit un fichier json d'area aléatoire
        string area_json = builder.GetAreaJson(area_name);

        // on set les tiles de l'area
        SetAreaTiles(x, y, area_json);
    }

    // SETTERS
    public void SetLayerTile(int x, int y, int tile=0, string layer="bg")
    {
        if (layer == "bg")
        {
            bg_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : bg_tile);
        }
        else if (layer == "fg")
        {
            fg_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : fg_tile);
        }
        else if (layer == "gd")
        {
            gd_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : gd_tile);
        }
    }

    public void SetTile(int x, int y, int fg, int bg, int gd)
    {
        SetLayerTile(x, y, fg, "fg");
        SetLayerTile(x, y, bg, "bg");
        SetLayerTile(x, y, gd, "gd");
    }

    public void SetAreaTiles(int x, int y, string json)
    {
        // loads the json to the tilemaps

        // on parse le json
        Dictionary<string, List<List<int>>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);

        // on récupère les listes
        List<List<int>> fg = dict["fg"];
        List<List<int>> bg = dict["bg"];
        List<List<int>> gd = dict["gd"];

        print(fg.Count);

        // on ajoute des tiles aux tilemaps
        for (int j = 0; j < area_size.y; j++)
        {
            int y_tile = y * area_size.y + j;
            for (int i = 0; i < area_size.x; i++)
            {
                int x_tile = x * area_size.x + i;

                // print(x_tile + " " + y_tile + " " + gd[j][i]);
                
                // on set les tiles
                SetTile(x_tile, y_tile, fg[j][i], bg[j][i], gd[j][i]);
            }
        }

        // on refresh les tilemaps
        fg_tm.RefreshAllTiles();
        bg_tm.RefreshAllTiles();
        gd_tm.RefreshAllTiles();
    }

    // GETTERS
    public Sprite GetSprite(int x, int y, string layer="bg")
    {
        if (layer == "bg")
        {
            return bg_tm.GetSprite(new Vector3Int(x, y, 0));
        }
        else if (layer == "fg")
        {
            return fg_tm.GetSprite(new Vector3Int(x, y, 0));
        }
        else if (layer == "gd")
        {
            return gd_tm.GetSprite(new Vector3Int(x, y, 0));
        }
        return null;
    }
}