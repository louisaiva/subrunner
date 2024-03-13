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
    [SerializeField] private Dictionary<string, TileBase> fg_tiles = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> bg_tiles = new Dictionary<string, TileBase>();
    [SerializeField] private Dictionary<string, TileBase> gd_tiles = new Dictionary<string, TileBase>();


    [Header("Areas")]
    public Vector2Int area_size = new Vector2Int(16, 16);

    [Header("Zones")]
    [SerializeField] protected ZoneManager bank_zones;


    void Awake()
    {
        // on récupère les tilemaps
        fg_tm = transform.Find("fg").GetComponent<Tilemap>();
        bg_tm = transform.Find("bg").GetComponent<Tilemap>();
        gd_tm = transform.Find("gd").GetComponent<Tilemap>();

        // on récupère les tiles
        fg_tiles.Clear();
        bg_tiles.Clear();
        gd_tiles.Clear();
        fg_tiles.Add("base_sector", Resources.Load<TileBase>("tilesets/fg_2_rule"));
        bg_tiles.Add("base_sector", Resources.Load<TileBase>("tilesets/bg_base"));
        gd_tiles.Add("base_sector", Resources.Load<TileBase>("tilesets/gd_2_rule"));
        fg_tiles.Add("server", Resources.Load<TileBase>("tilesets/fg_2_rule"));
        bg_tiles.Add("server", Resources.Load<TileBase>("tilesets/bg_server"));
        gd_tiles.Add("server", Resources.Load<TileBase>("tilesets/gd_2_rule"));
        gd_tiles.Add("matrix", Resources.Load<TileBase>("tilesets/gd_matrix_rule"));
        gd_tiles.Add("red", Resources.Load<TileBase>("tilesets/gd_red"));
        fg_tiles.Add("labo", Resources.Load<TileBase>("tilesets/fg_2_rule"));
        bg_tiles.Add("labo", Resources.Load<TileBase>("tilesets/bg_labo"));
        gd_tiles.Add("labo", Resources.Load<TileBase>("tilesets/gd_white"));
        fg_tiles.Add("mecha", Resources.Load<TileBase>("tilesets/fg_2_rule"));
        bg_tiles.Add("mecha", Resources.Load<TileBase>("tilesets/bg_mecha"));
        gd_tiles.Add("mecha", Resources.Load<TileBase>("tilesets/gd_dark"));

        // on récupère le builder
        builder = GameObject.Find("generator").transform.Find("builder").GetComponent<AreaJsonHandler>();

        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère les legendary zones
        bank_zones = GetComponent<ZoneManager>();
    }

    // GENERATION
    public void GENERATE(List<Sector> sect, Sector spawn_sector)
    {

        // on clear les tilemaps
        Clear();

        // on récupère les sectors
        sectors = sect;

        // on sauvegarde les zones qui pourraient accueillir des zones légendaires
        List<Zone> central_zones = new List<Zone>();

        // on parcourt les secteurs
        for (int i = 0; i < sect.Count; i++)
        {
            // on vérifie si le secteur est un hand made sector
            if (sect[i] is ComplexeSector)
            {
                MergeSector((ComplexeSector) sect[i]);
            }

            // on initialise les areas des secteurs
            sect[i].initAreas();

            // on ajoute les zones qui pourraient accueillir des zones légendaires
            central_zones.AddRange(sect[i].getCentralZones());
        }

        // on affiche les central zones qu'on a récupéré
        /* string s = "central zones: ";
        foreach (Zone zone in central_zones)
        {
            s += zone.name + ", ";
        }
        print(s); */

        // on place 1 fois chaque zone légendaire
        foreach(GameObject zone_prefab in bank_zones.GetLegendaryZones())
        {
            // on choisit une zone au hasard
            Zone zone = central_zones[Random.Range(0, central_zones.Count)];
            central_zones.Remove(zone);

            // on place la zone
            zone.SetZone(zone_prefab);
        }

        // on refresh les tilemaps
        fg_tm.RefreshAllTiles();
        bg_tm.RefreshAllTiles();
        gd_tm.RefreshAllTiles();

        // on lance la génération interne des secteurs
        for (int i = 0; i < sect.Count; i++)
        {
            Sector sector = sect[i];

            // on génère le secteur
            sector.GENERATE();
        }

        // on créé le plafond
        CreateVirtualCeiling();


        // on récupère la position de départ du perso
        Vector2 spawn_pos;
        if (spawn_sector == null)
        {
            // on choisit une room au hasard dans le premier secteur
            Vector2Int room = sect[0].rooms.ElementAt(Random.Range(0, sect[0].rooms.Count));
            room.x += sect[0].x;
            room.y += sect[0].y;
            // spawn_pos = room * area_size + new Vector2Int(area_size.x / 2, area_size.y / 2);

            // on place le spawn au milieu de la room
            spawn_pos = new Vector2((room.x * area_size.x + area_size.x / 2)/2f, (room.y * area_size.y + area_size.y / 2)/2f);
        }
        else if (spawn_sector is ComplexeSector)
        {
            // on récupère la position de spawn du secteur
            spawn_pos = ((ComplexeSector)spawn_sector).getSpawnPos();
        }
        else
        {
            // on choisit une room au hasard dans le secteur
            Vector2Int room = spawn_sector.rooms.ElementAt(Random.Range(0, spawn_sector.rooms.Count));
            room.x += spawn_sector.x;
            room.y += spawn_sector.y;

            // on place le spawn au milieu de la room
            spawn_pos = new Vector2((room.x * area_size.x + area_size.x / 2) / 2f, (room.y * area_size.y + area_size.y / 2) / 2f);
        }


        // on place le perso
        perso.transform.position = new Vector3(spawn_pos.x, spawn_pos.y, 0f);
        perso.transform.Find("minicam").GetComponent<Minimap>().Clear();

    }

    private void MergeSector(ComplexeSector sector)
    {
        // on récupère les TileBase[] des tilemaps initiales
        TileBase[] init_fg_tiles = sector.getHandmadeTiles("fg");
        TileBase[] init_bg_tiles = sector.getHandmadeTiles("bg");
        TileBase[] init_gd_tiles = sector.getHandmadeTiles("gd");

        // on récupère le BoundsInt final
        BoundsInt final_bounds = sector.getHandmadeBounds();

        // on set les tiles
        setTiles(final_bounds, init_fg_tiles, "fg");
        setTiles(final_bounds, init_bg_tiles, "bg");
        setTiles(final_bounds, init_gd_tiles, "gd");

        // on launch le secteur
        sector.LAUNCH();
    }

    private void CreateVirtualCeiling()
    {
        // crée un enorme sprite de "plafond" pour éviter de voir le vide
        // plafond virtuel car en réalité placé au niveau du sol

        // on récupère la taille max des tilemaps
        Vector2Int size = GetSize();

        // on créé le sprite
        GameObject ceiling = new GameObject("ceiling");
        ceiling.transform.parent = transform;
        ceiling.transform.position = new Vector3(size.x / 4f, size.y / 4f, 0f);

        // on ajoute une marge
        size += new Vector2Int(20, 20);

        // on créé le sprite renderer
        SpriteRenderer sr = ceiling.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("sprites/utils/almost_black");

        // on calcule la scale pour lui donner la bonne taille
        float scaleX = (size.x/2f) / sr.bounds.size.x;
        float scaleY = (size.y/2f) / sr.bounds.size.y;

        // Set the scale of the GameObject
        ceiling.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // sr.size = new Vector2(size.x/2f, size.y/2f);
        sr.sortingLayerName = "ground";
        sr.sortingOrder = -1;

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

        // on clear le plafond
        if (transform.Find("ceiling") != null)
        {
            Destroy(transform.Find("ceiling").gameObject);
        }
    }


    
    // SETTERS
    public void SetLayerTile(int x, int y, int tile=0, string layer="bg",string skin="base_sector")
    {
        if (layer == "bg")
        {
            bg_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : bg_tiles[skin]);
        }
        else if (layer == "fg")
        {
            fg_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : fg_tiles[skin]);
        }
        else if (layer == "gd")
        {
            gd_tm.SetTile(new Vector3Int(x, y, 0), tile == 0 ? null : gd_tiles[skin]);
        }
    }

    public void SetTile(int x, int y, int fg, int bg, int gd, string skin="base_sector")
    {
        SetLayerTile(x, y, fg, "fg", skin);
        SetLayerTile(x, y, bg, "bg", skin);
        SetLayerTile(x, y, gd, "gd", skin);
    }

    public void SetAreaTiles(int x, int y, AreaJson area, string skin="base_sector")
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
                SetTile(x_tile, y_tile, area.fg[j][i], area.bg[j][i], area.gd[j][i], skin);
            }
        }

        /* // on refresh les tilemaps
        fg_tm.RefreshAllTiles();
        bg_tm.RefreshAllTiles();
        gd_tm.RefreshAllTiles(); */
    }

    public void PlaceTile(TileBase tile, Vector2Int pos, string layer="bg")
    {
        // on place une tile dans le monde
        if (layer == "bg")
        {
            bg_tm.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
        }
        else if (layer == "fg")
        {
            fg_tm.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
        }
        else if (layer == "gd")
        {
            gd_tm.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
        }
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

    public Sector getSector(Vector2Int tile_pos)
    {
        // renvoie le sector correspondant à la tile_position de la tile

        // on récupère le areaPos
        Vector2Int areaPos = getAreaPos(tile_pos);

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

    public string getAreaName(Vector2Int tile_pos)
    {
        // on récupère le presector
        Sector sect = getSector(tile_pos);

        if (sect == null)
        {
            // Debug.LogError("(world - getAreaName) no sector found for " + tile_pos);
            return "null";
        }

        // on récupère la position locale de l'area dans le presector
        Vector2Int areaPos = getAreaPos(tile_pos) - sect.xy();

        // on récupère le nom de l'area
        return sect.getAreaName(areaPos);
    }

    public Vector2Int getAreaPos(Vector2Int tile_pos)
    {
        // on récupère le areaPos
        Vector2Int areaPos = new Vector2Int(tile_pos.x / area_size.x, tile_pos.y / area_size.y);

        return areaPos;
    }

    public Vector2Int getLocalAreaPos(Vector2Int tile_pos)
    {
        // on récupère le sector
        Sector sect = getSector(tile_pos);

        if (sect == null)
        {
            // Debug.LogError("(world - getLocalAreaPos) no sector found for " + tile_pos);
            return new Vector2Int(-1, -1);
        }

        // on récupère la position locale de l'area dans le sector
        Vector2Int local_area_pos = getAreaPos(tile_pos) - sect.xy();

        return local_area_pos;
    }

    public Vector2Int getLocalTilePos(Vector2Int pos)
    {
        return new Vector2Int(pos.x % area_size.x, pos.y % area_size.y);
    }

    public string getTileType(Vector2Int tile)
    {
        // on récupère l'area
        string area_name = getAreaName(tile);
        if (area_name == "ceiling") { return "ceiling"; }
        else if (area_name == "null") { return "ceiling"; }
        else if (area_name == "handmade") { return getActualTileType(tile,"handmade"); }

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
            return getActualTileType(tile);
        }
    }

    public string getActualTileType(Vector2Int tile,string not_found="not found")
    {
        // récupère directement le type de tile dans les tilemaps
        TileBase fg = fg_tm.GetTile(new Vector3Int(tile.x, tile.y, 0));
        if (fg != null) { return "ceiling"; }
        TileBase bg = bg_tm.GetTile(new Vector3Int(tile.x, tile.y, 0));
        if (bg != null) { return "wall"; }
        TileBase gd = gd_tm.GetTile(new Vector3Int(tile.x, tile.y, 0));
        if (gd != null) { return "ground"; }
        
        // on regarde si il n'y a pas un wall dans les 3 cases dessous
        TileBase bg1 = bg_tm.GetTile(new Vector3Int(tile.x, tile.y-1, 0));
        if (bg1 != null) { return "wall"; }
        TileBase bg2 = bg_tm.GetTile(new Vector3Int(tile.x, tile.y-2, 0));
        if (bg2 != null) { return "wall"; }
        TileBase bg3 = bg_tm.GetTile(new Vector3Int(tile.x, tile.y-3, 0));
        if (bg3 != null) { return "wall"; }

        // sinon on renvoie null
        return not_found;
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

    public void setTiles(BoundsInt bounds,TileBase[] tiles, string tm="bg")
    {
        // on set les tiles
        if (tm == "bg")
        {
            bg_tm.SetTilesBlock(bounds, tiles);
        }
        else if (tm == "fg")
        {
            fg_tm.SetTilesBlock(bounds, tiles);
        }
        else if (tm == "gd")
        {
            gd_tm.SetTilesBlock(bounds, tiles);
        }
    }



    // PERSO GETTERS
    public string getPersoAreaName()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récupère le nom de la zone
        return getAreaName(pos);
    }

    public string getPersoTilePos()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récup la position locale
        Vector2Int local_pos = getLocalTilePos(pos);

        return "(" + local_pos.x + ", " + local_pos.y + ")";
    }

    public string getPersoAreaPos()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récupère la position de l'area
        Vector2Int area_pos = getAreaPos(pos);

        return "(" + area_pos.x + ", " + area_pos.y + ")";
    }

    public Vector2Int getPersoPos()
    {
        // on récupère la position du perso
        Vector2Int pos = new Vector2Int((int)(perso.transform.position.x * 2), (int)(perso.transform.position.y * 2));

        return pos;
    }

}