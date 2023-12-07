using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    [Header("WORLD GENERATION")]
    [SerializeField] private int nb_sectors = 1;
    GameObject sector_prefab;
    GameObject world;

    [Header("sectors")]
    [SerializeField] private List<GameObject> sectors = new List<GameObject>();

    // unity functions
    void Start()
    {
        // on récupère le sector prefab
        sector_prefab = Resources.Load<GameObject>("prefabs/sectors/sector_4");

        // on récupère le world
        world = GameObject.Find("/world");

        // on génère le monde
        GenerateWorld();
    }

    // génère le monde
    public void GenerateWorld()
    {
        // on vide le monde
        Clear();

        // on génère les secteurs
        for (int i = 0; i < nb_sectors; i++)
        {
            // on instancie le secteur
            GameObject sector = Instantiate(sector_prefab, world.transform);

            // on génère le secteur
            sector.GetComponent<Sector>().GenerateSector();

            // on l'ajoute à la liste des secteurs
            sectors.Add(sector);
        }
    }

    // vide le monde
    public void Clear()
    {

        // on détruit tous les enfants du world
        foreach (GameObject sector in sectors)
        {
            Destroy(sector);
        }

        // on vide la liste des secteurs
        sectors.Clear();
    }
}