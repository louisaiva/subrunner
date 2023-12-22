using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;

public class TileAreaBuilder : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private string area_to_load = "...";

    [Header("Saving")]
    [SerializeField] private string rooms_path = "prefabs/rooms/";
    [SerializeField] private string areas_path = "prefabs/areas/";
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

    public void ApplyChanges(ref Tilemap tm)
    {
        // ici on applique différentes modifications aux tilemaps
        // tout d'abord on veut remonter les tiles de 1 unité

        // on récupère les dimensions de l'area
        GetAreaDimensions(new List<Tilemap> { tm }, out Vector2Int position, out Vector2Int size);

        // on crée une liste de tiles à remonter
        List<Vector2Int> tiles_to_move = new List<Vector2Int>();

        // on parcourt les tiles
        for (int y = position.y; y < position.y + size.y; y++)
        {
            for (int x = position.x; x < position.x + size.x; x++)
            {
                // on récupère les tiles
                TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));

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
            tm.SetTile(new Vector3Int(tile.x, tile.y, 0), null);
            tm.SetTile(new Vector3Int(tile.x, tile.y + 1, 0), Resources.Load<TileBase>("tilesets/walls_1_rule"));
        }
    }

    // SAVING SELECTED AREAS

    public void SaveJson(GameObject area)
    {
        
        // * format de sauvegarde
        /*
            fichier json contenant

            "fg" : [...],
            "bg" : [...],
            "gd" : [...]

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
        ApplyChanges(ref bgTilemap);

        // on récupère les dimensions de l'area
        GetAreaDimensions(new List<Tilemap> { fgTilemap, bgTilemap, gdTilemap }, out Vector2Int position, out Vector2Int size);
        string json_fg = "\"fg\" : [\n";
        string json_bg = "\"bg\" : [\n";
        string json_gd = "\"gd\" : [\n";

        // on parcourt les tiles
        for (int y = position.y; y < position.y + size.y; y++)
        {
            // on ajoute une ligne à chaque liste
            fg.Add(new List<int>());
            bg.Add(new List<int>());
            gd.Add(new List<int>());

            // on ajoute une ligne à chaque string
            json_fg += "[";
            json_bg += "[";
            json_gd += "[";

            for (int x = position.x; x < position.x + size.x; x++)
            {
                // on récupère les tiles
                TileBase fgTile = fgTilemap.GetTile(new Vector3Int(x, y, 0));
                TileBase bgTile = bgTilemap.GetTile(new Vector3Int(x, y, 0));
                TileBase gdTile = gdTilemap.GetTile(new Vector3Int(x, y, 0));

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
        Dictionary<string, List<List<int>>> dict = new Dictionary<string, List<List<int>>>();
        dict.Add("fg", fg);
        dict.Add("bg", bg);
        dict.Add("gd", gd);

        // on sauvegarde les listes dans un fichier json
        string json = "{\n\n" + json_fg + ",\n\n" + json_bg + ",\n\n" + json_gd + "\n\n}";
        // string json = JsonConvert.SerializeObject(dict);
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
}