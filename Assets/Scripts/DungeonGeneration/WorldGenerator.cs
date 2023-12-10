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

    [Header("pre-sectors")]
    [SerializeField] private List<PreSector> preSecteurs = new List<PreSector>();
    // private Dictionary<PreSector,Vector2Int> sectors_pos = new Dictionary<PreSector, Vector2Int>();
    protected Vector2 roomDimensions = new Vector2(8f, 7.5f);

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
        // 1 - on vide le monde
        Clear();


        // 2 - on génère les pré-secteurs
        for (int i = 0; i < nb_sectors; i++)
        {
            // on crée 2 HashSets par secteur (rooms et corridors)
            HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
            HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

            // on remplit les hashsets via sector generator
            GetComponent<SectorGenerator>().GenerateSectorHashSets(ref rooms, ref corridors);

            // on ajoute le pre-secteur à la liste
            preSecteurs.Add(new PreSector(rooms, corridors));
        }

        // 3 - on sépare les secteurs (calcule les positions des futurs secteurs)
        separateSectors();

        // on génère les secteurs
        for (int i = 0; i < preSecteurs.Count; i++)
        {

            // on instancie le secteur
            GameObject sector = Instantiate(sector_prefab, world.transform);


            // on récupère les hashsets
            // HashSet<Vector2Int> rooms = preSecteurs[i].rooms;
            // HashSet<Vector2Int> corridors = preSecteurs[i].corridors;

            // on génère le secteur
            sector.GetComponent<Sector>().GenerateSelf(preSecteurs[i]);

            // on l'ajoute à la liste des secteurs
            sectors.Add(sector);

            // on récupère la position
            Vector3 pos = new Vector3(preSecteurs[i].x, preSecteurs[i].y, 0);

            // on multiplie par la taille des salles
            pos.x *= roomDimensions.x;
            pos.y *= roomDimensions.y;

            // on déplace le secteur
            sector.transform.localPosition = pos;
            print("on deplace le secteur en " + pos);

            // * temporaire * on déplace le secteur
            // sector.transform.position = new Vector3(i * 100, 0, 0);
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

        // on vide la liste des pre-secteurs
        preSecteurs.Clear();
    }



    // TINY KEEP GENERATION
    private void separateSectors()
    {
        // on affiche dans la console les positions et tailles des pre-secteurs
        foreach (PreSector preSec in preSecteurs)
        {
            print("avant sepa : " + preSec.x + " " + preSec.y + " " + preSec.w + " " + preSec.h);
        }

        
        int max_iterations = 10;

        

        // on sépare les secteurs
        for (int iteration = 0; iteration < max_iterations; iteration++)
        {
            int total_collisions = 0;

            foreach (PreSector preSec in preSecteurs)
            {
                // on récupère la séparation
                Vector2Int separation = computeSeparation(preSec,out int nb_collisions);

                print("separation : " + separation);

                // on incrémente le nombre total de collisions
                total_collisions += nb_collisions;

                // on vérifie qu'on a une séparation
                if (separation == new Vector2Int(0, 0)) { continue; }

                // on 


                // on déplace le pre-secteur
                preSec.move(separation);
            }

            print("iteration " + iteration + " : " + total_collisions + " collisions");

            // on vérifie qu'on a pas de collisions
            if (total_collisions == 0) { break; }
            // total_collisions = 0;
        }


        // on affiche dans la console les positions et tailles des pre-secteurs
        foreach (PreSector preSec in preSecteurs)
        {
            print("apres sepa : " + preSec.x + " " + preSec.y + " " + preSec.w + " " + preSec.h);
        }
    }

    private Vector2Int computeSeparation(PreSector agent,out int nb_collisions)
    {
        Vector2 separation = new Vector2();
        nb_collisions = 0;

        // on récupère les secteurs adjacents
        foreach (PreSector sec in preSecteurs)
        {
            // on vérifie que ce n'est pas le même secteur
            if (agent == sec) { continue; }

            // on regarde si les secteurs sont adjacents
            if (agent.isColliding(sec))
            {
                // on ajoute la séparation
                separation.x += agent.cx() - sec.cx();
                separation.y += agent.cy() - sec.cy();

                // on incrémente le nombre d'adjacents
                nb_collisions++;
            }
        }

        // on vérifie qu'on a des adjacents
        if (nb_collisions == 0) { return new Vector2Int(0, 0); }

        // on divise la séparation par le nombre d'adjacents
        separation.x /= nb_collisions;
        separation.y /= nb_collisions;

        // on multiplie la séparation par -1
        separation *= -1;

        // on normalise la séparation
        separation.Normalize();

        // on incrémente le nombre total de collisions
        // total_collisions += nb_collisions;

        // on retourne la séparation
        return new Vector2Int((int) separation.x, (int) separation.y);
    }

}


// pre-sectors
public class PreSector
{
    // variables
    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();



    public int x;
    public int y;

    public int w;
    public int h;

    // dimensions d'une salle
    protected Vector2 roomDimensions = new Vector2(8f, 7.5f);

    // constructor
    public PreSector(HashSet<Vector2Int> rooms, HashSet<Vector2Int> corridors)
    {
        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on récupère les hashsets
        this.rooms = rooms;
        this.corridors = corridors;

        // on calcule la position SW du pre-secteur
        x = tiles.Min(x => x.x);
        y = tiles.Min(x => x.y);

        // on calcule la taille du pre-secteur
        w = tiles.Max(x => x.x) - x + 1;
        h = tiles.Max(x => x.y) - y + 1;

        // on calcule le plafond
        ceiling = CreateCeiling();
    }

    // functions
    public void move(Vector2Int movement)
    {
        x += movement.x;
        y += movement.y;
    }

    public bool isColliding(PreSector other)
    {
        // on vérifie si les secteurs overlappés
        if (x < other.x + other.w &&
            x + w > other.x &&
            y < other.y + other.h &&
            y + h > other.y)
        {
            return true;
        }

        // sinon, ils ne sont pas overlappés
        return false;
    }

    // getters
    public Vector2Int getSize()
    {
        // on retourne la taille
        return new Vector2Int(w, h);
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

    public Vector2 GetCentralWorldPos()
    {
        // on retourne la position centrale à l'échelle du monde
        return new Vector2((cx() * roomDimensions.x), (cy() * roomDimensions.y));
    }


    // utils
    private HashSet<Vector2Int> CreateCeiling()
    {
        // on récupère le plafond
        HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();

        // renvoit un hashet qui est l'inverse des tiles, de dimension w*h
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                ceiling.Add(new Vector2Int(x + i, y + j));
            }
        }

        // on enlève les tiles
        ceiling.ExceptWith(tiles);

        // on retourne le plafond
        return ceiling;
    }

}