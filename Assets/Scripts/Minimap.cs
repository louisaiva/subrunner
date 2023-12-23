using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Minimap : MonoBehaviour {
    public Transform player;
    private WorldGenerator generator;
    private World world;
    
    [Header("Map Settings")]
    // [SerializeField] private float mapScale = 0.2f;
    public bool is_init = false;
    [SerializeField] private bool is_discovering = true;
    [SerializeField] private float discorveryRadius = 1f;


    [Header("Map data")]
    [SerializeField] private Vector2Int mapOffset;
    // [SerializeField] private Vector2Int mapSize;
    public Texture2D mapTexture;
    // [SerializeField] private Dictionary<Vector2Int, string> tiles = new Dictionary<Vector2Int, string>();
    // [SerializeField] private List<Vector2Int> discoveredTiles = new List<Vector2Int>();

    [Header("Tilemaps")]
    [SerializeField] private Tilemap fg_tm;
    [SerializeField] private Tilemap bg_tm;
    [SerializeField] private Tilemap gd_tm;


    [Header("show")]
    [SerializeField] private Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
    [SerializeField] private Color undiscoveredColor = Color.black;

    void Awake()
    {
        // on vide le dictionnaire
        colorMap.Clear();

        // on met les couleurs dans le dictionnaire
        colorMap.Add("wall", Color.white);
        colorMap.Add("ground", Color.grey);
        colorMap.Add("ceiling", Color.black);
        colorMap.Add("not found", Color.red);

        // on récupère le perso
        player = GameObject.Find("/perso").transform;

        // on récupère le world generator
        generator = GameObject.Find("/generator").GetComponent<WorldGenerator>();

        // on récupère le world et les tilemaps
        world = GameObject.Find("/world").GetComponent<World>();
        world.GetTilemaps(out fg_tm, out bg_tm, out gd_tm);
    }

    /* public void init(HashSet<Vector2Int> tiles)
    {
        Awake();

        // on récupère la taille de la map
        int min_x = 100000;
        int min_y = 100000;
        int max_x = -100000;
        int max_y = -100000;

        // on parcourt les tiles
        foreach (Vector2Int tile in tiles)
        {
            // on récupère les coordonnées
            int x = tile.x;
            int y = tile.y;

            // on met à jour les min et max
            if (x < min_x) min_x = x;
            if (y < min_y) min_y = y;
            if (x > max_x) max_x = x;
            if (y > max_y) max_y = y;


            // on récupère le type de tile
            string type = generator.getTileType(tile);

            // print("tile " + tile + " is " + type);

            // on ajoute la tile au dictionnaire
            this.tiles.Add(tile, type);
        }

        // on calcule l'offset
        mapOffset = new Vector2Int(min_x, min_y);

        // on calcule la taille de la map
        mapSize = new Vector2Int(max_x - min_x + 1, max_y - min_y + 1);

        // on crée la texture
        mapTexture = new Texture2D(mapSize.x, mapSize.y);

        // on met la couleur de fond
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                mapTexture.SetPixel(x, y, undiscoveredColor);
            }
        }

        // on met les couleurs des tiles
        foreach (KeyValuePair<Vector2Int, string> tile in this.tiles)
        {
            // on récupère les coordonnées
            int x = tile.Key.x - mapOffset.x;
            int y = tile.Key.y - mapOffset.y;

            // on récupère le type de tile
            string type = tile.Value;
            print("tile " + tile.Key + " is " + colorMap[type]);

            // on met la couleur
            mapTexture.SetPixel(x, y, colorMap[type]);
        }

        // on change le mode de filtre
        mapTexture.filterMode = FilterMode.Point;


        // on applique les changements
        is_init = true;
    } */

    public void createTexture()
    {
        // on récupère les dimensions de la map
        Vector2Int mapSize = world.GetSize();

        // on crée la texture
        mapTexture = new Texture2D(mapSize.x, mapSize.y);

        // on met la couleur de fond
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                mapTexture.SetPixel(x, y, undiscoveredColor);
            }
        }
    }

    public string getPersoAreaName()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récupère le nom de la zone
        return world.getAreaName(pos);
    }

    public string getPersoTilePos()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récup la position locale
        Vector2Int local_pos = world.getLocalTilePos(pos);

        return "(" + local_pos.x + ", " + local_pos.y + ")";
    }

    public string getPersoAreaPos()
    {
        // on récupère la position du perso
        Vector2Int pos = getPersoPos();

        // on récupère la position de l'area
        Vector2Int area_pos = world.getAreaPos(pos);

        return "(" + area_pos.x + ", " + area_pos.y + ")";
    }

    public Vector2Int getPersoPos()
    {
        // on récupère la position du perso
        Vector2Int pos = new Vector2Int((int)(player.position.x * 2), (int)(player.position.y * 2));

        return pos;
    }

}