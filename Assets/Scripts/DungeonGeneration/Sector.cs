using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class Sector : MonoBehaviour
{

    // une instance de SECTOR est un secteur du jeu
    // s'occupe de différentes choses:
    // génération procédurale du secteur (salles, couloirs, etc)
    // répartition des objets interactifs dans les salles (ennemis, chests, computers, etc)
    // mise en place des bonnes tiles sur les tilemaps en fonction du skin du secteur

    [Header("SECTOR")]
    [SerializeField] private SectorGenerator sectorGenerator;
    [SerializeField] private PreSector preSec;
    [SerializeField] private Dictionary<Vector2Int, Room> areas = new Dictionary<Vector2Int, Room>();
    // stocke les salles et les couloirs du secteur via leur POSITION GLOBALE
    

    // [Header("SECTOR TILES")]
    // [SerializeField] private string sectorSkin = "sector_1";

    [Header("SECTOR ENEMIES")]
    [SerializeField] private bool isSafe = false; // si true, pas d'ennemis dans le secteur
    [SerializeField] private int nb_enemies = 5;
    private GameObject enemy_prefab;
    private Transform enemy_parent;
    [SerializeField] private float enemy_spawn_radius = 1f;


    [Header("SECTOR OBJECTS")]
    [SerializeField] private int nb_chests = 2;
    // private GameObject chest_prefab;
    [SerializeField] private int nb_computers = 3;
    // private GameObject computer_prefab;


    [Header("SECTOR TUYAUX")]
    [SerializeField] private bool hasTuyaux = true;
    [SerializeField] private HashSet<Vector2Int> tuyaux_grey = new HashSet<Vector2Int>();
    [SerializeField] private HashSet<Vector2Int> tuyaux_blue = new HashSet<Vector2Int>();
    [SerializeField] private int corr_number = 30;
    [SerializeField] private Vector2Int corr_length_min_max = new Vector2Int(5, 30);
    [SerializeField] private float corr_random_start_chance = 0.5f;

    [Header("SECTOR CEILING")]
    [SerializeField] private GameObject ceiling_prefab;
    private Transform ceiling_parent;


    // functions
    void Awake()
    {
        // on récupère le sectorgenerator
        GameObject generator = GameObject.Find("/generator");
        sectorGenerator = generator.GetComponent<SectorGenerator>();

        // on récupère le parent des ennemis
        enemy_parent = transform.Find("enemies");

        // on récupère le prefab des ennemis
        enemy_prefab = Resources.Load<GameObject>("prefabs/beings/enemies/zombo");

        // on récupère le prefab du plafond
        ceiling_prefab = Resources.Load<GameObject>("prefabs/sectors/ceiling");

        // on récupère le parent du plafond
        ceiling_parent = transform.Find("ceiling");
    }

    void Update()
    {
        if (!isSafe)
        {
            // on compte le nombre d'ennemis vivants
            int nb_enemies_alive = 0;
            foreach (Transform child in enemy_parent.transform)
            {
                if (child.GetComponent<Being>().isAlive())
                {
                    nb_enemies_alive++;
                }
            }

            // on vérifie si on doit générer un nouvel ennemi
            if (nb_enemies_alive < nb_enemies)
            {
                GenerateSingleEnemy();
            }
        }
    }

    // génère le secteur
    /* public void GenerateSector()
    {

        // on vide le secteur
        Clear();

        // on génère les salles et les couloirs
        sectorGenerator.GenerateRoomsAndCorridors(this.gameObject);

        Dictionary<string, int> emplacements_interactifs = new Dictionary<string, int>();

        // on lance l'initialisation des salles
        foreach (Transform child in transform)
        {
            // on vérifie que ce n'est pas le parent des ennemis
            if (child.gameObject.name == "enemies")
            {
                continue;
            }

            // on récupère la salle
            Room room = child.GetComponent<Room>();
            room.init();
            emplacements_interactifs[room.gameObject.name] = room.GetNbEmplacementsInteractifs();
        }

        // on génère les objets interactifs
        GenerateInteractifs(emplacements_interactifs);


        // on génère les ennemis
        if (!isSafe)
        {
            GenerateEnemies();
        }

    } */

    public void GenerateSelf(PreSector preSec)
    {
        // on vide le secteur
        Clear();

        // on intègre le pre-secteur
        this.preSec = preSec;

        // on récupère les hashsets des positions des salles et des couloirs
        HashSet<Vector2Int> roomPositions = preSec.rooms;
        HashSet<Vector2Int> corrPositions = preSec.corridors;
        Vector2Int sectorGlobalPos = preSec.xy();

        // on génère les salles et les couloirs
        areas = sectorGenerator.GenerateRooms(roomPositions, corrPositions, sectorGlobalPos,this.gameObject);
        Dictionary<Vector2Int, Room> corridors = sectorGenerator.GenerateCorridors(corrPositions, roomPositions, sectorGlobalPos, this.gameObject);
        // areas.UnionWith(sectorGenerator.GenerateCorridors(corrPositions, roomPositions, sectorGlobalPos, this.gameObject));
        for (int i = 0; i < corridors.Count; i++)
        {
            areas[corridors.Keys.ToList()[i]] = corridors.Values.ToList()[i];
        }


        Dictionary<string, int> emplacements_interactifs = new Dictionary<string, int>();

        // on lance l'initialisation des salles
        foreach (Room room in GetRooms())
        {
            room.init();
            emplacements_interactifs[room.gameObject.name] = room.GetNbEmplacementsInteractifs();
        }

        // on génère les objets interactifs
        GenerateInteractifs(emplacements_interactifs);


        // on génère les ennemis
        if (!isSafe)
        {
            GenerateEnemies();
        }

        // on génère les tuyaux
        if (hasTuyaux)
        {
            GenerateTuyaux();
        }

        // on génère le plafond
        GenerateCeiling();
    }

    // génère les objets interactifs
    void GenerateInteractifs(Dictionary<string,int> emplacements_interactifs)
    {
        int max_emplacements = emplacements_interactifs.Values.ToList().Sum();

        // on vériie que le nombre d'emplacements interactifs disponibles est suffisant
        if (max_emplacements < nb_chests + nb_computers)
        {
            // on récupère la proportion de chests et de computers
            float proportion_chests = (float)nb_chests / (float)(nb_chests + nb_computers);

            // on calcule le nombre de chests
            nb_chests = (int)(proportion_chests * max_emplacements);

            // on calcule le nombre de computers
            nb_computers = max_emplacements - nb_chests;
        }

        // on place les chests au hasard
        for (int i = 0; i < nb_chests; i++)
        {
            // on récupère une salle au hasard
            string room_name = emplacements_interactifs.Keys.ToList()[Random.Range(0, emplacements_interactifs.Keys.Count)];
            Room room = transform.Find(room_name).GetComponent<Room>();

            // on vérifie qu'il reste des emplacements interactifs dans la salle
            if (emplacements_interactifs[room.gameObject.name] == 0)
            {
                // on cherche une autre salle
                i--;
                continue;
            }

            // on place le chest
            room.PlaceChest();

            // si on est dans la dernière itération, on place un item légendaire
            if (i == nb_chests - 1)
            {
                Item hackin_os = Resources.Load<Item>("prefabs/items/legendary/hackin_os");
                hackin_os = Instantiate(hackin_os, new Vector3(0,0,1000), Quaternion.identity);
                // print(hackin_os + " " + hackin_os.item_name);
                Chest chest = transform.Find(room_name).transform.Find("chests").transform.GetChild(0).GetComponent<Chest>();
                chest.forceGrab(hackin_os);
            }

            // on décrémente le nombre d'emplacements interactifs disponibles
            emplacements_interactifs[room.gameObject.name]--;
            if (emplacements_interactifs[room.gameObject.name] == 0)
            {
                emplacements_interactifs.Remove(room.gameObject.name);
            }
        }

        // on place les computers au hasard
        for (int i = 0; i < nb_computers; i++)
        {
            // on récupère une salle au hasard
            string room_name = emplacements_interactifs.Keys.ToList()[Random.Range(0, emplacements_interactifs.Keys.Count)];
            Room room = transform.Find(room_name).GetComponent<Room>();

            // on vérifie qu'il reste des emplacements interactifs dans la salle
            if (emplacements_interactifs[room.gameObject.name] == 0)
            {
                // on cherche une autre salle
                i--;
                continue;
            }

            // on place le computer
            room.PlaceComputer();

            // on décrémente le nombre d'emplacements interactifs disponibles
            emplacements_interactifs[room.gameObject.name]--;
            if (emplacements_interactifs[room.gameObject.name] == 0)
            {
                emplacements_interactifs.Remove(room.gameObject.name);
            }
        }
    }


    // génère les ennemis
    public void GenerateEnemies()
    {
        // on place les ennemis
        for (int i = 0; i < nb_enemies; i++)
        {
            GenerateSingleEnemy();
        }
    }


    // génère les tuyaux
    private void GenerateTuyaux()
    {
        // on déclare le path des tiles
        string[] tuyaux_tiles_path = new string[2] {"tilesets/tuyau_grey","tilesets/tuyau_blue"};
        string[] tuyaux_tm_path = new string[2] {"tm/tuyaux_grey","tm/tuyaux_blue"};

        // liste des réseaux de tuyaux
        List<HashSet<Vector2Int>> tuyaux_total = new List<HashSet<Vector2Int>>();
        tuyaux_total.Add(tuyaux_grey);
        tuyaux_total.Add(tuyaux_blue);

        for (int i = 0; i < tuyaux_total.Count; i++)
        {

            // on récup le réseau de tuyau
            HashSet<Vector2Int> tuyau = tuyaux_total[i];

            // on génère les tiles
            tuyau = GenerateTuyauPath();

            // on récupère le tilemap
            Tilemap tuyau_tm = transform.Find(tuyaux_tm_path[i]).GetComponent<Tilemap>();

            // on récupère la tilebase
            TileBase tuyau_tile = Resources.Load<TileBase>(tuyaux_tiles_path[i]);

            // on peint la tilemap avec notre tilebase et les positions du hashset
            foreach (Vector2Int position in tuyau)
            {
                tuyau_tm.SetTile(new Vector3Int(position.x, position.y, 0), tuyau_tile);
            }
        }

    }

    private HashSet<Vector2Int> GenerateTuyauPath()
    {

        // on définit les paramètres de la génération des tuyaux
        // on utilise le walkinMan

        // on génère n fois le chemin (n = iterations) pour avoir un gros réseau
        // de tuyaux

        // on définit la position de départ (prendre la position du milieu du secteur)
        Vector2 currentPosition = preSec.GetCentralWorldPos();
        Vector2Int start = new Vector2Int((int)currentPosition.x, (int)currentPosition.y);

        // on génère le chemin total
        HashSet<Vector2Int> tuyaux = ProceduralGenerationAlgo.RandomWalkCorridorNetwork(start, corr_number, corr_length_min_max, corr_random_start_chance);

        return tuyaux;
    }


    // génère le plafond (pour cacher les tuyaux etc)
    public void GenerateCeiling()
    {
        // on récupère le HashSet du plafond
        HashSet<Vector2Int> ceiling = preSec.ceiling;

        // on parcourt le hashset et on place les tiles
        foreach (Vector2Int position in ceiling)
        {
            // on instancie le plafond
            GameObject ceiling_tile = Instantiate(ceiling_prefab, ceiling_parent);

            // on met à jour le parent du plafond
            ceiling_tile.transform.SetParent(ceiling_parent);

            // on met à jour la position du plafond
            ceiling_tile.transform.localPosition = new Vector3(position.x*8f, position.y*7.5f, 0);
        }
    }

    // ONCE FONCTIONS
    public void GenerateSingleEnemy()
    {
        // on choisit une salle au hasard dans les enfants du secteur
        GameObject room = transform.GetChild(Random.Range(0, transform.childCount)).gameObject;

        // on vérifie que c'est une room
        while (room.GetComponent<Room>() == null)
        {
            // on cherche une autre salle
            room = transform.GetChild(Random.Range(0, transform.childCount)).gameObject;
        }
        
        // on récupère la position de spawn
        Vector3 spawn_position = new Vector3();

        // on ajoute un offset aléatoire dans un cercle de rayon enemy_spawn_radius
        spawn_position = Random.insideUnitCircle.normalized * enemy_spawn_radius;
        spawn_position += room.GetComponent<Room>().GetEnemySpawnEmplacement();

        // on instancie l'ennemi
        GameObject enemy = Instantiate(enemy_prefab, spawn_position, Quaternion.identity, enemy_parent.transform);

        // on met à jour le parent de l'ennemi
        enemy.transform.SetParent(enemy_parent);
    }

    // USEFUL FUNCTIONS

    void Clear()
    {
        // on détruit tous les enfants du secteur
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "enemies" || child.gameObject.name == "ceiling")
            {
                // on détruit tous les enfants du parent des ennemis
                foreach (Transform child2 in child)
                {
                    Destroy(child2.gameObject);
                }
            }
            else if (child.gameObject.name == "tm")
            {
                // on vide les tilemaps
                foreach (Transform child2 in child)
                {
                    child2.GetComponent<Tilemap>().ClearAllTiles();
                }
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }

    List<Room> GetRooms()
    {
        // on récupère les salles
        /* List<Room> rooms = new List<Room>();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Room>() != null)
            {
                rooms.Add(child.GetComponent<Room>());
            }
        }
        return rooms; */
        return areas.Values.ToList();
    }

    // GETTERS

    public bool IsSafe()
    {
        return isSafe;
    }

    public PreSector GetPreSector()
    {
        return preSec;
    }


    public HashSet<Vector2Int> GetGlobalTiles()
    {
        // returns the global tiles of the sector (not the ceiling, only the above tile of each tile in each room)

        // on récupère les tiles de chaque salle
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
        
        // on parcout les salles via le dict
        for (int i=0; i<areas.Count; i++)
        {
            // on récupère la salle
            Room area = areas.Values.ToList()[i];
            Vector2Int global_area_pos = areas.Keys.ToList()[i] * 16;

            // on récupère les tiles de la salle
            HashSet<Vector2Int> room_tiles = area.GetTiles();

            // on déplace les tiles de la salle
            foreach (Vector2Int tile in room_tiles)
            {
                tiles.Add(tile + global_area_pos);
            }
        }


        return tiles;
    }

    public string GetTileType(Vector2Int globalTilePos)
    {
        // on récupère la position de la room
        Vector2Int roomPos = globalTilePos / 16;

        // on vérifie ce qu'on touche
        string room_type = preSec.getGlobalRoomType(roomPos);
        print("room " + roomPos + " : " + room_type);

        if (new string[2] {"ceiling", "not found"}.Contains(room_type))
        {
            return room_type;
        }

        // on récupère la room
        Room room = GetRoomWithGlobalRoomPos(globalTilePos, out Vector2Int localTilePos);

        // on récupère le type de tile
        string type = room.GetTileType(localTilePos);

        return type;
    }

    public Room GetRoomWithGlobalRoomPos(Vector2Int globalTilePos, out Vector2Int localTilePos)
    {
        // on récupère la position de la room
        Vector2Int roomPos = globalTilePos / 16;

        // on récupère la position locale
        localTilePos = globalTilePos - roomPos * 16;

        // on vérifie que la room existe
        if (!areas.ContainsKey(roomPos))
        {
            print("room " + roomPos + " not found in " + this.gameObject.name);
            print("global tile pos: " + globalTilePos);

            string s = "";
            foreach (Vector2Int key in areas.Keys)
            {
                s += key + "\n";
            }
            print("rooms: " + s);
            return null;
        }

        // on retourne la room
        return areas[roomPos];
    }

}
