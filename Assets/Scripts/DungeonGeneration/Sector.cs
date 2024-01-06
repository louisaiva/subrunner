using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public class Sector : MonoBehaviour
{
    [Header("World")]
    public World world;
    private WorldGenerator world_generator;
    private Vector2Int area_size = new Vector2Int(16, 16);


    [Header("HashSet")]
    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> sas = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
    // public HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();

    [Header("Connections")]
    public Dictionary<Vector2Int, List<Vector2Int>> connections = new Dictionary<Vector2Int, List<Vector2Int>>();

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
    [SerializeField] private Dictionary<string,Vector2> empl_doors = new Dictionary<string, Vector2>();

    [Header("Enemies")]
    [SerializeField] private List<Being> enemies = new List<Being>();
    [SerializeField] private int nb_enemies = 5;
    public bool is_safe = false;

    [Header("Interactives")]
    [SerializeField] private int nb_chests = 5;
    [SerializeField] private int nb_comp = 3;

    [Header("Doors")]
    [SerializeField] private Dictionary<Vector2, Vector2Int> doors = new Dictionary<Vector2, Vector2Int>();

    // UNITY METHODS
    void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();
        world_generator = GameObject.Find("/generator").GetComponent<WorldGenerator>();

        // on récupère les prefabs
        prefabs.Add("light", Resources.Load<GameObject>("prefabs/objects/small_light"));
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

        // on ajoute une tile de marge
        w += 1;
        h += 1;

        // on recale les tiles pour que le min soit en 0,0
        recalibrateTiles();

        // on calcule le plafond
        // ceiling = CreateCeiling();

        // on calcule le nombre de zombies basé sur le nombre de salles dans room
        nb_enemies = rooms.Count;
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
        if (!is_safe) {
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

    private void PlaceChest()
    {
        // on récupère un emplacement
        Vector2 empl = consumeRandomEmplacement("interactives");

        // on instancie un coffre
        Vector3 pos = new Vector3(empl.x, empl.y, 0);

        // on choisit un coffre random
        GameObject chest = null;
        if (Random.Range(0f, 1f) < 0.5f)
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
        int i=0;

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

    // EMPLACEMENTS
    private Vector2 consumeRandomEmplacement(string type)
    {

        // on récupère la liste d'emplacements du type
        List<Vector2> empl = new List<Vector2>();
        if (type == "enemies") { empl = empl_enemies; }
        else if (type == "interactives") { empl = empl_interactives; }

        // on vérifie qu'il reste des emplacements
        if (empl.Count == 0) {
            Debug.LogError("Il n'y a plus d'emplacement de type "+type+" dans le secteur "+x+"_"+y);
            return new Vector2(20000, 0);
        }

        // on récupère un emplacement aléatoire
        Vector2 emplacement = empl[Random.Range(0, empl.Count)];

        // on le supprime de la liste
        empl.Remove(emplacement);

        // on retourne l'emplacement
        return emplacement;
    }

    // CONNECTIONS
    public void connectWithSector(Sector other)
    {
        // créé le plus petit chemin entre les deux secteurs

        // trouve l'area la plus proche de l'autre secteur
        Vector2Int closest_area = new Vector2Int(0, 0);

        // on trouve la frontière entre les secteurs
        string border = getBorder(other);

        // on vérifie si les secteurs sont collés
        if (border == "R")
        {
            // on trouve l'area la plus proche de l'autre secteur par la droite
            float min_dist = 100000;
            for (int y_area = D(); y_area < U(); y_area++)
            {
                Vector2Int tile = new Vector2Int(R()-2, y_area);

                // on vérifie que la tile est dans le secteur
                if (!tiles.Contains(new Vector2Int(tile.x - x, tile.y - y))) { continue; }

                // on calcule la distance
                float dist = Vector2.Distance(tile, other.center());

                // on vérifie si c'est la plus petite distance
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_area = tile;
                }
            }
        }
        else if (border == "U")
        {
            // on trouve l'area la plus proche de l'autre secteur par le haut
            float min_dist = 100000;
            for (int x_area = L(); x_area < R(); x_area++)
            {
                Vector2Int tile = new Vector2Int(x_area, U()-2);

                // on vérifie que la tile est dans le secteur
                if (!tiles.Contains(new Vector2Int(tile.x - x, tile.y - y))) { continue; }

                // on calcule la distance
                float dist = Vector2.Distance(tile, other.center());

                // on vérifie si c'est la plus petite distance
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_area = tile;
                }
            }
        }
        else if (border == "collision")
        {
            Debug.LogError("Les secteurs collisionnent");
            return;
        }
        else if (border == "no border")
        {
            Debug.LogError("Les secteurs ne sont pas voisins");
            return;
        }
        else
        {
            Debug.LogError("Erreur dans la recherche de la frontière entre les secteurs");
            return;
        }


        // on récupère la tile de l'autre secteur la plus proche de l'area
        Vector2Int closest_area_other = other.findClosestArea(closest_area);

        print(gameObject.name + " : " + closest_area + " / " + other.gameObject.name + " : " + closest_area_other);

        // on choisit un sas dans notre secteur qui est sur la border
        Vector2Int sas = closest_area;
        Vector2Int other_sas = closest_area_other;
        if (border == "R")
        {

            // on cherche les dimensions de la frontière partagée
            int border_D = Mathf.Max(D(), other.D());
            int border_U = Mathf.Min(U(), other.U());

            // on choisit un sas random dans la frontière
            sas = new Vector2Int(R()-1, Random.Range(border_D, border_U));

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(other.L(), sas.y);
        }
        else if (border == "U")
        {
            // on cherche les dimensions de la frontière partagée
            int border_L = Mathf.Max(L(), other.L());
            int border_R = Mathf.Min(R(), other.R());

            // on choisit un sas random dans la frontière
            sas = new Vector2Int(Random.Range(border_L, border_R), U()-1);

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(sas.x, other.D());
        }
        else
        {
            Debug.LogError("Erreur dans la recherche des sas entre les secteurs");
            return;
        }


        // on crée un path entre les deux areas qui est obligatoirement compris dans les 2 secteurs
        List<Vector2Int> path = createPath(closest_area, sas);
        List<Vector2Int> other_path = other.createPath(other_sas, closest_area_other);

        // on ajoute les paths
        addPath(path);
        other.addPath(other_path);

        
        string s= "path :";
        foreach (Vector2Int pos in path)
        {
            s += " " + pos;
        }
        foreach (Vector2Int pos in other_path)
        {
            s += " " + pos;
        }
        print(s);

        // on ajoute le path aux secteurs
        // Vector2Int sas = addPath(path);
        // Vector2Int other_sas = other.addPath(path);

        // todo on ajoute un sas à la fin
        // sas.Add(path.Last());
        // Vector2Int sas = path.Last();

        // on ajoute une connection
        addConnection(sas, other_sas);
        other.addConnection(other_sas, sas);
    }

    public Vector2Int findClosestArea(Vector2Int area)
    {
        // on trouve l'area la plus proche de l'autre secteur
        float min_dist = 100000;
        Vector2Int closest_area = new Vector2Int(0, 0);

        // on parcourt les tiles
        foreach (Vector2Int pos in tiles)
        {
            Vector2Int tile = new Vector2Int(pos.x + x, pos.y + y);

            // on calcule la distance
            float dist = Vector2Int.Distance(tile, area);

            // on vérifie si c'est la plus petite distance
            if (dist < min_dist)
            {
                min_dist = dist;
                closest_area = tile;
            }
        }

        // on retourne l'area la plus proche
        return closest_area;
    }

    private List<Vector2Int> createPath(Vector2Int start, Vector2Int end)
    {
        // on crée un path entre deux areas dans le secteur
        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on crée un path
        List<Vector2Int> path = new List<Vector2Int>();

        // on ajoute le start
        path.Add(start);

        Vector2Int last_last_pos = new Vector2Int(-1,-1);

        // on boucle tant qu'on a pas atteint la fin
        while (path.Last() != end)
        {
            // on vérifie qu'on fait pas inlassablement des tours de boucle
            Vector2Int last_pos = path.Last();
            if (last_pos == last_last_pos)
            {
                Debug.LogError("Erreur dans la création du path");
                break;
            }
            last_last_pos = last_pos;

            // on initialise le path
            Vector2Int best_direction = new Vector2Int(0, 0);
            float min_dist = 100000;

            foreach (Vector2Int direction in directions)
            {
                // on récupère la position adjacente
                Vector2Int adjacentPosition = last_pos + direction;

                // on vérifie si la position adjacente est end
                if (adjacentPosition == end)
                {
                    // on ajoute la direction au path
                    // path.Add(adjacentPosition);
                    best_direction = direction;
                    break;
                }

                // on calcule la distance
                float dist = Vector2Int.Distance(adjacentPosition, end);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    best_direction = direction;
                }
            }
            // on ajoute la direction au path
            path.Add(last_pos + best_direction);
        }

        // on enlève le start et la fin
        // path.RemoveAt(0);
        // path.RemoveAt(path.Count - 1);

        // on retourne le path
        return path;
    }

    public Vector2Int addPath(List<Vector2Int> path)
    {

        // on renvoit la position du sas
        Vector2Int sas = new Vector2Int(-1000, -1000);

        // on regarde si c'est un path de départ (start dans le secteur)
        // ou un path d'arrivée (end dans le secteur)
        bool is_start = collidesWithRoomPoint(path.First());

        // on ajoute les tiles du path qui sont dans le secteur
        for (int i = 0; i < path.Count; i++)
        {
            // on vérifie que la tile est dans les limites du secteur
            if (!collidesWithRoomPoint(path[i]))
            {
                continue;
            }

            if (!is_start && sas == new Vector2Int(-1000, -1000))
            {
                // la position actuelle est notre sas
                sas = path[i];
            }
            else if (is_start)
            {
                sas = path[i];
            }

            // on modifie la tile
            Vector2Int tile = path[i];
            tile.x -= x;
            tile.y -= y;

            // on ajoute un corridor si ce n'est pas déjà une room
            if (!rooms.Contains(tile))
            {
                corridors.Add(tile);
                tiles.Add(tile);
            }
            else if (i == 0 || i == path.Count - 1)
            {
                // on ajoute un sas
                rooms.Remove(tile);
                corridors.Add(tile);
            }
        }

        return sas;
    }

    public void addConnection(Vector2Int start, Vector2Int end)
    {
        Vector2Int tile = new Vector2Int(start.x - x, start.y - y);

        // on ajoute une connection
        if (!connections.ContainsKey(tile))
        {
            connections.Add(tile,new List<Vector2Int>());
        }
        connections[tile].Add(end - start);
        // print("connection added : " + start + " / " + end + " / " + (end - start));
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

    public Vector2 center()
    {
        // on retourne le centre
        return new Vector2(cx(), cy());
    }

    public int L()
    {
        // on retourne la position la plus à gauche
        return x;
    }

    public int R()
    {
        // on retourne la position la plus à droite
        return x + w;
    }

    public int D()
    {
        // on retourne la position la plus en bas
        return y;
    }

    public int U()
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
            else if (sas.Contains(pos))
            {
                return "sas";
            }
            else
            {
                // on retourne le type de corridor
                return "corridor";
            }

        }
        return "ceiling";
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

        // on parcourt les directions dans le secteur
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

        // on vérifie les connections
        if (connections.ContainsKey(pos))
        {
            // on ajoute toutes les directions de la connection
            foreach (Vector2Int direction in connections[pos])
            {
                corr_directions.Add(direction);
            }

            // print("connection found : " + pos + " / " + connections[pos]);
        }

    }

    public BoundsInt getBounds()
    {
        // on retourne les bounds
        return new BoundsInt(x * area_size.x, y * area_size.y, 0, w * area_size.x, h * area_size.y, 1);
    }

    public float findClosestAreas(Sector other, out Vector2Int area_sec, out Vector2Int area_oth)
    {
        // finds the smallest path between two sectors
        float min_dist = 100000;

        area_sec = new Vector2Int(0, 0);
        area_oth = new Vector2Int(0, 0);

        foreach (Vector2Int sec in tiles)
        {
            foreach (Vector2Int oth in other.tiles)
            {
                float dist = Vector2Int.Distance(sec, oth);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    area_sec = sec;
                    area_oth = oth;
                }
            }
        }

        return min_dist;
    }

    public string getBorder(Sector other)
    {
        if (isColliding(other)) { return "collision"; }

        string s = "self / other : \n";
        s += "L : " + L() + " / " + other.L() + "\n";
        s += "R : " + R() + " / " + other.R() + "\n";
        s += "U : " + U() + " / " + other.U() + "\n";
        s += "D : " + D() + " / " + other.D() + "\n";
        // Debug.Log(s);

        // on vérifie si les secteurs sont voisins
        if (other.L() == R()) { return "R"; }
        if (other.R() == L()) { return "L"; }
        if (other.U() == D()) { return "D"; }
        if (other.D() == U()) { return "U"; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs sont voisins
        return "no border";
    }
}