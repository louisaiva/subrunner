using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ProceduralSector : Sector
{


    [Header("Objects")]
    [SerializeField] private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    [SerializeField] private Dictionary<string, Transform> parents = new Dictionary<string, Transform>();

    [Header("Lights")]
    [SerializeField] private List<string> walls_light_tiles = new List<string> { "bg_2_7", "walls_1_7", "walls_2_7" };
    [SerializeField] private Vector3 light_offset = new Vector3(0.25f, 1f, 0f);

    [Header("Posters")]
    [SerializeField] private Sprite[] poster_sprites;
    private List<string> full_walls_tiles = new List<string> {"walls_1_3", "walls_1_4", "walls_1_5", "walls_1_6"
                                                        , "walls_1_7", "walls_1_8",
                                                        "walls_2_3", "walls_2_4", "walls_2_5", "walls_2_6"
                                                        , "walls_2_7", "walls_2_8"};
    [SerializeField] private float density_posters = 0.3f; // 0.5 = 1 poster tous les 2 tiles compatibles

    [Header("Emplacements")]
    [SerializeField] private List<Vector2> empl_enemies = new List<Vector2>();
    [SerializeField] private List<Vector2> empl_interactives = new List<Vector2>();
    [SerializeField] private Dictionary<string, Vector2> empl_doors = new Dictionary<string, Vector2>();

    [Header("Enemies")]
    [SerializeField] private List<Being> enemies = new List<Being>();
    [SerializeField] private int nb_enemies = 5;
    public bool is_safe = false;

    [Header("Interactives")]
    [SerializeField] private int nb_chests = 5;
    [SerializeField] private int nb_comp = 3;

    [Header("Doors")]
    [SerializeField] private Dictionary<Vector2, Vector2Int> doors = new Dictionary<Vector2, Vector2Int>();

    // unity functions
    new void Awake()
    {
        base.Awake();

        // on récupère les prefabs
        prefabs.Add("light", Resources.Load<GameObject>("prefabs/objects/lights/small_light"));
        prefabs.Add("poster", Resources.Load<GameObject>("prefabs/objects/poster"));
        prefabs.Add("enemy", Resources.Load<GameObject>("prefabs/beings/enemies/zombo"));
        prefabs.Add("chest", Resources.Load<GameObject>("prefabs/objects/chest"));
        prefabs.Add("xp_chest", Resources.Load<GameObject>("prefabs/objects/xp_chest"));
        prefabs.Add("computer", Resources.Load<GameObject>("prefabs/objects/computer"));
        prefabs.Add("doorUD", Resources.Load<GameObject>("prefabs/objects/door_hackbl"));
        prefabs.Add("doorLR", Resources.Load<GameObject>("prefabs/objects/door_L"));

        // on récupère les parents
        parents.Add("light", transform.Find("decoratives/lights"));
        parents.Add("poster", transform.Find("decoratives/posters"));
        parents.Add("enemy", GameObject.Find("/world/enemies").transform);
        parents.Add("chest", transform.Find("interactives"));
        parents.Add("computer", transform.Find("interactives"));
        parents.Add("doors", transform.Find("interactives"));

        poster_sprites = Resources.LoadAll<Sprite>("spritesheets/environments/objects/posters");
    }

    void Update()
    {
        if (!world_generator.generate_world) { return; }

        if (!is_safe)
        {
            // on génère des zombO TANT QUE le nombre d'ennemis est inférieur à nb_enemies
            if (enemies.Count < nb_enemies)
            {
                PlaceEnemy();
            }

            // on vérifie si les ennemis sont tous morts
            if (enemies.Count > 0)
            {
                // on récupère les ennemis morts
                List<Being> dead_enemies = enemies.Where(x => !x.isAlive()).ToList();

                // on les supprime de la liste
                foreach (Being enemy in dead_enemies)
                {
                    enemies.Remove(enemy);
                }
            }
        }

    }


    // INIT
    public void init(HashSet<Vector2Int> rooms, HashSet<Vector2Int> corridors)
    {

        // on récupère les hashsets
        this.rooms = rooms;
        this.corridors = corridors;

        // on calcule le nombre de zombies basé sur le nombre de salles dans room
        nb_enemies = rooms.Count;

        base.init();

        // on recale les tiles pour que le min soit en 0,0
        recalibrateTiles();
    }

    private void recalibrateTiles()
    {
        // ROOMS

        // on crée un hashset temporaire
        HashSet<Vector2Int> temp = new HashSet<Vector2Int>();

        // on parcourt les rooms
        foreach (Vector2Int room in rooms)
        {
            // on ajoute le room recalibré
            temp.Add(new Vector2Int(room.x - x, room.y - y));
        }

        // on remplace les rooms
        rooms = temp;

        // CORRIDORS

        // on crée un hashset temporaire
        temp = new HashSet<Vector2Int>();

        // on parcourt les corridors
        foreach (Vector2Int corr in corridors)
        {
            // on ajoute le corr recalibré
            temp.Add(new Vector2Int(corr.x - x, corr.y - y));
        }

        // on remplace les corridors
        corridors = temp;

        // TILES
        tiles = new HashSet<Vector2Int>();
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

    }

    // GENERATION
    public void GENERATE(List<Vector2> empl_enemies, List<Vector2> empl_interactives, Dictionary<Vector2, Vector2Int> empl_doors)
    {
        // on récupère les emplacements
        this.empl_enemies = empl_enemies;
        this.empl_interactives = empl_interactives;

        // on génère les objets
        PlaceLights();

        // on génère les posters
        PlacePosters();

        // on place les ennemis
        if (!is_safe)
        {
            PlaceEnemies();
        }

        // on place les coffres
        for (int i = 0; i < nb_chests; i++)
        {
            PlaceChest();
        }

        // on place les ordinateurs
        for (int i = 0; i < nb_comp; i++)
        {
            PlaceComputer();
        }

        this.doors = empl_doors;
        PlaceDoors();
    }

    // OBJETS GENERATION
    private void PlaceLights()
    {
        BoundsInt bounds = getBounds();
        TileBase[] tiles = world.getTiles(bounds);

        for (int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                int x_ = bounds.x + i;
                int y_ = bounds.y + j;

                // on récupère le sprite
                Sprite sprite = world.GetSprite(x_, y_);
                if (sprite == null) { continue; }

                if (walls_light_tiles.Contains(sprite.name))
                {
                    // on récupère la position globale de la tile
                    Vector3 tile_pos = world.CellToWorld(new Vector3Int(x_, y_, 0));

                    // on met une lumiere
                    GameObject light = Instantiate(prefabs["light"], tile_pos + light_offset, Quaternion.identity);

                    // on met le bon parent
                    light.transform.SetParent(parents["light"]);
                }
            }
        }
    }

    private void PlacePosters()
    {

        BoundsInt bounds = getBounds();
        TileBase[] tiles = world.getTiles(bounds);

        for (int i = 0; i < bounds.size.x; i++)
        {
            for (int j = 0; j < bounds.size.y; j++)
            {
                // on récupère la position globale de la tile
                int x_ = bounds.x + i;
                int y_ = bounds.y + j;

                // on récupère le sprite
                Sprite sprite = world.GetSprite(x_, y_);
                if (sprite == null) { continue; }

                if (full_walls_tiles.Contains(sprite.name))
                {
                    // on lance un random pour savoir si on met un poster
                    if (Random.Range(0f, 1f) > density_posters) { continue; }

                    // on instancie un poster aleatoire dans une premiere position
                    Vector3 tile_pos = world.CellToWorld(new Vector3Int(x_, y_, 0));
                    GameObject poster = Instantiate(prefabs["poster"], tile_pos, Quaternion.identity);
                    poster.GetComponent<SpriteRenderer>().sprite = poster_sprites[Random.Range(0, poster_sprites.Length)];


                    // on calcule un offset plus ou moins aléatoire
                    float OFFSET_Y = 0.2f;
                    float OFFSET_RANGE_Y = 0.3f;
                    float OFFSET_X = -poster.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                    float OFFSET_RANGE_X = 0f;

                    Vector3 offset = Vector3.zero;
                    // offset += new Vector3(OFFSET_X, OFFSET_Y, 0f);
                    // offset += new Vector3(Random.Range(-OFFSET_RANGE_X, OFFSET_RANGE_X), OFFSET_Y + Random.Range(0f, OFFSET_RANGE_Y), 0f);
                    // offset.x += OFFSET_X;
                    offset.y += 3 * OFFSET_Y;

                    // on déplace le poster
                    poster.transform.position += offset;

                    // on met le bon parent
                    poster.transform.SetParent(parents["poster"]);

                    // on relance un random pour savoir si on remet un poster
                    if (Random.Range(0f, 1f) < 0.1f)
                    {
                        i--;
                    }
                }
            }
        }
    }

    private void PlaceEnemies()
    {
        for (int i = 0; i < nb_enemies; i++)
        {
            PlaceEnemy();
        }
    }

    private void PlaceEnemy()
    {
        // on récupère un emplacement
        Vector2 empl = empl_enemies[Random.Range(0, empl_enemies.Count)];

        // on instancie un enemy
        // Vector3 pos = world.CellToWorld(new Vector3Int(empl.x, empl.y, 0));
        Vector3 pos = new Vector3(empl.x, empl.y, 0);
        GameObject enemy = Instantiate(prefabs["enemy"], pos, Quaternion.identity);

        // on met le bon parent
        enemy.transform.SetParent(parents["enemy"]);

        // on ajoute l'enemy à la liste
        enemies.Add(enemy.GetComponent<Being>());
    }

    private void PlaceChest()
    {
        // on récupère un emplacement
        Vector2 empl = consumeRandomEmplacement("interactives");

        // on instancie un coffre
        Vector3 pos = new Vector3(empl.x, empl.y, 0);

        // on choisit un coffre random
        GameObject chest = null;
        if (Random.Range(0f, 1f) < .5f)
        {
            chest = Instantiate(prefabs["chest"], pos, Quaternion.identity);
        }
        else
        {
            chest = Instantiate(prefabs["xp_chest"], pos, Quaternion.identity);
        }

        // on met le bon parent
        chest.transform.SetParent(parents["chest"]);
    }
    private void PlaceComputer()
    {
        // on récupère un emplacement
        Vector2 empl = consumeRandomEmplacement("interactives");

        // on instancie un computer
        Vector3 pos = new Vector3(empl.x, empl.y, 0);
        GameObject computer = Instantiate(prefabs["computer"], pos, Quaternion.identity);

        // on met le bon parent
        computer.transform.SetParent(parents["computer"]);
    }

    private void PlaceDoors()
    {
        int i = 0;

        // on parcourt les connections
        foreach (KeyValuePair<Vector2, Vector2Int> empl in doors)
        {

            // on instancie une porte
            Vector3 pos = new Vector3(empl.Key.x, empl.Key.y, 0);
            GameObject door = null;


            if (new Vector2Int[] { Vector2Int.up, Vector2Int.down }.Contains(empl.Value))
            {
                print("instantiate a UD door at " + empl.Key);
                // on instancie une porte verticale (up ou down)
                door = Instantiate(prefabs["doorUD"], pos, Quaternion.identity);
            }
            else if (new Vector2Int[] { Vector2Int.left, Vector2Int.right }.Contains(empl.Value))
            {
                print("instantiate a LR door at " + empl.Key);
                // on instancie une porte horizontale (left ou right
                door = Instantiate(prefabs["doorLR"], pos, Quaternion.identity);
            }

            // on met le bon parent
            door.transform.SetParent(parents["doors"]);

            i++;
        }
    }



    // EMPLACEMENTS
    private Vector2 consumeRandomEmplacement(string type)
    {

        // on récupère la liste d'emplacements du type
        List<Vector2> empl = new List<Vector2>();
        if (type == "enemies") { empl = empl_enemies; }
        else if (type == "interactives") { empl = empl_interactives; }

        // on vérifie qu'il reste des emplacements
        if (empl.Count == 0)
        {
            Debug.LogError("Il n'y a plus d'emplacement de type " + type + " dans le secteur " + x + "_" + y);
            return new Vector2(20000, 0);
        }

        // on récupère un emplacement aléatoire
        Vector2 emplacement = empl[Random.Range(0, empl.Count)];

        // on le supprime de la liste
        empl.Remove(emplacement);

        // on retourne l'emplacement
        return emplacement;
    }


}