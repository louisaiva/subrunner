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
    [SerializeField] private List<Sector> sectors = new List<Sector>();
    
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
    public void GENERATE(List<Sector> sect)
    {

        // on clear les tilemaps
        Clear();

        // on récupère les sectors
        sectors = sect;

        // on parcourt les secteurs
        for (int i = 0; i < sect.Count; i++)
        {
            Vector2Int sector_pos = sect[i].xy();

            // on parcourt les areas
            foreach (Vector2Int areaPos in sect[i].tiles)
            {
                // on récupère le nom de l'area
                string area_name = sect[i].getAreaName(areaPos);

                // on place l'area
                PlaceArea(sector_pos.x + areaPos.x, sector_pos.y + areaPos.y, area_name);
            }
        }

        // on récupère la position de départ du perso
        Vector2Int room = sect[0].rooms.ElementAt(Random.Range(0, sect[0].rooms.Count));
        Vector2Int pos = room*area_size + new Vector2Int(area_size.x / 2, area_size.y / 2);

        // on place le perso
        perso.transform.position = new Vector3(pos.x / 2f, pos.y / 2f, 0f);
        perso.transform.Find("minicam").GetComponent<Minimap>().Clear();

        // on lance la génération des secteurs
        foreach (Sector sector in sectors)
        {
            // print("getting emplacements for sector");

            GetEmplacements(sector, out List<Vector2> empl_enemies, out List<Vector2> empl_interactives);
            print("empl enemies: " + empl_enemies.Count);

            // on génère le secteur
            sector.GENERATE(empl_enemies, empl_interactives);
        }

    }

    private void PlaceArea(int x, int y, string area_name)
    {
        // on choisit un fichier json d'area aléatoire
        // string area_json = builder.GetAreaJson(area_name);
        AreaJson area_json = builder.LoadAreaJson(area_name);

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

        // on clear les enemis
        foreach (Transform child in transform.Find("enemies"))
        {
            Destroy(child.gameObject);
        }
    }

    // EMPLACEMENTS
    private void GetEmplacements(Sector sector, out List<Vector2> empl_enemies, out List<Vector2> empl_interactives)
    {
        // on créé les emplacements
        empl_enemies = new List<Vector2>();
        empl_interactives = new List<Vector2>();

        // on parcourt les rooms
        foreach (Vector2Int area in sector.tiles)
        {
            // on récupère la position de l'area
            Vector2Int area_pos = (area + sector.xy());


            // on récupère les emplacements de la room
            List<Vector2> empl_e, empl_i;
            GetAreaEmplacements(area_pos, out empl_e, out empl_i);

            // on ajoute les emplacements à la liste
            empl_enemies.AddRange(empl_e);
            empl_interactives.AddRange(empl_i);
        }
    }

    private void GetAreaEmplacements(Vector2Int area_pos, out List<Vector2> empl_enemies, out List<Vector2> empl_interactives)
    {
        // on créé les emplacements
        empl_enemies = new List<Vector2>();
        empl_interactives = new List<Vector2>();

        // on convertit l'area_pos en tile_pos
        Vector2Int tile_pos = area_pos * area_size + new Vector2Int(area_size.x / 2, area_size.y / 2);


        // on récupère l'area
        string area_name = getAreaName(tile_pos);

        // print("getting emplacements for area " + area_name + " at " + area_pos + "("+ tile_pos + ")");

        // on récupère le json de l'area
        AreaJson area = builder.LoadAreaJson(area_name);

        // on récupère les emplacements
        Dictionary<string, HashSet<Vector2>> empl = area.GetEmplacements();
        string s= "empl enemies: ";
        foreach (string key in empl.Keys)
        {
            s += key + ": " + empl[key].Count + ", ";
        }
        // print(s);

        if (empl.ContainsKey("enemy"))
        {
            foreach (Vector2 emp in empl["enemy"])
            {
                // print("adding enemy at " + (pos.x + tile_pos.x) + ", " + (pos.y + tile_pos.y));
                Vector3 pos = CellToWorld(new Vector3Int(tile_pos.x, tile_pos.y, 0));
                empl_enemies.Add(new Vector2(pos.x + emp.x, pos.y + emp.y));
            }
        }
        if (empl.ContainsKey("interactive"))
        {
            foreach (Vector2 emp in empl["interactive"])
            {
                Vector3 pos = CellToWorld(new Vector3Int(tile_pos.x, tile_pos.y, 0));
                empl_interactives.Add(new Vector2(pos.x + emp.x, pos.y + emp.y));
            }
        }

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

    public void SetAreaTiles(int x, int y, AreaJson area)
    {
        // loads the json to the tilemaps

        // on ajoute des tiles aux tilemaps
        for (int j = 0; j < area_size.y; j++)
        {
            int y_tile = y * area_size.y + j;
            for (int i = 0; i < area_size.x; i++)
            {
                int x_tile = x * area_size.x + i;
                
                // on set les tiles
                SetTile(x_tile, y_tile, area.fg[j][i], area.bg[j][i], area.gd[j][i]);
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

    public TileBase GetTile(int x, int y, string layer="bg")
    {
        if (layer == "bg")
        {
            return bg_tm.GetTile(new Vector3Int(x, y, 0));
        }
        else if (layer == "fg")
        {
            return fg_tm.GetTile(new Vector3Int(x, y, 0));
        }
        else if (layer == "gd")
        {
            return gd_tm.GetTile(new Vector3Int(x, y, 0));
        }
        return null;
    }

    public TileData GetTileData(int x, int y, string layer="bg")
    {
        TileBase tile = GetTile(x, y, layer);
        TileData tile_data = new TileData();

        if (tile == null) { return tile_data; }

        if (layer == "bg")
        {
            tile.GetTileData(new Vector3Int(x, y, 0), bg_tm, ref tile_data);
        }
        else if (layer == "fg")
        {
            tile.GetTileData(new Vector3Int(x, y, 0), fg_tm, ref tile_data);
        }
        else if (layer == "gd")
        {
            tile.GetTileData(new Vector3Int(x, y, 0), gd_tm, ref tile_data);
        }

        return tile_data;
    }

    public Vector3 CellToWorld(Vector3Int cellPos, string layer="bg")
    {
        if (layer == "bg")
        {
            return bg_tm.CellToWorld(cellPos);
        }
        else if (layer == "fg")
        {
            return fg_tm.CellToWorld(cellPos);
        }
        else if (layer == "gd")
        {
            return gd_tm.CellToWorld(cellPos);
        }
        return Vector3.zero;
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

    public Sector getSector(Vector2Int pos)
    {
        // renvoie le presector correspondant à la position de la tile

        // on récupère le areaPos
        Vector2Int areaPos = getAreaPos(pos);

        // on parcourt tous les sectors pour trouver celui qui correspond
        foreach (Sector sect in sectors)
        {
            // on vérifie si le sect correspond
            if (sect.collidesWithRoomPoint(areaPos))
            {
                return sect;
            }
        }
        return null;
    }

    public string getAreaName(Vector2Int pos)
    {
        // on récupère le presector
        Sector sect = getSector(pos);

        if (sect == null)
        {
            // print("no presector found for " + pos);

            return "null";
        }

        // on récupère la position locale de l'area dans le presector
        Vector2Int areaPos = getAreaPos(pos) - sect.xy();

        // on récupère le nom de l'area
        return sect.getAreaName(areaPos);
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

        AreaJson area = builder.LoadAreaJson(area_name);

        // on vérifie quelle tile est à la position
        if (area.fg[local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "ceiling";
        }
        else if (area.bg[local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "wall";
        }
        else if (area.gd[local_tile_pos.y][local_tile_pos.x] != 0)
        {
            return "ground";
        }
        else
        {
            return "not found";
        }
    }

    public TileBase[] getTiles(BoundsInt bounds,string tm="bg")
    {
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