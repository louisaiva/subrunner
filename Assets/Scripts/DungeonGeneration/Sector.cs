using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Sector : MonoBehaviour
{

    // une instance de SECTOR est un secteur du jeu
    // s'occupe de différentes choses:
    // génération procédurale du secteur (salles, couloirs, etc)
    // répartition des objets interactifs dans les salles (ennemis, chests, computers, etc)
    // mise en place des bonnes tiles sur les tilemaps en fonction du skin du secteur

    [Header("SECTOR GENERATION")]
    // [SerializeField] private int iterations = 20;
    // [SerializeField] private int walkLength = 5;
    // [SerializeField] private bool startRandomlyEachIteration = false;
    // [SerializeField] private int max_nb_rooms = 10;
    [SerializeField] private SectorGenerator sectorGenerator;

    // [Header("SECTOR TILES")]
    // [SerializeField] private string sectorSkin = "sector_1";

    [Header("SECTOR ENEMIES")]
    [SerializeField] private bool isSafe = false; // si true, pas d'ennemis dans le secteur
    [SerializeField] private int nb_enemies = 10;
    private GameObject enemy_prefab;
    private Transform enemy_parent;
    [SerializeField] private float enemy_spawn_radius = 1f;


    [Header("SECTOR OBJECTS")]
    [SerializeField] private int nb_chests = 2;
    // private GameObject chest_prefab;
    [SerializeField] private int nb_computers = 3;
    // private GameObject computer_prefab;

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

    public void GenerateSelf(HashSet<Vector2Int> roomPositions, HashSet<Vector2Int> corrPositions)
    {
        // on vide le secteur
        Clear();

        // on génère les salles et les couloirs
        sectorGenerator.GenerateRooms(roomPositions, corrPositions,this.gameObject);
        sectorGenerator.GenerateCorridors(corrPositions, roomPositions, this.gameObject);

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
            if (child.gameObject.name == "enemies")
            {
                // on détruit tous les enfants du parent des ennemis
                foreach (Transform child2 in child)
                {
                    Destroy(child2.gameObject);
                }
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }

}
