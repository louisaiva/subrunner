using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Minimap : MonoBehaviour {
    public Transform player;
    private WorldGenerator generator;
    private World world;

    [Header("Map Settings")]
    public bool is_init = false;
    [SerializeField] private bool is_discovering = true;
    [SerializeField] private float discorveryRadius = 1f;


    [Header("Map data")]
    public Texture2D mapTexture;
    public Texture2D maskTexture;

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
        colorMap.Add("wall", Color.grey);
        colorMap.Add("ground", Color.grey);
        colorMap.Add("ceiling", Color.black);
        colorMap.Add("handmade", Color.black);
        colorMap.Add("not found", Color.red);
        colorMap.Add("void", new Color(1f, 0f, 1f, 0f));

        // on récupère le perso
        player = GameObject.Find("/perso").transform;

        // on récupère le world generator
        generator = GameObject.Find("/generator").GetComponent<WorldGenerator>();

        // on récupère le world et les tilemaps
        world = GameObject.Find("/world").GetComponent<World>();
        world.GetTilemaps(out fg_tm, out bg_tm, out gd_tm);


        // on crée la texture
        createTextures();
    }

    public void createTextures()
    {
        print("(Minimap) creating textures");

        // on récupère les dimensions de la map
        Vector2Int mapSize = world.GetSize();

        // on crée les textures
        mapTexture = new Texture2D(mapSize.x, mapSize.y);
        maskTexture = new Texture2D(mapSize.x, mapSize.y);

        // on remplit les textures
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                // mask
                maskTexture.SetPixel(x, y, undiscoveredColor);

                // map
                // on récupère le type de tile
                string type = world.getTileType(new Vector2Int(x, y));

                if (type == "void") print("(Minimap) void detected at " + x + ", " + y);

                // on met la couleur
                mapTexture.SetPixel(x, y, colorMap[type]);
            }
        }

        // on change le mode de filtre
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        maskTexture.filterMode = FilterMode.Point;
        maskTexture.Apply();

        // on applique les changements
        is_init = true;
    }

    public void Clear()
    {
        if (!is_init) return;

        createTextures();

        GameObject.Find("/ui/minimap/mask/map").GetComponent<RawImage>().texture = null;
        GameObject.Find("/ui/fullmap/mask/map").GetComponent<RawImage>().texture = null;
    }

    // Update is called once per frame
    void Update()
    {
        // si on a pas généré le monde, on return
        if (!generator.generate_world) return;

        if (is_discovering && player.GetComponent<Perso>().hasCapacity("gyroscope"))
        {
            // on récupère la position du perso
            Vector2Int pos = world.getPersoPos();

            // on parcourt toutes les tiles dans le rayon de découverte
            for (int x = pos.x - (int)discorveryRadius; x <= pos.x + (int)discorveryRadius; x++)
            {
                for (int y = pos.y - (int)discorveryRadius; y <= pos.y + (int)discorveryRadius; y++)
                {
                    // on vérifie qu'on est positifs
                    if (x < 0 || y < 0) continue;

                    // on vérifie si on est dans le rayon
                    if (Vector2.Distance(pos, new Vector2(x, y)) <= discorveryRadius)
                    {
                        // on met la couleur
                        maskTexture.SetPixel(x, y, colorMap["void"]);
                    }
                }
            }
            maskTexture.Apply();
        }
    }


}