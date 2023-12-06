using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {

    // cette classe sert à créer des salles
    // elle prend en paramètre les dimensions de la salle
    // et elle remplit les tilemaps avec des tiles
    // puis génère des lights etc apropriées
    // puis génère les portes
    // puis embellit le tout avec des vieux posters, etc

    // tilemaps
    public GameObject fg;
    public GameObject bg;
    public GameObject gd;

    // dimensions
    public int width;
    public int height;
    public int x;
    public int y;

    // light tiles
    private List<string> walls_light_tiles = new List<string> {"bg_2_7","walls_1_7","walls_2_7"};
    public GameObject light_prefab;
    public Transform light_parent;
    private Vector3 light_offset = new Vector3(0.25f, 1f, 0f);

    // posters tiles
    private List<string> walls_tiles = new List<string> {"walls_1_2", "walls_1_3", "walls_1_4", "walls_1_5", "walls_1_6"
                                                        , "walls_1_7", "walls_1_8", "walls_1_9",
                                                        "walls_2_2", "walls_2_3", "walls_2_4", "walls_2_5", "walls_2_6"
                                                        , "walls_2_7", "walls_2_8", "walls_2_9" };
    // [SerializeField]
    private float density_posters = 0.3f; // 0.5 = 1 poster tous les 2 tiles compatibles
    // [SerializeField]
    private Vector2 max_offset_posters = new Vector2(0.25f, 0.25f);
    public GameObject poster_prefab;
    public Sprite[] poster_sprites;

    // tags
    // [SerializeField]
    private float density_tags = 0.5f;  // 1 = 1 tag tous les 2 tiles compatibles (un tag prend 2 tiles donc c normal)
    // 0.5 = 1 tag tous les 4 tiles compatibles

    // doors
    public Transform doors_parent;
    public GameObject door_prefab;

    // objects
    public Transform objects_parent;

    // unity functions
    void Start()
    {
        // on récupère les prefabs
        light_prefab = Resources.Load<GameObject>("prefabs/objects/small_light");
        door_prefab = Resources.Load<GameObject>("prefabs/objects/door");
        poster_prefab = Resources.Load<GameObject>("prefabs/objects/poster");
        poster_sprites = Resources.LoadAll<Sprite>("spritesheets/environments/objects/posters");

        // on récupère les parents
        light_parent = transform.Find("lights");
        objects_parent = transform.Find("objects");
        doors_parent = transform.Find("doors");

        // on récupère les tilemaps
        fg = transform.Find("fg_tilemap").gameObject;
        bg = transform.Find("bg_tilemap").gameObject;
        gd = transform.Find("gd_tilemap").gameObject;

        // on cherche les bonnes dimensions
        width = 0;
        height = 0;
        x = 50000;
        y = 50000;

        // on parcourt les 3 tilemaps

        // fg
        fg.GetComponent<Tilemap>().CompressBounds();
        if (fg.GetComponent<Tilemap>().size.x > width) { width = (int) fg.GetComponent<Tilemap>().size.x; }
        if (fg.GetComponent<Tilemap>().size.y > height) { height = (int) fg.GetComponent<Tilemap>().size.y; }
        if ((int) fg.GetComponent<Tilemap>().cellBounds.xMin < x) { x = (int) fg.GetComponent<Tilemap>().cellBounds.xMin; }
        if ((int) fg.GetComponent<Tilemap>().cellBounds.yMin < y) { y = (int) fg.GetComponent<Tilemap>().cellBounds.yMin; }

        // gd
        gd.GetComponent<Tilemap>().CompressBounds();
        if (gd.GetComponent<Tilemap>().size.x > width) { width = (int) gd.GetComponent<Tilemap>().size.x; }
        if (gd.GetComponent<Tilemap>().size.y > height) { height = (int) gd.GetComponent<Tilemap>().size.y; }
        if ((int) gd.GetComponent<Tilemap>().cellBounds.xMin < x) { x = (int) gd.GetComponent<Tilemap>().cellBounds.xMin; }
        if ((int) gd.GetComponent<Tilemap>().cellBounds.yMin < y) { y = (int) gd.GetComponent<Tilemap>().cellBounds.yMin; }

        // bg
        bg.GetComponent<Tilemap>().CompressBounds();
        if (bg.GetComponent<Tilemap>().size.x > width) { width = (int) bg.GetComponent<Tilemap>().size.x; }
        if (bg.GetComponent<Tilemap>().size.y > height) { height = (int) bg.GetComponent<Tilemap>().size.y; }
        if ((int) bg.GetComponent<Tilemap>().cellBounds.xMin < x) { x = (int) bg.GetComponent<Tilemap>().cellBounds.xMin; }
        if ((int) bg.GetComponent<Tilemap>().cellBounds.yMin < y) { y = (int) bg.GetComponent<Tilemap>().cellBounds.yMin; }

        print(gameObject.name + " : tilemap dimensions : " + width + " " + height + " " + x + " " + y);

        // on initialise la salle
        init();

    }

    // functions

    public void init()
    {
        // on met des lumieres sur les murs
        // là où les tiles correspondent à tile_bg_light
        place_lights();

        // on met des posters sur les murs
        // là où les tiles correspondent à tile_bg_poster
        place_posters();
    }

    private void place_lights()
    {
        // on parcourt les tiles de bg_tilemap
        Tilemap bg_tilemap = bg.GetComponent<Tilemap>();
        for (int j = x; j < x + width; j++)
        {
            for (int i = y; i < y + height; i++)
            {
                // position de la tile
                Vector3Int pos = new Vector3Int(j, i, 0);

                // on récupère la tile
                TileBase tile = bg_tilemap.GetTile(pos);
                if (tile == null) { continue; }

                // on regarde si c'est une tile de mur
                TileData tile_data = new TileData();
                tile.GetTileData(pos, bg_tilemap, ref tile_data);

                // on regarde si c une tile de lumiere
                if (walls_light_tiles.Contains(tile_data.sprite.name))
                {

                    // on récupère la position globale de la tile
                    Vector3 tile_pos = bg_tilemap.CellToWorld(pos);

                    // on met une lumiere
                    GameObject light = Instantiate(light_prefab, tile_pos + light_offset, Quaternion.identity);

                    // on met le bon parent
                    light.transform.SetParent(light_parent);
                }
            }
        }
    }

    private void place_posters()
    {
        // on parcourt les tiles de bg_tilemap
        Tilemap bg_tilemap = bg.GetComponent<Tilemap>();
        for (int j = x; j < x + width; j++)
        {
            for (int i = y; i < y + height; i++)
            {
                // position de la tile
                Vector3Int pos = new Vector3Int(j, i, 0);

                // on récupère la tile
                TileBase tile = bg_tilemap.GetTile(pos);
                if (tile == null) { continue; }

                // on regarde si c'est une tile de mur
                TileData tile_data = new TileData();
                tile.GetTileData(pos, bg_tilemap, ref tile_data);

                // on regarde si c une tile de lumiere
                if (walls_tiles.Contains(tile_data.sprite.name))
                {

                    // on lance un random pour savoir si on met un poster
                    if (Random.Range(0f, 1f) > density_posters) { continue; }

                    // on calcule un offset véritable qui advient à chaque fois
                    float OFFSET_Y = 0.75f;

                    // on calcule un offset aléatoire
                    Vector2 offset = new Vector2(Random.Range(-max_offset_posters.x, max_offset_posters.x), OFFSET_Y + Random.Range(0f, max_offset_posters.y));

                    // on récupère la position globale de la tile
                    Vector3 tile_pos = bg_tilemap.CellToWorld(pos);
                    
                    // on instancie le poster
                    GameObject poster = Instantiate(poster_prefab, tile_pos + (Vector3) offset, Quaternion.identity);

                    // on choisit un sprite au hasard
                    poster.GetComponent<SpriteRenderer>().sprite = poster_sprites[Random.Range(0, poster_sprites.Length)];

                    // on met le bon parent
                    poster.transform.SetParent(objects_parent);

                    // on relance un random pour savoir si on remet un poster
                    if (Random.Range(0f, 1f) < density_posters*density_posters) {
                        i--;                        
                    }
                }
            }
        }
    }

}