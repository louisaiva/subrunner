using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;


public class Sector : MonoBehaviour
{
    [Header("World")]
    public World world;
    protected WorldGenerator world_generator;
    protected Vector2Int area_size = new Vector2Int(16, 16);


    [Header("HashSet")]
    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> sas = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

    [Header("Connections")]
    public Dictionary<Vector2Int, List<Vector2Int>> connections = new Dictionary<Vector2Int, List<Vector2Int>>();

    [Header("Position")]
    public int x;
    public int y;
    public int w;
    public int h;

    [Header("Skin")]
    [SerializeField] protected string sector_skin = "base_sector";

    [Header("Objects")]
    [SerializeField] protected Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    [SerializeField] protected Dictionary<string, Transform> parents = new Dictionary<string, Transform>();

    [Header("Enemies")]
    protected List<Area> enemies_spawn_areas = new List<Area>();
    [SerializeField] protected List<Being> enemies = new List<Being>();
    [SerializeField] protected int nb_enemies = 5;
    public bool is_safe = false;

    [Header("Interactives")]
    [SerializeField] protected float density_interactives = 0.5f;
    [SerializeField] protected int planned_nb_interactives = 10;
    protected int nb_interactives = 0;


    // [Header("Doors")]
    // [SerializeField] private Dictionary<Vector2, string> sas_doors = new Dictionary<Vector2, string>();


    [Header("Ceilings")]
    public Dictionary<Vector2Int,Dictionary<string, GameObject>> ceilings = new Dictionary<Vector2Int,Dictionary<string, GameObject>>();
    private Dictionary<Vector2Int, List<string>> hidden_ceilings = new Dictionary<Vector2Int, List<string>>();
    private string ceiling_sas_prefabs_path = "prefabs/rooms/sas";

    [Header("Areas")]
    public GameObject area_prefab;
    public Dictionary<Vector2Int, Area> areas = new Dictionary<Vector2Int, Area>();


    // UNITY METHODS
    protected void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();
        world_generator = GameObject.Find("/generator").GetComponent<WorldGenerator>();
        area_prefab = Resources.Load<GameObject>("prefabs/sectors/base_area");

        // on récupère les prefabs
        prefabs.Add("light", Resources.Load<GameObject>("prefabs/objects/lights/small_light"));
        prefabs.Add("poster", Resources.Load<GameObject>("prefabs/objects/poster"));
        prefabs.Add("enemy", Resources.Load<GameObject>("prefabs/beings/enemies/zombo"));
        prefabs.Add("chest", Resources.Load<GameObject>("prefabs/objects/chest"));
        prefabs.Add("xp_chest", Resources.Load<GameObject>("prefabs/objects/xp_chest"));
        prefabs.Add("computer", Resources.Load<GameObject>("prefabs/objects/computer"));
        prefabs.Add("doorUD", Resources.Load<GameObject>("prefabs/objects/door"));
        prefabs.Add("doorLR", Resources.Load<GameObject>("prefabs/objects/door_L"));
        prefabs.Add("sector_label", Resources.Load<GameObject>("prefabs/objects/sector_label"));
        prefabs.Add("tag", Resources.Load<GameObject>("prefabs/objects/tag"));
        prefabs.Add("base_ceiling", Resources.Load<GameObject>("prefabs/sectors/ceiling"));

        // on récupère les parents
        parents.Add("light", transform.Find("decoratives/lights"));
        parents.Add("poster", transform.Find("decoratives/posters"));
        parents.Add("enemy", GameObject.Find("/world/enemies").transform);
        parents.Add("chest", transform.Find("interactives"));
        parents.Add("computer", transform.Find("interactives"));
        parents.Add("sas_doors", transform.Find("interactives"));
        parents.Add("sector_label", transform.Find("decoratives"));
        parents.Add("tag", transform.Find("decoratives/posters"));
        parents.Add("ceiling", transform.Find("ceilings"));
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

    public void UpdateCeilings(Vector2Int perso_area)
    {
        Dictionary<Vector2Int, List<string>> new_hidden_ceilings = new Dictionary<Vector2Int, List<string>>();

        // on affiche les ceilings autour de l'area du perso
        List<Vector2Int> directions = new List<Vector2Int>() { new Vector2Int(0, 0),
                                         new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0),
                                         new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)};
        foreach (Vector2Int dir in directions)
        {
            Vector2Int area = perso_area + dir;

            // on vérifie que l'area existe
            if (!ceilings.ContainsKey(area)) { continue; }

            // on l'ajoute aux ceilings à cacher
            string ceiling_type = "base";
            if (!new_hidden_ceilings.ContainsKey(area))
            {
                new_hidden_ceilings[area] = new List<string>() { ceiling_type };
            }
            else
            {
                new_hidden_ceilings[area].Add(ceiling_type);
            }
        }

        // on cache les nouveaux ceilings
        foreach (KeyValuePair<Vector2Int, List<string>> ceiling_area in new_hidden_ceilings)
        {
            Vector2Int area = ceiling_area.Key;
            foreach (string ceiling_type in ceiling_area.Value)
            {
                // si le ceiling est déjà caché c good
                if (hidden_ceilings.ContainsKey(area) && hidden_ceilings[area].Contains(ceiling_type)) { continue; }

                // sinon on le cache
                ceilings[area][ceiling_type].SetActive(false);
            }
        }

        // on affiche les ceilings qui ne sont plus cachés
        foreach (KeyValuePair<Vector2Int, List<string>> ceiling_area in hidden_ceilings)
        {
            Vector2Int area = ceiling_area.Key;
            foreach (string ceiling_type in ceiling_area.Value)
            {
                // si le ceiling doit être caché c good
                if (new_hidden_ceilings.ContainsKey(area) && new_hidden_ceilings[area].Contains(ceiling_type)) { continue; }

                // sinon on l'affiche
                ceilings[area][ceiling_type].SetActive(true);
            }
        }

        // on met à jour les hidden_ceilings
        hidden_ceilings = new_hidden_ceilings;

    }


    // INIT FUNCTIONS
    public void init()
    {
        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on calcule la position SW du secteur
        x = tiles.Min(x => x.x);
        y = tiles.Min(x => x.y);

        // on calcule la taille du secteur
        w = tiles.Max(x => x.x) - x + 1;
        h = tiles.Max(x => x.y) - y + 1;

        // on ajoute une tile de marge
        w += 1;
        h += 1;
    }

    public virtual void initAreas()
    {
        // on crée des areas pour chaque tile
        foreach (Vector2Int tile in tiles)
        {
            // on récup le type de l'area
            string type = getAreaName(tile);

            // on crée l'area
            GameObject area_go = Instantiate(area_prefab, Vector3.zero, Quaternion.identity);
            Area area = area_go.GetComponent<Area>();
            area.init(tile.x, tile.y, area_size.x, area_size.y, type, this);

            // on ajoute l'area au dictionnaire
            areas[tile] = area;
        }
    }


    // MAIN FUNCTIONS
    public void move(Vector2Int movement)
    {
        x += movement.x;
        y += movement.y;
    }

    public void MoveToSetCenterTo(Vector2 c)
    {
        // on calcule le mouvement à faire
        float closest_x = c.x - w/2f;
        float closest_y = c.y - h/2f;

        // on bouge le secteur
        x = Mathf.RoundToInt(closest_x);
        y = Mathf.RoundToInt(closest_y);
    }



    // GENERATION
    public virtual void GENERATE()
    {
        // on génère les areas
        foreach (KeyValuePair<Vector2Int, Area> area in areas)
        {
            area.Value.GENERATE();
        }

        // on récupère les areas spawn
        enemies_spawn_areas = areas.Values.Where(x => x.hasEnemyEmplacement()).ToList();

        // on place les ennemis
        if (!is_safe)
        {
            PlaceEnemies();
        }

        // on place les portes
        // this.sas_doors = empl_doors;
        // PlaceDoors();

        // on place les labels
        // PlaceLabels(empl_labels);
    }

    public void GenerateCeiling(Vector2Int area)
    {
        // on génère les ceilings de l'area spécifique
        string type = getAreaType(area);
        if (type == "ceiling") { return; }

        else if (type == "sas")
        {

            // si c'est un sas : on a 2 ceilings : base et sas
            // on récupère les objets dans le dossier ceiling_sas_prefabs_path
            GameObject area_go = Instantiate(Resources.Load<GameObject>(ceiling_sas_prefabs_path + "/" + getAreaName(area)), Vector3.zero, Quaternion.identity);
            GameObject base_ceiling = area_go.transform.Find("ceilings/base").gameObject;
            GameObject sas_ceiling = area_go.transform.Find("ceilings/sas").gameObject;

            // on les met à la bonne position
            Vector3 translation = new Vector3((x+area.x) * area_size.x / 2, (y+area.y) * area_size.y / 2, 0f);
            Vector3 base_pos = base_ceiling.transform.position + translation;
            Vector3 sas_pos = sas_ceiling.transform.position + translation;
            // base_ceiling.transform.position = new Vector3(area.x * area_size.x / 2, area.y * area_size.y / 2, 0f);
            // sas_ceiling.transform.position = new Vector3(area.x * area_size.x / 2, area.y * area_size.y / 2, 0f);
            base_ceiling.transform.position = base_pos;
            sas_ceiling.transform.position = sas_pos;

            // on les met dans le bon parent
            base_ceiling.transform.SetParent(parents["ceiling"]);
            sas_ceiling.transform.SetParent(parents["ceiling"]);

            // on les ajoute au dictionnaire
            ceilings[area] = new Dictionary<string, GameObject>();
            ceilings[area]["base"] = base_ceiling;
            ceilings[area]["sas"] = sas_ceiling;

            // on affiche les ceilings
            base_ceiling.SetActive(true);
            foreach (Transform child in sas_ceiling.transform)
            {
                child.gameObject.SetActive(true);
            }
            sas_ceiling.SetActive(true);
            foreach (Transform child in base_ceiling.transform)
            {
                child.gameObject.SetActive(true);
            }

            // on supprime l'oject area_go
            Destroy(area_go);
        }
        /* else if (type == "secret")
        {
            GenerateCeilingSecret(area);
        } */
        else
        {
            // si c'est une area normale on génère un ceiling carré : sprite qui fait exactement la taille de l'area
            Vector3 pos = new Vector3((x + area.x)*area_size.x/2 + area_size.x / 4, (y + area.y)*area_size.y/2 + area_size.y / 4, 0f);
            print("placing ceiling at " + pos +" because x,y = " + x + "," + y + " and area.x,area.y = " + area.x + "," + area.y);
            GameObject ceiling = Instantiate(prefabs["base_ceiling"], pos, Quaternion.identity);
            ceiling.transform.SetParent(parents["ceiling"]);

            ceilings[area] = new Dictionary<string, GameObject>();
            ceilings[area]["base"] = ceiling;

        }
    }

    protected void PlaceEnemies()
    {
        for (int i = 0; i < nb_enemies; i++)
        {
            PlaceEnemy();
        }
    }

    protected void PlaceEnemy()
    {
        if (enemies_spawn_areas.Count == 0) { return; }

        // on choisi un spawn area random
        Area spawn_area = enemies_spawn_areas[Random.Range(0, enemies_spawn_areas.Count)];

        // on place l'enemy
        GameObject enemy = spawn_area.PlaceEnemy();

        // on ajoute l'enemy à la liste
        enemies.Add(enemy.GetComponent<Being>());
    }

    /* protected void PlaceChest()
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
    
    protected void PlaceComputer()
    {
        // on récupère un emplacement
        Vector2 empl = consumeRandomEmplacement("interactives");

        // on instancie un computer
        Vector3 pos = new Vector3(empl.x, empl.y, 0);
        GameObject computer = Instantiate(prefabs["computer"], pos, Quaternion.identity);

        // on met le bon parent
        computer.transform.SetParent(parents["computer"]);
    }
   */
    
    /* private void PlaceDoors()
    {
        int i = 0;

        // on parcourt les connections
        foreach (KeyValuePair<Vector2, string> empl in sas_doors)
        {

            // on instancie une porte
            Vector3 pos = new Vector3(empl.Key.x, empl.Key.y, 0);
            GameObject door = null;


            if ("hackable_vertical" == empl.Value)
            {
                print("instantiate a UD door at " + empl.Key);
                // on instancie une porte verticale (up ou down)
                door = Instantiate(prefabs["doorUD"], pos, Quaternion.identity);
            }
            else if ("simple_side" == empl.Value)
            {
                print("instantiate a LR door at " + empl.Key);
                // on instancie une porte horizontale (left ou right)
                door = Instantiate(prefabs["doorLR"], pos, Quaternion.identity);
            }

            // on met le bon parent
            door.transform.SetParent(parents["sas_doors"]);

            i++;
        }
    } */


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



    // BORDERS AND CONNECTIONS
    public string getBorder(Sector other)
    {
        // check if the sectors are colliding
        if (isColliding(other)) { return "collision"; }


        // check if the sectors are bordering
        if (other.L() > R()) { return "no border"; }
        else if (other.R() < L()) { return "no border"; }
        else if (other.D() > U()) { return "no border"; }
        else if (other.U() < D()) { return "no border"; }

        // check the particular case of the corners
        if (other.L() == R())
        {
            if (other.D() == U() || other.U() == D()) { return "no border"; }
        }
        if (other.R() == L())
        {
            if (other.D() == U() || other.U() == D()) { return "no border"; }
        }

        /* string s = "self / other : \n";
        s += "L : " + L() + " / " + other.L() + "\n";
        s += "R : " + R() + " / " + other.R() + "\n";
        s += "U : " + U() + " / " + other.U() + "\n";
        s += "D : " + D() + " / " + other.D() + "\n"; */
        // Debug.Log(s);

        // check which border is shared
        if (other.L() == R()) { return "R"; }
        if (other.R() == L()) { return "L"; }
        if (other.U() == D()) { return "D"; }
        if (other.D() == U()) { return "U"; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs sont voisins
        return "no border";
    }

    public void connectWithSector(Sector other)
    {
        // créé le plus petit chemin entre les deux secteurs

        print("(Sector - connectWithSector) connecting " + gameObject.name + (this is ComplexeSector? "(ComplexeSector)": "") + " with " + other.gameObject.name + (other is ComplexeSector ? "(ComplexeSector)" : ""));

        // on trouve la frontière entre les secteurs
        string border = getBorder(other);

        // on trouve l'area la plus proche de l'autre secteur
        Vector2Int closest_area = findClosestInsideConnectingArea(other,border);
        if (closest_area == new Vector2Int(-1, -1))
        {
            Debug.LogError("(Sector - connectWithSector) Erreur dans la recherche de l'area la plus proche");
            return;
        }

        // on récupère la tile de l'autre secteur la plus proche de l'area
        Vector2Int closest_area_other = other.findClosestConnectingArea(closest_area);

        print(gameObject.name + " : " + closest_area + " / " + other.gameObject.name + " : " + closest_area_other);

        // on choisit un sas dans notre secteur qui est sur la border
        Vector2Int sas = closest_area;
        Vector2Int other_sas = closest_area_other;
        if (border == "R")
        {

            // on cherche les dimensions de la frontière partagée
            int border_D = Mathf.Max(D(), other.D());
            int border_U = Mathf.Min(U(), other.U());

            // on choisit le sas le plus proche de closest_area
            if (closest_area.y < border_U && closest_area.y >= border_D)
            {
                sas = new Vector2Int(R()-1, closest_area.y);
            }
            else if (closest_area.y < border_D)
            {
                sas = new Vector2Int(R()-1, border_D);
            }
            else if (closest_area.y >= border_U)
            {
                sas = new Vector2Int(R()-1, border_U-1);
            }
            else
            {
                // on choisit un sas aléatoire dans la frontière
                sas = new Vector2Int(R()-1, Random.Range(border_D, border_U));
            }

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(other.L(), sas.y);
        }
        else if (border == "U")
        {
            // on cherche les dimensions de la frontière partagée
            int border_L = Mathf.Max(L(), other.L());
            int border_R = Mathf.Min(R(), other.R());

            // on choisit le sas le plus proche de closest_area
            if (closest_area.x >= border_L && closest_area.x < border_R)
            {
                sas = new Vector2Int(closest_area.x , U() - 1);
            }
            else if (closest_area.x < border_L)
            {
                sas = new Vector2Int(border_L, U() - 1);
            }
            else if (closest_area.x >= border_R)
            {
                sas = new Vector2Int(border_R-1 , U() - 1);
            }
            else
            {
                // on choisit un sas random dans la frontière
                sas = new Vector2Int(Random.Range(border_L, border_R), U() - 1);
            }

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(sas.x, other.D());
        }
        else
        {
            Debug.LogError("(Sector - connectWithSector) Erreur dans la recherche des sas entre les secteurs");
            return;
        }


        // on crée un path entre les deux areas qui est obligatoirement compris dans les 2 secteurs
        List<Vector2Int> path = createPath(closest_area, sas);
        List<Vector2Int> other_path = other.createPath(other_sas, closest_area_other);

        // on ajoute les paths
        addPath(path);
        other.addPath(other_path);

        
        string s= "(Sector - connectWithSector) path :";
        foreach (Vector2Int pos in path)
        {
            s += " " + pos;
        }
        foreach (Vector2Int pos in other_path)
        {
            s += " " + pos;
        }
        print(s);

        // on ajoute une connection
        addConnection(sas, other_sas);
        other.addConnection(other_sas, sas);
    }

    public virtual Vector2Int findClosestInsideConnectingArea(Sector other, string border="")
    {
        // trouve l'area la plus proche de l'autre secteur
        Vector2Int closest_area = new Vector2Int(0, 0);

        // on trouve la frontière entre les secteurs
        if (border == "") {border = getBorder(other);}

        // on vérifie si les secteurs sont collés
        if (border == "R")
        {
            // on trouve l'area la plus proche de l'autre secteur par la droite
            float min_dist = 100000;
            for (int y_area = D(); y_area < U(); y_area++)
            {
                Vector2Int tile = new Vector2Int(R() - 2, y_area);

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
                Vector2Int tile = new Vector2Int(x_area, U() - 2);

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
            Debug.LogError("(Sector - connectWithSector) Les secteurs collisionnent");
            return new Vector2Int(-1,-1);
        }
        else if (border == "no border")
        {
            Debug.LogError("(Sector - connectWithSector) Les secteurs ne sont pas voisins");
            return new Vector2Int(-1,-1);
        }
        else
        {
            Debug.LogError("(Sector - connectWithSector) Erreur dans la recherche de la frontière entre les secteurs");
            return new Vector2Int(-1,-1);
        }

        return closest_area;

    }

    public virtual Vector2Int findClosestConnectingArea(Vector2Int area)
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

    protected virtual List<Vector2Int> createPath(Vector2Int start, Vector2Int end)
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
                Debug.LogError("(Sector - createPath) Erreur dans la création du path");
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

    public void addPath(List<Vector2Int> path)
    {

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

            // on modifie la tile
            Vector2Int tile = path[i];
            tile.x -= x;
            tile.y -= y;

            if (!rooms.Contains(tile))
            {
                // on ajoute un corridor si ce n'est pas déjà une room
                corridors.Add(tile);
                tiles.Add(tile);
            }
        }
    }

    public void addSas(Vector2Int sas_position)
    {
        // on ajoute un sas
        sas.Add(sas_position);

        // on vérifie que la tile n'est pas déjà dans les tiles
        if (tiles.Contains(sas_position))
        {
            // on l'enlève des rooms et des corridors
            rooms.Remove(sas_position);
            corridors.Remove(sas_position);
        }
        else
        {
            tiles.Add(sas_position);
        }

    }

    public void addConnection(Vector2Int start, Vector2Int end)
    {
        Vector2Int tile = new Vector2Int(start.x - x, start.y - y);

        // on ajoute un sas
        addSas(tile);

        // on ajoute une connection
        if (!connections.ContainsKey(tile))
        {
            connections.Add(tile,new List<Vector2Int>());
        }
        connections[tile].Add(end - start);
        // print("connection added : " + start + " / " + end + " / " + (end - start));
    }



    // SETTERS
    public void setSkin(string skin)
    {
        sector_skin = skin;
    }


    // AREA GETTERS
    public Area getArea(Vector2Int area)
    {
        // on récupère l'area
        if (hasArea(area))
        {
            return areas[area];
        }
        else
        {
            Debug.LogError("(Sector - getArea) Erreur dans la récupération de l'area");
            return null;
        }
    }

    public bool hasArea(Vector2Int area)
    {
        // on vérifie si le secteur a l'area
        return areas.ContainsKey(area);
    }

    public Vector2Int getGlobalRandomRoomPosition()
    {
        // on print les tiles et les rooms
        string s = "tiles : \n\n";
        foreach (Vector2Int tile in tiles)
        {
            s += " " + tile + "\n";
        }
        s += "\n\n   rooms : \n\n";
        foreach (Vector2Int room in rooms)
        {
            s += " " + room + "\n";
        }
        print(s);

        try
        {
            // on récupère une room aléatoire
            Vector2Int pos = rooms.ElementAt(Random.Range(0, rooms.Count));

            Debug.Log("(Sector - getGlobalRandomRoomPosition) pos : " + pos + " / x,y : " + x + "," + y);

            // on retourne la position globale
            return new Vector2Int(pos.x + x, pos.y + y);
        }
        catch
        {
            Debug.LogError("(Sector - getGlobalRandomRoomPosition) Erreur pas de room dans le secteur " + gameObject.name);
            return new Vector2Int(-1, -1);
        }
    }

    public List<Zone> getCentralZones() 
    {
        // on récupère toutes les zones qui sont au centre de leurs areas respectives
        // return areas.Values.ToList().Where(area => area.couldHostLegendaryZone()).ToList();

        List<Zone> zones = new List<Zone>();
        foreach (Area area in areas.Values)
        {
            zones.AddRange(area.getCentralZones());
        }
        return zones;
    }


    // GETTERS
    public Vector2Int wh()
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

    public virtual string getAreaType(Vector2Int pos)
    {
        // pos en area
        if (hasArea(pos))
        {
            return areas[pos].type.Split('_')[0];
        }

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
        if (hasArea(pos))
        {
            return areas[pos].type;
        }

        // pos en area
        string type = getAreaType(pos);
        if (type == "ceiling") { return "ceiling"; }
        if (type == "handmade") { return "handmade"; }

        // on récupère les noms des salles
        string name = type + "_";

        // on récupère le voisinage
        Dictionary<Vector2Int, string> neighbors = getAreaNeighborsType(pos);
        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on ajoute le nom de la direction
        if (neighbors.ContainsKey(Vector2Int.up))
        {
            if (type == "sas" && neighbors[Vector2Int.up] == "connection")
                name += "N";
            else
                name += "U";
        }
        if (neighbors.ContainsKey(Vector2Int.down))
        {
            if (type == "sas" && neighbors[Vector2Int.down] == "connection")
                name += "S";
            else
                name += "D";
        }
        if (neighbors.ContainsKey(Vector2Int.left))
        {
            if (type == "sas" && neighbors[Vector2Int.left] == "connection")
                name += "W";
            else
                name += "L";
        }
        if (neighbors.ContainsKey(Vector2Int.right))
        {
            if (type == "sas" && neighbors[Vector2Int.right] == "connection")
                name += "E";
            else
                name += "R";
        }

        // on retourne le nom
        return name;
    }

    public virtual Dictionary<Vector2Int, string> getAreaNeighborsType(Vector2Int pos)
    {
        // on regarde les types des voisins
        Dictionary<Vector2Int, string> neighbors = new Dictionary<Vector2Int, string>();

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on parcourt les directions dans le secteur
        foreach (Vector2Int direction in directions)
        {
            // on récupère la position adjacente
            Vector2Int adjacentPosition = pos + direction;

            // on récupère le type de l'area
            string type = getAreaType(adjacentPosition);

            if (type != "ceiling")
            {
                // on ajoute l'area à la liste
                neighbors.Add(direction, type);
            }
        }

        // on regarde si c'est une position de connection
        if (connections.ContainsKey(pos))
        {
            // on ajoute toutes les directions de la connection
            foreach (Vector2Int dir in connections[pos])
            {
                neighbors.Add(dir, "connection");
            }
        }

        return neighbors;

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

    public string getAreaSkin(Vector2Int pos)
    {
        return sector_skin;
    }

}