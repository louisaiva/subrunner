using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    [Header("WORLD GENERATION")]
    [SerializeField] private int nb_sectors = 1;
    GameObject world;

    [Header("sectors")]
    [SerializeField] private GameObject sector_prefab;
    [SerializeField] private List<Sector> sectors = new List<Sector>();

    [Header("pre-sectors")]
    // [SerializeField] private List<PreSector> sectors = new List<PreSector>();
    // protected Vector2 roomDimensions = new Vector2(8f, 7.5f);
    protected Vector2Int roomDimensionsInTiles = new Vector2Int(16,16);

    [Header("visualisation")]
    [SerializeField] private Transform visu_parent;

    [Header("minimap")]
    [SerializeField] private GameObject minimap;
    [SerializeField] private HashSet<Vector2Int> global_tiles = new HashSet<Vector2Int>();


    // unity     functions
    void Awake()
    {
        // on récupère le sector prefab
        sector_prefab = Resources.Load<GameObject>("prefabs/sectors/base_sector");

        // on récupère le world
        world = GameObject.Find("/world");

        // on récupère le parent des visualisations
        visu_parent = transform.Find("visu");

        // on récupère la minimap
        minimap = GameObject.Find("/perso/minicam");

        // on génère le monde
        GenerateWorld();
    }

    // génère le monde
    public void GenerateWorld()
    {
        Debug.Log("<color=blue>on génère le monde</color>");
        // cmd("on génère le monde");
        // "<color=red>Error: </color>AssetBundle not found"

        // 1 - on vide le monde
        Clear();

        // 2 - on génère les pré-secteurs
        generateSectors();

        // 3 - on crée les visus
        visualizeSectors();

        // 4 - on sépare les pré-secteurs
        separateSectors();

        // on les bascule en full positive
        makeSectorsAllPositives();

        // 5 - on génère les secteurs
        // generateSectors();
        // generateAreas();
        world.GetComponent<World>().GENERATE(sectors);

        // 6 - on génère le Hashset global des tiles
        // generateGlobalTilesHashSet();
    }

    // vide le monde
    public void Clear()
    {

        // on détruit tous les enfants du world
        foreach (Sector sect in sectors)
        {
            Destroy(sect.gameObject);
        }

        // on vide la liste des secteurs
        sectors.Clear();

        // on vide la liste des pre-secteurs
        sectors.Clear();

        // on détruit tous les enfants du parent des visualisations
        foreach (Transform child in visu_parent)
        {
            Destroy(child.gameObject);
        }
    }


    // RANDOM WALK GENERATION
    private void generateSectors()
    {
        for (int i = 0; i < nb_sectors; i++)
        {

            // on crée des hashsets vides
            HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
            HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

            // on remplit les hashsets via sector generator
            GetComponent<SectorGenerator>().GenerateSectorHashSets(ref rooms, ref corridors);

            // on crée le secteur
            GameObject sect = Instantiate(sector_prefab, world.transform);
            sect.name = "sector_" + i;
            sect.GetComponent<Sector>().init(rooms, corridors);

            // on ajoute le pre-secteur à la liste
            sectors.Add(sect.GetComponent<Sector>());
        }
    }


    // STEERING SEPARATION BEHAVIOUR
    private void separateSectors()
    {
        int max_iterations = 500;

        // on sépare les secteurs
        for (int iteration = 0; iteration < max_iterations; iteration++)
        {
            // on sépare les secteurs
            int nb_collisions = separateIteration();

            // on vérifie qu'on a pas de collisions
            if (nb_collisions == 0) { break; }
        }

    }

    private int separateIteration()
    {
        // on récupère le nombre de collisions avant séparation
        int coll = 0;
        
        // on parcourt les pre-secteurs par paire
        for (int i=0;i<sectors.Count-1;i++)
        {
            for (int j=i+1; j<sectors.Count;j++)
            {
                // print("on teste la collision entre "+i+" et "+j);

                // on récupère les pre-secteurs
                Sector sect1 = sectors[i];
                Sector sect2 = sectors[j];

                // on vérifie si les secteurs collident
                if (sect1.isColliding(sect2))
                {
                    // print("collision entre "+i+" et "+j);

                    // on récupère la séparation
                    Vector2 separation = findSeparationVector(sect1, sect2);

                    // on incrémente le nombre total de collisions
                    coll += 1;

                    // on divise la séparation par 2 et on arrondit pour sect1
                    Vector2Int separation_int = new Vector2Int(Mathf.CeilToInt(separation.x/2), Mathf.CeilToInt(separation.y/2));
                    // on déplace le pre-secteur
                    sect1.move(separation_int);

                    // on divise la séparation par 2 et on arrondit pour sect2 (on inverse la séparation)
                    separation_int = new Vector2Int(Mathf.CeilToInt(-separation.x / 2), Mathf.CeilToInt(-separation.y / 2));
                    // on déplace le pre-secteur
                    sect2.move(separation_int);
                }
            }
        }



        return coll;
    }

    private Vector2 findSeparationVector(Sector agent, Sector other)
    {
        Vector2 separation = new Vector2();

        // ON CHERCHE LE VECTEUR DE SEPARATION
        // le plus petit possible (que sur un axe du coup !!)

        // en X
        float x_centers_diff = agent.cx() - other.cx();
        if (x_centers_diff > 0)
        {
            separation.x = other.R() - agent.L();
        }
        else
        {
            separation.x = other.L() - agent.R();
        }

        // en Y
        float y_centers_diff = agent.cy() - other.cy();
        if (y_centers_diff > 0)
        {
            separation.y = other.U() - agent.D();
        }
        else
        {
            separation.y = other.D() - agent.U();
        }

        // on compare quel axe est le plus petit
        if (Mathf.Abs(separation.x) < Mathf.Abs(separation.y))
        {
            separation.y = 0;
        }
        else
        {
            separation.x = 0;
        }

        return separation;

    }

    // VISUALISATION
    private void visualizeSectors()
    {
        // on défini la couleur de base
        Color base_color = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color done_color = new Color(0.5f, 1f, 0.5f, 1f);

        // on parcourt les pre-secteurs
        for (int i = 0; i < sectors.Count; i++)
        {
            // on récupère le pre-secteur
            Sector sect = sectors[i];

            // on créé un visu vide
            GameObject sectVisu = new GameObject("visu_sector_" + i);

            // on lui ajoute un SectorVisualiser
            sectVisu.AddComponent<SectorVisualiser>();

            // on applique les paramètres
            sectVisu.GetComponent<SectorVisualiser>().init(sect, base_color, done_color);

            // on l'ajoute au parent
            sectVisu.transform.SetParent(visu_parent);
        }
    }



    // FINAL GENERATION

    private void makeSectorsAllPositives()
    {
        // on récupère le min
        Vector2Int min = new Vector2Int(100000, 100000);
        foreach (Sector sect in sectors)
        {
            // on récupère la position
            Vector2Int pos = sect.xy();

            // on met à jour le min
            if (pos.x < min.x) { min.x = pos.x; }
            if (pos.y < min.y) { min.y = pos.y; }
        }

        // on déplace tous les secteurs
        foreach (Sector sect in sectors)
        {
            // on récupère la position
            Vector2Int pos = sect.xy();

            // on déplace le secteur
            sect.move(-min);
        }
    }


}