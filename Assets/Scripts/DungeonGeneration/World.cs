using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class World : MonoBehaviour
{

    [Header("Builder")]
    [SerializeField] private AreaJsonHandler builder;
    [SerializeField] private Perso perso;
    
    [Header("Sectors")]
    [SerializeField] private List<PreSector> pre_sectors = new List<PreSector>();

    // [Header("World")]
    // [SerializeField] private int width;
    // [SerializeField] private int height;
    
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
        builder = GameObject.Find("generator").transform.Find("builder").GetComponent<AreaJsonHandler>();

        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();
    }



    // GENERATION
    public void GENERATE(List<PreSector> psec)
    {
        // on clear les tilemaps
        Clear();

        // on récupère les pre_sectors
        pre_sectors = psec;

        // on parcourt les secteurs
        for (int i = 0; i < psec.Count; i++)
        {
            Vector2Int sector_pos = psec[i].xy();

            // on parcourt les areas
            foreach (Vector2Int areaPos in psec[i].tiles)
            {
                // on récupère le nom de l'area
                string area_name = psec[i].getAreaName(areaPos);

                // on place l'area
                PlaceArea(sector_pos.x + areaPos.x, sector_pos.y + areaPos.y, area_name);
            }
        }

        // on récupère la position de départ du perso
        Vector2Int room = psec[0].rooms.ElementAt(Random.Range(0, psec[0].rooms.Count));
        Vector2Int pos = room*area_size + new Vector2Int(area_size.x / 2, area_size.y / 2);

        // on place le perso
        perso.transform.position = new Vector3(pos.x / 2f, pos.y / 2f, 0f);
        perso.transform.Find("minicam").GetComponent<Minimap>().Clear();

    }

    private void PlaceArea(int x, int y, string area_name)
    {
        // on choisit un fichier json d'area aléatoire
        string area_json = builder.GetAreaJson(area_name);

        // on set les tiles de l'area
        SetAreaTiles(x, y, area_json);
    }

    private void Clear()
    {
        // on vérifie que les tilemaps existent
        if (fg_tm == null || bg_tm == null || gd_tm == null)
        {
            Awake();
        }

        // on clear les tilemaps
        fg_tm.ClearAllTiles();
        bg_tm.ClearAllTiles();
        gd_tm.ClearAllTiles();
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

    public void GetTilemaps(out Tilemap fg, out Tilemap bg, out Tilemap gd)
    {
        fg = fg_tm;
        bg = bg_tm;
        gd = gd_tm;
    }

    public Vector2Int GetSize()
    {
        // on récupère la taille max des tilemaps
        Vector2Int size = new Vector2Int(0, 0);
        foreach (Tilemap tm in new Tilemap[] { fg_tm, bg_tm, gd_tm })
        {
            tm.CompressBounds();
            if (tm.size.x > size.x) size.x = tm.size.x;
            if (tm.size.y > size.y) size.y = tm.size.y;
        }
        return size;
    }

    public PreSector getSector(Vector2Int pos)
    {
        // renvoie le presector correspondant à la position de la tile

        // on récupère le areaPos
        Vector2Int areaPos = getAreaPos(pos);

        // on parcourt tous les pre_sectors pour trouver celui qui correspond
        foreach (PreSector psec in pre_sectors)
        {
            // on vérifie si le psec correspond
            if (psec.collidesWithRoomPoint(areaPos))
            {
                return psec;
            }
        }
        return null;
    }

    public string getAreaName(Vector2Int pos)
    {
        // on récupère le presector
        PreSector psec = getSector(pos);

        if (psec == null)
        {
            // print("no presector found for " + pos);

            return "null";
        }

        // on récupère la position locale de l'area dans le presector
        Vector2Int areaPos = getAreaPos(pos) - psec.xy();

        // on récupère le nom de l'area
        return psec.getAreaName(areaPos);
    }

    public Vector2Int getAreaPos(Vector2Int tile_pos)
    {
        // on récupère le areaPos
        Vector2Int areaPos = new Vector2Int(tile_pos.x / area_size.x, tile_pos.y / area_size.y);

        return areaPos;
    }

    public Vector2Int getLocalTilePos(Vector2Int pos)
    {
        return new Vector2Int(pos.x % area_size.x, pos.y % area_size.y);
    }

    public string getTileType(Vector2Int tile)
    {
        // on récupère l'area
        string area_name = getAreaName(tile);
        if (area_name == "ceiling") { return "ceiling";}
        else if (area_name == "null") { return "ceiling";}

        // on récupère la position locale de la tile dans l'area
        Vector2Int local_tile_pos = getLocalTilePos(tile);

        // on récupère le json de l'area
        string json = builder.GetAreaJson(area_name);

        // on parse le json
        Dictionary<string, List<List<int>>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);

        // on vérifie quelle tile est à la position
        if (dict["fg"][local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "ceiling";
        }
        else if (dict["bg"][local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "wall";
        }
        else if (dict["gd"][local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "ground";
        }
        else
        {
            return "not found";
        }
    }

}