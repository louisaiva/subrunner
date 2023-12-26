using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public class Sector : MonoBehaviour
{
    [Header("World")]
    public World world;
    private Vector2Int area_size = new Vector2Int(16, 16);


    [Header("HashSet")]
    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();


    [Header("Position")]
    public int x;
    public int y;
    public int w;
    public int h;

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

    [Header("Enemies")]
    [SerializeField] private List<Being> enemies = new List<Being>();
    [SerializeField] private int nb_enemies = 5;
    [SerializeField] private bool is_safe = false;

    // UNITY METHODS
    void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();

        // on récupère les prefabs
        prefabs.Add("light", Resources.Load<GameObject>("prefabs/objects/small_light"));
        prefabs.Add("poster", Resources.Load<GameObject>("prefabs/objects/poster"));
        prefabs.Add("enemy", Resources.Load<GameObject>("prefabs/beings/enemies/zombo"));
        prefabs.Add("chest", Resources.Load<GameObject>("prefabs/objects/chest"));

        // on récupère les parents
        parents.Add("light", transform.Find("decoratives/lights"));
        parents.Add("poster", transform.Find("decoratives/posters"));
        parents.Add("enemy", GameObject.Find("/world/enemies").transform);
        parents.Add("chest", transform.Find("interactives"));

        poster_sprites = Resources.LoadAll<Sprite>("spritesheets/environments/objects/posters");
    }

    void Update()
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

    // INIT
    public void init(HashSet<Vector2Int> rooms, HashSet<Vector2Int> corridors)
    {
        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on récupère les hashsets
        this.rooms = rooms;
        this.corridors = corridors;

        // on calcule la position SW du secteur
        x = tiles.Min(x => x.x);
        y = tiles.Min(x => x.y);

        // on calcule la taille du secteur
        w = tiles.Max(x => x.x) - x + 1;
        h = tiles.Max(x => x.y) - y + 1;

        // on recale les tiles pour que le min soit en 0,0
        recalibrateTiles();

        // on calcule le plafond
        ceiling = CreateCeiling();
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

    private HashSet<Vector2Int> CreateCeiling()
    {
        // on récupère le plafond
        HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();

        // renvoit un hashet qui est l'inverse des tiles, de dimension w*h
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                ceiling.Add(new Vector2Int(i, j));
            }
        }

        // on enlève les tiles
        ceiling.ExceptWith(tiles);

        // on retourne le plafond
        return ceiling;
    }

    // GENERATION
    public void GENERATE(List<Vector2> empl_enemies, List<Vector2> empl_interactives)
    {
        // on récupère les emplacements
        this.empl_enemies = empl_enemies;
        this.empl_interactives = empl_interactives;

        // on génère les objets
        PlaceLights();

        // on génère les posters
        PlacePosters();

        // on place les ennemis
        if (!is_safe) {
            PlaceEnemies();
        }
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
                    offset.y += 3*OFFSET_Y;

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

    // MAIN FUNCTIONS
    public void move(Vector2Int movement)
    {
        x += movement.x;
        y += movement.y;
    }

    
    // COLLISIONS
    public bool isColliding(Sector other)
    {
        // on vérifie si les secteurs collident
        if (other.L() >= R()) { return false; }
        if (other.R() <= L()) { return false; }
        if (other.D() >= U()) { return false; }
        if (other.U() <= D()) { return false; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs collident
        return true;
    }

    public bool collidesWithRoomPoint(Vector2Int roomPoint)
    {
        // on vérifie si les secteurs collident
        if (roomPoint.x >= R()) { return false; }
        if (roomPoint.x < L()) { return false; }
        if (roomPoint.y >= U()) { return false; }
        if (roomPoint.y < D()) { return false; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs collident
        return true;
    }



    // GETTERS
    public Vector2Int getSize()
    {
        // on retourne la taille
        return new Vector2Int(w, h);
    }

    public Vector2Int xy()
    {
        // on retourne la position
        return new Vector2Int(x, y);
    }

    public float cx()
    {
        // on retourne le centre x
        return x + w / 2f;
    }

    public float cy()
    {
        // on retourne le centre y
        return y + h / 2f;
    }

    public float L()
    {
        // on retourne la position la plus à gauche
        return x;
    }

    public float R()
    {
        // on retourne la position la plus à droite
        return x + w;
    }

    public float D()
    {
        // on retourne la position la plus en bas
        return y;
    }

    public float U()
    {
        // on retourne la position la plus en haut
        return y + h;
    }

    public string getAreaType(Vector2Int pos)
    {
        // on vérifie qu'elle est dans les tiles
        if (tiles.Contains(pos))
        {
            // on vérifie si c'est une room
            if (rooms.Contains(pos))
            {
                // on retourne le type de room
                return "room";
            }
            else
            {
                // on retourne le type de corridor
                return "corridor";
            }
        }
        else if (ceiling.Contains(pos))
        {
            // on retourne le type de ceiling
            return "ceiling";
        }
        else
        {
            // on retourne le type de tile
            return "not found";
        }
    }

    public string getAreaName(Vector2Int pos)
    {
        string type = getAreaType(pos);
        if (type == "ceiling") { return "ceiling"; }

        // on récupère les noms des salles
        string name = type + "_";

        // on récupère le voisinage
        HashSet<Vector2Int> room_directions;
        HashSet<Vector2Int> corr_directions;
        getAreaNeighbors(pos, out room_directions, out corr_directions);

        if (type == "room")
        {
            // on concatène les directions
            HashSet<Vector2Int> directions = new HashSet<Vector2Int>();
            directions.UnionWith(room_directions);
            directions.UnionWith(corr_directions);

            // on parcourt les directions des rooms puis on ajoute le nom de la direction
            if (directions.Contains(Vector2Int.up)) { name += "U"; }
            if (directions.Contains(Vector2Int.down)) { name += "D"; }
            if (directions.Contains(Vector2Int.left)) { name += "L"; }
            if (directions.Contains(Vector2Int.right)) { name += "R"; }
        }
        else if (type == "corridor")
        {
            // on concatène les directions
            HashSet<Vector2Int> directions = new HashSet<Vector2Int>();
            directions.UnionWith(room_directions);
            directions.UnionWith(corr_directions);

            // on parcourt les directions des rooms puis on ajoute le nom de la direction
            if (directions.Contains(Vector2Int.up)) { name += "U"; }
            if (directions.Contains(Vector2Int.down)) { name += "D"; }
            if (directions.Contains(Vector2Int.left)) { name += "L"; }
            if (directions.Contains(Vector2Int.right)) { name += "R"; }
        }

        // on retourne le nom
        return name;
    }

    public void getAreaNeighbors(Vector2Int pos, out HashSet<Vector2Int> room_directions, out HashSet<Vector2Int> corr_directions)
    {
        // on crée un hashset d'areas similaires adjacentes
        room_directions = new HashSet<Vector2Int>();
        corr_directions = new HashSet<Vector2Int>();

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on parcourt les directions
        foreach (var direction in directions)
        {
            // on récupère la position adjacente
            Vector2Int adjacentPosition = pos + direction;

            // on vérifie que la position adjacente est une salle
            if (rooms.Contains(adjacentPosition))
            {
                // on ajoute la direction de la room
                room_directions.Add(direction);
            }
            else if (corridors.Contains(adjacentPosition))
            {
                // on ajoute la direction du corridor
                corr_directions.Add(direction);
            }
        }

    }

    public BoundsInt getBounds()
    {
        // on retourne les bounds
        return new BoundsInt(x * area_size.x, y * area_size.y, 0, w * area_size.x, h * area_size.y, 1);
    }
}