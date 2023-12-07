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
    [SerializeField] private int iterations = 20;
    [SerializeField] private int walkLength = 5;
    [SerializeField] private bool startRandomlyEachIteration = false;
    [SerializeField] private int max_nb_rooms = 10;
    [SerializeField] private SectorGenerator sectorGenerator;

    // [Header("SECTOR TILES")]
    // [SerializeField] private string sectorSkin = "sector_1";

    [Header("SECTOR ENEMIES")]
    [SerializeField] private bool isSafe = false; // si true, pas d'ennemis dans le secteur
    [SerializeField] private int nb_enemies = 10;

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

        // on récupère les prefabs
        // chest_prefab = Resources.Load<GameObject>("prefabs/objects/chest");
        // computer_prefab = Resources.Load<GameObject>("prefabs/objects/computer");
    }

    // génère le secteur
    public void GenerateSector()
    {

        // on vide le secteur
        Clear();

        // on génère les salles et les couloirs
        sectorGenerator.GenerateRoomsAndCorridors(this.gameObject);

        Dictionary<string, int> emplacements_interactifs = new Dictionary<string, int>();

        // on lance l'initialisation des salles
        foreach (Transform child in transform)
        {
            Room room = child.GetComponent<Room>();
            room.init();
            emplacements_interactifs[room.gameObject.name] = room.GetNbEmplacementsInteractifs();
        }

        // on génère les objets interactifs
        GenerateInteractifs(emplacements_interactifs);
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

    // USEFUL FUNCTIONS

    void Clear()
    {
        // on détruit tous les enfants du secteur
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    // GETTERS
    public int GetIterations()
    {
        return iterations;
    }
    public int GetWalkLength()
    {
        return walkLength;
    }
    public bool GetStartRandomlyEachIteration()
    {
        return startRandomlyEachIteration;
    }
    public int GetMaxNbRooms()
    {
        return max_nb_rooms;
    }
}
