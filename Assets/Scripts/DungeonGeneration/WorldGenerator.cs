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

    [Header("visualisation")]
    [SerializeField] private Transform visu_parent;

    // unity functions
    void Awake()
    {
        // on récupère le sector prefab
        sector_prefab = Resources.Load<GameObject>("prefabs/sectors/sector_4");

        // on récupère le world
        world = GameObject.Find("/world");

        // on récupère le parent des visualisations
        visu_parent = transform.Find("visu");

        // on génère le monde
        GenerateWorld();
    }

    // génère le monde
    public void GenerateWorld()
    {
        // 1 - on vide le monde
        Clear();

        // 2 - on génère les pré-secteurs
        generatePreSectors();

        // 3 - on crée les visus
        visualizePreSectors();

        // 4 - on sépare les pré-secteurs
        StartCoroutine(separatePreSectors());
    }

    public void ConstructWorld()
    {
        // 5 - on génère les secteurs
        print("GENERATIONNNNNNNNNNNNNNNNNNNNNNNNNN");
        // generateSectors();
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

        // on détruit tous les enfants du parent des visualisations
        foreach (Transform child in visu_parent)
        {
            Destroy(child.gameObject);
        }
    }


    // RANDOM WALK GENERATION
    private void generatePreSectors()
    {
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
    }



    // STEERING SEPARATION BEHAVIOUR
    private IEnumerator separatePreSectors()
    {
        int max_iterations = 500;

        // on sépare les secteurs
        for (int iteration = 0; iteration < max_iterations; iteration++)
        {
            // on sépare les secteurs
            int nb_collisions = separateIteration();

            // on affiche dans la console le nombre de collisions
            print("iteration " + iteration + " : " + nb_collisions + " collisions");

            // on vérifie qu'on a pas de collisions
            if (nb_collisions == 0) { break; }

            // on attend une demiseconde
            yield return new WaitForSeconds(0.05f);
        }

    }

    private int separateIteration()
    {
        // on récupère le nombre de collisions avant séparation
        int coll = 0;
        
        // on parcourt les pre-secteurs par paire
        for (int i=0;i<preSecteurs.Count-1;i++)
        {
            for (int j=i+1; j<preSecteurs.Count;j++)
            {
                print("on teste la collision entre "+i+" et "+j);

                // on récupère les pre-secteurs
                PreSector preSec1 = preSecteurs[i];
                PreSector preSec2 = preSecteurs[j];

                // on vérifie si les secteurs collident
                if (preSec1.isColliding(preSec2))
                {
                    print("collision entre "+i+" et "+j);

                    // on récupère la séparation
                    Vector2 separation = findSeparationVector(preSec1, preSec2);

                    // on incrémente le nombre total de collisions
                    coll += 1;

                    // on divise la séparation par 2 et on arrondit pour preSec1
                    Vector2Int separation_int = new Vector2Int(Mathf.CeilToInt(separation.x/2), Mathf.CeilToInt(separation.y/2));
                    // on déplace le pre-secteur
                    preSec1.move(separation_int);

                    // on divise la séparation par 2 et on arrondit pour preSec2 (on inverse la séparation)
                    separation_int = new Vector2Int(Mathf.CeilToInt(-separation.x / 2), Mathf.CeilToInt(-separation.y / 2));
                    // on déplace le pre-secteur
                    preSec2.move(separation_int);
                }
            }
        }



        return coll;
    }

    private Vector2 findSeparationVector(PreSector agent, PreSector other)
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
    private void visualizePreSectors()
    {
        // on défini la couleur de base
        Color base_color = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color done_color = new Color(0.5f, 1f, 0.5f, 1f);

        // on parcourt les pre-secteurs
        for (int i = 0; i < preSecteurs.Count; i++)
        {
            // on récupère le pre-secteur
            PreSector preSec = preSecteurs[i];

            // on créé un visu vide
            GameObject preSecVisu = new GameObject("visu_sector_" + i);

            // on lui ajoute un PreSectorVisualiser
            preSecVisu.AddComponent<PreSectorVisualiser>();

            // on applique les paramètres
            preSecVisu.GetComponent<PreSectorVisualiser>().init(preSec, base_color, done_color);

            // on l'ajoute au parent
            preSecVisu.transform.SetParent(visu_parent);
        }
    }



    // FINAL GENERATION
    private void generateSectors()
    {
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
        // on vérifie si les secteurs collident
        if (other.L() >= R()) { return false; }
        if (other.R() <= L()) { return false; }
        if (other.D() >= U()) { return false; }
        if (other.U() <= D()) { return false; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs collident
        return true;
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