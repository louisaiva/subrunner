using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using UnityEditor;
using System.Linq;

public class AreaJsonHandler : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private string area_to_load = "";

    [Header("Saving")]
    [SerializeField] private string rooms_path = "prefabs/rooms/";
    [SerializeField] private string areas_path = "Assets/Resources/prefabs/areas/";
    [SerializeField] private string tile_areas_path = "Assets/Resources/prefabs/tileareas/";
    [SerializeField] private List<GameObject> areas;

    // SELECTING AREAS

    public void SelectAllAreas()
    {
        areas = new List<GameObject>();
        foreach (GameObject area in Resources.LoadAll<GameObject>(rooms_path))
        {
            if (area.name == "empty") { continue;}
            areas.Add(area);
        }
    }

    // CHANGING TILEMAPS

    public void MoveUpBg(ref Tilemap fg_tm, ref Tilemap bg_tm, ref Tilemap gd_tm)
    {
        // ici on applique différentes modifications aux tilemaps
        // tout d'abord on veut remonter les tiles de 1 unité

        // on reset l'anchor des tilemaps
        fg_tm.tileAnchor = new Vector3(0.5f, 0.5f, 0);
        bg_tm.tileAnchor = new Vector3(0.5f, 2f, 0);
        gd_tm.tileAnchor = new Vector3(0.5f, 0.5f, 0);

        // on récupère les dimensions de l'area
        GetAreaDimensions(new List<Tilemap> { bg_tm }, out Vector2Int position, out Vector2Int size);

        // on crée une liste de tiles à remonter
        List<Vector2Int> tiles_to_move = new List<Vector2Int>();

        // on parcourt les tiles
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                // on récupère les tiles
                TileBase tile = bg_tm.GetTile(new Vector3Int(x, y, 0));

                // on remonte les tiles
                if (tile != null)
                {
                    tiles_to_move.Add(new Vector2Int(x, y));
                }
            }
        }

        // on remonte les tiles
        foreach (Vector2Int tile in tiles_to_move)
        {
            bg_tm.SetTile(new Vector3Int(tile.x, tile.y, 0), null);
            bg_tm.SetTile(new Vector3Int(tile.x, tile.y + 1, 0), Resources.Load<TileBase>("tilesets/walls_1_rule"));
        }
    }

    public void FillEmptyBottomLines(ref Tilemap fg_tm, ref Tilemap gd_tm)
    {
        // on vérifie si on a une ligne de zéros dans fg_tm tout en bas
        // si oui on la remplit de tiles sauf au milieu

        // on vérifie si on a 2 ligne de zéros dans gd_tm tout en bas
        // si oui on remplit le milieu de tiles

        if (fg_tm == null || gd_tm == null) { return; }
        
        bool is_fg_affected = true;
        bool is_gd_affected = true;

        for (int x = -8; x < 8; x++)
        {
            if (fg_tm.GetTile(new Vector3Int(x, 7, 0)) != null)
            {
                is_fg_affected = false;
            }
            if (gd_tm.GetTile(new Vector3Int(x, 7, 0)) != null)
            {
                is_gd_affected = false;
            }
            else if (gd_tm.GetTile(new Vector3Int(x, 6, 0)) != null)
            {
                is_gd_affected = false;
            }
        }

        print("filling empty bottom lines : " + is_fg_affected + " " + is_gd_affected);

        if (is_fg_affected)
        {
            for (int x = -8; x < 8; x++)
            {
                if (x == 0 || x == -1) { continue; }

                fg_tm.SetTile(new Vector3Int(x, 7, 0), Resources.Load<TileBase>("tilesets/fg_2_rule"));
            }
        }

        if (is_gd_affected)
        {
            gd_tm.SetTile(new Vector3Int(-1,7, 0), Resources.Load<TileBase>("tilesets/gd_2_rule"));
            gd_tm.SetTile(new Vector3Int(0, 7, 0), Resources.Load<TileBase>("tilesets/gd_2_rule"));
            gd_tm.SetTile(new Vector3Int(-1, 6, 0), Resources.Load<TileBase>("tilesets/gd_2_rule"));
            gd_tm.SetTile(new Vector3Int(0, 6, 0), Resources.Load<TileBase>("tilesets/gd_2_rule"));
        }

    }

    // SAVING SELECTED AREAS

    public void SaveJson(GameObject area)
    {
        
        // * format de sauvegarde
        /*
            fichier json contenant

            {
                "fg" : [...],
                "bg" : [...],
                "gd" : [...],

                "emplacements" : {
                    "type1" : [{"x":-1.25,"y":0.0},...],
                    "type2" : [{"x":-1.25,"y":0.0},...],
                    ...
                }
            }

            où [...] est une liste de liste d'entiers définit par :
            - 0 : tile vide
            - 1 : tile pleine de base (fg = ceiling, gd = ground, bg = normal wall)
            - 2 : semi tile L (bg = left wall, fg = left ceiling)
            - 3 : semi tile R (bg = right wall, fg = right ceiling)

        */
        
        // on récupère le type d'aera (corr, room, room ext)
        string area_type = "";
        if (area.name.Contains("corridor_"))
            area_type = "corridors/";
        else if (area.name.Contains("room_"))
            if (area.name.Contains("S") || area.name.Contains("N") || area.name.Contains("E") || area.name.Contains("W"))
                area_type = "rooms_ext/";
            else
                area_type = "rooms/";

        // on en déduit le path du fichier json
        string path = tile_areas_path + area_type + area.name + ".json";

        // on sauvegarde les données des tilemaps dans des listes
        List<List<int>> fg = new List<List<int>>();
        List<List<int>> bg = new List<List<int>>();
        List<List<int>> gd = new List<List<int>>();

        // on récupère les tilemaps
        Tilemap fgTilemap = area.transform.Find("fg_tilemap").GetComponent<Tilemap>();
        Tilemap bgTilemap = area.transform.Find("bg_tilemap").GetComponent<Tilemap>();
        Tilemap gdTilemap = area.transform.Find("gd_tilemap").GetComponent<Tilemap>();

        // on applique des changements à nos tilemaps !!
        FillEmptyBottomLines(ref fgTilemap, ref gdTilemap);

        bool flip_x = false;
        if (area.transform.localScale.x < 0) { flip_x = true; }

        // on récupère les dimensions de l'area
        string json_fg = "\"fg\" : [\n";
        string json_bg = "\"bg\" : [\n";
        string json_gd = "\"gd\" : [\n";

        // on parcourt les tiles
        for (int y = -8; y < 8; y++)
        {
            // on ajoute une ligne à chaque liste
            fg.Add(new List<int>());
            bg.Add(new List<int>());
            gd.Add(new List<int>());

            // on ajoute une ligne à chaque string
            json_fg += "[";
            json_bg += "[";
            json_gd += "[";

            for (int x = -8; x < 8; x++)
            {
                // on flip les tiles
                int fx = x;
                if (flip_x) { fx = (15-(x+8))-8; }

                // on récupère les tiles
                TileBase fgTile = fgTilemap.GetTile(new Vector3Int(fx, y, 0));
                TileBase bgTile = bgTilemap.GetTile(new Vector3Int(fx, y, 0));
                TileBase gdTile = gdTilemap.GetTile(new Vector3Int(fx, y, 0));

                // on ajoute les tiles à la liste
                fg[fg.Count - 1].Add(fgTile == null ? 0 : 1);
                bg[bg.Count - 1].Add(bgTile == null ? 0 : 1);
                gd[gd.Count - 1].Add(gdTile == null ? 0 : 1);

                // on ajoute les tiles à la string
                json_fg += fgTile == null ? "0," : "1,";
                json_gd += gdTile == null ? "0," : "1,";
                json_bg += bgTile == null ? "0," : "1,";
            }

            // on enlève la dernière virgule
            json_fg = json_fg.Remove(json_fg.Length - 1);
            json_bg = json_bg.Remove(json_bg.Length - 1);
            json_gd = json_gd.Remove(json_gd.Length - 1);

            // on ajoute une ligne à chaque string
            json_fg += "],\n";
            json_bg += "],\n";
            json_gd += "],\n";
        }

        // on enlève la dernière virgule
        json_fg = json_fg.Remove(json_fg.Length - 1);
        json_bg = json_bg.Remove(json_bg.Length - 1);
        json_gd = json_gd.Remove(json_gd.Length - 1);
        json_fg = json_fg.Remove(json_fg.Length - 1);
        json_bg = json_bg.Remove(json_bg.Length - 1);
        json_gd = json_gd.Remove(json_gd.Length - 1);

        // on ferme les listes
        json_fg += "]";
        json_bg += "]";
        json_gd += "]";

        // on crée un dict contenant les listes
        // Dictionary<string, List<List<int>>> dict = new Dictionary<string, List<List<int>>>();
        // dict.Add("fg", fg);
        // dict.Add("bg", bg);
        // dict.Add("gd", gd);

        // on cherche les emplacements
        Dictionary<string, HashSet<Vector2>> emplacements = new Dictionary<string, HashSet<Vector2>>();
        if (area.transform.Find("emplacements") != null)
        {
            foreach (Transform emplacement in area.transform.Find("emplacements"))
            {
                string type = emplacement.name.Split('_')[0];
                Vector2 pos = emplacement.position;
                if (!emplacements.ContainsKey(type))
                {
                    emplacements.Add(type, new HashSet<Vector2>());
                }

                if (new string[] {"enemy","interactive"}.Contains(type))
                {
                    // on déplace la pos en Y de -0.25
                    pos.y -= 0.25f;
                }


                // on ajoute
                emplacements[type].Add(pos);

                print("adding " + type + " at " + pos);
            }
        }



        // on convertit les emplacements en json
        string json_empl = "\"emplacements\":{\n";
        foreach (string type in emplacements.Keys)
        {
            json_empl += "\"" + type + "\" : [";
            foreach (Vector2 pos in emplacements[type])
            {
                json_empl += "{\"x\":" + pos.x.ToString().Replace(',', '.') + ",\"y\":" + pos.y.ToString().Replace(',', '.') + "},\n";
            }
            json_empl = json_empl.Remove(json_empl.Length - 1);
            json_empl = json_empl.Remove(json_empl.Length - 1);
            json_empl += "],\n";
        }
        json_empl = json_empl.Remove(json_empl.Length - 1);
        json_empl = json_empl.Remove(json_empl.Length - 1);
        json_empl += "}";





        // on sauvegarde les listes dans un fichier json
        string json = "{\n\n" + json_fg + ",\n\n" + json_bg + ",\n\n" + json_gd + ",\n\n" + json_empl + "\n\n}";
        // AreaJson area_json = new AreaJson(fg, bg, gd, emplacements);
        // string json = JsonConvert.SerializeObject(area_json, Formatting.None);
        print("saving " + area.name + " to " + path);
        System.IO.File.WriteAllText(path, json);
    }

    public void GetAreaDimensions(List<Tilemap> tms,out Vector2Int position, out Vector2Int size)
    {
        // on cherche les bonnes dimensions
        int width = 0;
        int height = 0;
        int x = 50000;
        int y = 50000;

        // on parcourt les tilemaps
        foreach (Tilemap tm in tms)
        {
            // on compresse les bounds
            tm.CompressBounds();

            // on récupère les dimensions
            if (tm.size.x > width) { width = (int)tm.size.x; }
            if (tm.size.y > height) { height = (int)tm.size.y; }
            if ((int)tm.cellBounds.xMin < x) { x = (int)tm.cellBounds.xMin; }
            if ((int)tm.cellBounds.yMin < y) { y = (int)tm.cellBounds.yMin; }
        }

        // on définit la position et la taille de l'area
        position = new Vector2Int(x, y);
        size = new Vector2Int(width, height);
    }

    public void SaveTileAreas()
    {
        foreach (GameObject area in areas)
        {
            SaveJson(area);
        }
    }

    // LOADING
    public void LoadArea()
    {
        if (area_to_load == "") { return; }
        GameObject area = LoadFromJson(area_to_load);
    }

    public GameObject LoadFromJson(string name)
    {

        // on récupère le json
        string json = GetAreaJson(name);

        // on le parse
        Dictionary<string, List<List<int>>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);

        print(dict.Count + " " + dict);

        // on récupère les listes
        List<List<int>> fg = dict["fg"];
        List<List<int>> bg = dict["bg"];
        List<List<int>> gd = dict["gd"];

        // on instancie une empty area
        GameObject area = Resources.Load<GameObject>(rooms_path + "empty");
        area = Instantiate(area);
        area.name = name;

        // on ajoute des tiles aux tilemaps
        for (int y = 0; y < fg.Count; y++)
        {
            for (int x = 0; x < fg[y].Count; x++)
            {
                // on récupère les tiles
                TileBase fgTile = fg[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/fg_2_rule");
                TileBase bgTile = bg[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/walls_1_rule");
                TileBase gdTile = gd[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/gd_2_rule");

                // on ajoute les tiles aux tilemaps
                area.transform.Find("fg_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), fgTile);
                area.transform.Find("bg_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), bgTile);
                area.transform.Find("gd_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), gdTile);
            }
        }

        // on retourne l'area
        return area;

    }

    public AreaJson LoadAreaJson(string name)
    {
        // on récupère le json
        string json = GetAreaJson(name);

        // on le parse
        AreaJson area_json = JsonConvert.DeserializeObject<AreaJson>(json);

        // on retourne l'area
        return area_json;
    }

    public string GetRandomAreaJson()
    {
        // on récupère un fichier json aléatoire
        string[] files = System.IO.Directory.GetFiles(tile_areas_path, "*.json", System.IO.SearchOption.AllDirectories);
        string file = files[Random.Range(0, files.Length)];
        string json = System.IO.File.ReadAllText(file);

        // on retourne le json
        return json;
    }

    public string GetAreaJson(string name)
    {
        // on récupère le type d'aera (corr, room, room ext)
        string area_type = "";
        if (name.Contains("corridor_"))
            area_type = "corridors/";
        else if (name.Contains("room_"))
            if (name.Contains("S") || name.Contains("N") || name.Contains("E") || name.Contains("W"))
                area_type = "rooms_ext/";
            else
                area_type = "rooms/";

        // on en déduit le path du fichier json
        string path = tile_areas_path + area_type + name + ".json";

        // on récupère le json
        string json = System.IO.File.ReadAllText(path);

        // on retourne le json
        return json;
    }


    // LOADING THEN SAVING TO PREFABS
    public void SaveAllAreas()
    {
        foreach (string directory in System.IO.Directory.GetDirectories(tile_areas_path))
        {

            string dir = directory.Split('/')[directory.Split('/').Length - 1] + "\\";
            print("saving areas prefabs to" + dir);
            SaveDirAreas(dir);
        }
    }

    public void SaveDirAreas(string directory)
    {
        // we load all areas from json
        foreach (string area_name in System.IO.Directory.GetFiles(tile_areas_path + directory, "*.json"))
        {
            // print("loading " + area_name);
            string name = area_name.Split('/')[area_name.Split('/').Length - 1].Split('.')[0];
            name = area_name.Split('\\')[area_name.Split('\\').Length - 1].Split('.')[0];

            // on récupère le json
            string json = System.IO.File.ReadAllText(area_name);


            // on récupère les listes
            List<List<int>> fg = new List<List<int>>();
            List<List<int>> bg = new List<List<int>>();
            List<List<int>> gd = new List<List<int>>();

            // et les emplacements
            Dictionary<string, HashSet<Vector2>> emplacements = new Dictionary<string, HashSet<Vector2>>();

            // on regarde si on a un dictionnaire ou un areajson
            bool deserialise_dict = false;
            /* foreach (char c in json)
            {
                if (c == '{')
                {
                    deserialise_dict = true;
                    break;
                }
            } */

            // on deserialise le json
            if (deserialise_dict)
            {
                // on le parse
                Dictionary<string, List<List<int>>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(json);

                // on récupère les listes
                fg = dict["fg"];
                bg = dict["bg"];
                gd = dict["gd"];
            }
            else
            {
                // on le parse
                AreaJson area_json = JsonConvert.DeserializeObject<AreaJson>(json);

                // on récupère les listes
                fg = area_json.fg;
                bg = area_json.bg;
                gd = area_json.gd;

                // et les emplacements
                emplacements = area_json.GetEmplacements();
            }

            // on instancie une empty area
            GameObject area = Resources.Load<GameObject>(rooms_path + "empty");
            area = Instantiate(area);
            // area.name = area_name.Split('/')[area_name.Split('/').Length - 1].Split('.')[0];

            // on ajoute des tiles aux tilemaps
            for (int y = 0; y < fg.Count; y++)
            {
                for (int x = 0; x < fg[y].Count; x++)
                {
                    // on récupère les tiles
                    TileBase fgTile = fg[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/fg_2_rule");
                    TileBase bgTile = bg[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/walls_1_rule");
                    TileBase gdTile = gd[y][x] == 0 ? null : Resources.Load<TileBase>("tilesets/gd_2_rule");

                    // on ajoute les tiles aux tilemaps
                    area.transform.Find("fg_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), fgTile);
                    area.transform.Find("bg_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), bgTile);
                    area.transform.Find("gd_tilemap").GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), gdTile);
                }
            }

            // on ajoute les emplacements
            foreach (string type in emplacements.Keys)
            {
                foreach (Vector2 pos in emplacements[type])
                {
                    GameObject emplacement = new GameObject(type);
                    emplacement.transform.position = pos;
                    emplacement.transform.parent = area.transform.Find("emplacements");
                }
            }

            // on sauvegarde l'area
            string path = areas_path + directory + name + ".prefab";
            // print("saving " + name + " to " + path);
            PrefabUtility.SaveAsPrefabAsset(area, path);

            // on détruit l'area
            DestroyImmediate(area);
        }
    }
}

public class AreaJson
{
    public List<List<int>> fg;
    public List<List<int>> bg;
    public List<List<int>> gd;

    public Dictionary<string, HashSet<Point>> emplacements;


    public AreaJson()
    {
        // on positionne les tm
        this.fg = new List<List<int>>();
        this.bg = new List<List<int>>();
        this.gd = new List<List<int>>();

        // on ajoute les emplacements
        this.emplacements = new Dictionary<string, HashSet<Point>>();
    }

    public AreaJson(List<List<int>> fg, List<List<int>> bg, List<List<int>> gd, Dictionary<string, HashSet<Vector2>> emplacements)
    {
        // on positionne les tm
        this.fg = fg;
        this.bg = bg;
        this.gd = gd;

        // on ajoute les emplacements
        this.emplacements = new Dictionary<string, HashSet<Point>>();
        foreach (string type in emplacements.Keys)
        {
            this.emplacements.Add(type, new HashSet<Point>());
            foreach (Vector2 pos in emplacements[type])
            {
                this.emplacements[type].Add(new Point(pos));
            }
        }
    }

    public AreaJson(Dictionary<string, List<List<int>>> dict)
    {
        // on positionne les tm
        this.fg = dict["fg"];
        this.bg = dict["bg"];
        this.gd = dict["gd"];

        // on ajoute les emplacements
        this.emplacements = new Dictionary<string, HashSet<Point>>();
    }

    public Dictionary<string, HashSet<Vector2>> GetEmplacements()
    {
        Dictionary<string, HashSet<Vector2>> empl = new Dictionary<string, HashSet<Vector2>>();
        foreach (string type in this.emplacements.Keys)
        {
            empl.Add(type, new HashSet<Vector2>());
            foreach (Point pos in this.emplacements[type])
            {
                empl[type].Add(pos.vec2());
            }
        }
        return empl;
    }

    public Vector2 GetDoorEmplacement(Vector2Int direction)
    {

        string dir = "";
        if (direction == new Vector2Int(1,0)) {dir = "R";}
        else if (direction == new Vector2Int(-1, 0)) { dir = "L"; }
        else if (direction == new Vector2Int(0, 1)) { dir = "U"; }
        else if (direction == new Vector2Int(0, -1)) { dir = "D"; }

        string type = "door" + dir;

        if (!this.emplacements.ContainsKey(type))
        {
            // on retourne la position du centre
            return new Vector2(0, 0);
        }

        // on récupère la position de la porte
        Point door_pos = this.emplacements[type].ElementAt(0);

        // on retourne la position de la porte
        return door_pos.vec2();
    
    }
}

public class Point
{
    public float x;
    public float y;

    public Point()
    {
        this.x = 0;
        this.y = 0;
    }
    
    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public Point(Vector2 xy)
    {
        this.x = xy.x;
        this.y = xy.y;
    }

    public Vector2 vec2()
    {
        return new Vector2(x, y);
    }
}