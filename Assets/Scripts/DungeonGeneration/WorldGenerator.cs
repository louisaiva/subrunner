using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;

public class WorldGenerator : MonoBehaviour
{
    [Header("WORLD GENERATION")]
    public bool generate_world = true;
    private bool playtest_enabled = false;
    [SerializeField] private int nb_sectors = 1;
    public bool generate_ceilings = true;
    [SerializeField] private bool is_world_safe = false;
    [SerializeField] private Vector2 generation_radius = new Vector2(5, 5);
    private GameObject world;
    private double creation_time;



    List<Edge> DT = new List<Edge>();
    List<Edge> MST = new List<Edge>();
    List<Edge> loops = new List<Edge>();

    [Header("sectors")]
    private GameObject sector_prefab;
    private GameObject connector_sector_prefab;
    [SerializeField] private List<Sector> sectors = new List<Sector>();
    // [SerializeField] private List<string> hand_made_sectors_names= new List<string>();

    [Header("hand-made sectors")]
    [SerializeField] private List<GameObject> hand_made_sectors = new List<GameObject>();

    [Header("visualisation")]
    [SerializeField] private Transform visu_parent;


    // unity functions
    void Awake()
    {
        // on récupère le sector prefab
        sector_prefab = Resources.Load<GameObject>("prefabs/sectors/base_sector");
        connector_sector_prefab = Resources.Load<GameObject>("prefabs/sectors/base_connector_sector");

        // on récupère le world
        world = GameObject.Find("/world");

        // on récupère le parent des visualisations
        visu_parent = transform.Find("visu");

        // on récupère les hand made sectors
        hand_made_sectors = Resources.LoadAll<GameObject>("prefabs/sectors/hand_made").ToList();

        // on vérifie si PLAYTEST est activé -> si oui on ne génère pas le monde (le playtest est déjà généré)
        GameObject playtest = GameObject.Find("/playtest");
        try
        {
            bool generate = !playtest.activeSelf;
            if (!generate)
            {
                print("PLAYTEST is active -> we don't generate the world");
                generate_world = false;
                playtest_enabled = true;
            }
        }
        catch {}
        

        // on génère le monde
        GenerateWorld();
    }

    // génère le monde
    public void GenerateWorld()
    {
        // on récupère le temps de création
        creation_time = Time.realtimeSinceStartup;
        Debug.Log("<color=blue>on génère le monde</color>");

        // 1 - on vide le monde
        Clear();

        // 2 - on génère les secteurs
        generateSectors();
 
        // 3 - on sépare les secteurs
        separateSectors();

        // 4 - on les bascule en full positive
        makeSectorsAllPositives();

        if (sectors.Count > 1)
        {
            // 5 - on créé un delaunay triangulation
            List<Vector2> vertices = new List<Vector2>();
            foreach (Sector sect in sectors)
            {
                vertices.Add(sect.center());
            }
            if (vertices.Count >= 3)
            {
                DT = createDelanauyTriangulation(vertices);
            }
            else if (vertices.Count == 2)
            {
                DT.Add(new Edge(vertices[0], vertices[1]));
            }

            // 6 - on calcule le minimum spanning tree
            MST = calculateMinimumSpanningTree(vertices, DT);

            // 7 - on ajoute quelques loops
            // loops = addSomeLoops(MST, DT);
            loops = new List<Edge>();

            // 8 - on connecte les secteurs
            connectSectors(MST, loops);

            // 9 - on met à jour les sectors complexes pour ajouter les extensions si pas de voisin
            foreach (Sector sect in sectors)
            {
                if (sect is ComplexeSector)
                {
                    ((ComplexeSector)sect).updateExtensions();
                }
            }
        }

        // on crée des visus
        if (!playtest_enabled) { visualizeSectors(); }

        if (generate_world)
        {
            // on lance la génération du monde
            world.GetComponent<World>().GENERATE(sectors);
        }
        else
        {
            // on cache les secteurs déjà générés
            foreach (Sector sect in sectors)
            {
                sect.gameObject.SetActive(false);
            }
        }

        Debug.Log("<color=blue>world generated in </color>" + (Time.realtimeSinceStartup - creation_time) + " <color=blue>seconds</color>");
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

        // on détruit tous les enfants du parent des visualisations
        foreach (Transform child in visu_parent)
        {
            Destroy(child.gameObject);
        }

        // on vide les listes
        DT.Clear();
        MST.Clear();
        loops.Clear();
    }


    // RANDOM WALK GENERATION
    private void generateSectors()
    {
        // on génère les secteurs random
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
            sect.GetComponent<ProceduralSector>().init(rooms, corridors);

            // on met safe à jour
            sect.GetComponent<ProceduralSector>().is_safe = is_world_safe;

            // on met un skin aléatoire
            string skin = new string[] { "base_sector", "server", "labo","mecha" }[Random.Range(0, 4)];
            sect.GetComponent<ProceduralSector>().setSkin(skin);

            // on donne une position aléatoire au secteur
            Vector2 random_center_pos = new Vector2(Random.Range(-generation_radius.x, generation_radius.x), Random.Range(-generation_radius.y, generation_radius.y));
            sect.GetComponent<Sector>().MoveToSetCenterTo(random_center_pos);


            // on ajoute le secteur à la liste
            sectors.Add(sect.GetComponent<Sector>());
        }

        // on ajoute les secteurs hand made
        for (int i = 0; i < hand_made_sectors.Count; i++)
        {
            // on récupère le secteur
            GameObject sect = hand_made_sectors[i];

            // on crée le secteur
            GameObject sect2 = Instantiate(sect, world.transform);
            sect2.name = "sector_" + (i + nb_sectors) + "_" + sect.name;
            sect2.GetComponent<ComplexeSector>().initHashets();

            // on met un skin aléatoire
            string skin = new string[] { "base_sector", "server", "labo","mecha" }[Random.Range(0, 4)];
            sect2.GetComponent<ComplexeSector>().setSkin(skin);

            // on donne une position aléatoire au secteur
            // Vector2Int random_center_pos = new Vector2Int(Random.Range(-generation_radius, generation_radius), Random.Range(-generation_radius, generation_radius));
            Vector2 random_center_pos = new Vector2(Random.Range(-generation_radius.x, generation_radius.x), Random.Range(-generation_radius.y, generation_radius.y));
            sect2.GetComponent<Sector>().MoveToSetCenterTo(random_center_pos);

            // on ajoute le secteur à la liste
            sectors.Add(sect2.GetComponent<Sector>());
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


    // DELAUNAY TRIANGULATION
    private List<Edge> createDelanauyTriangulation(List<Vector2> positions)
    {

        // on crée une liste de Point
        DelaunatorSharp.IPoint[] points = new DelaunatorSharp.IPoint[positions.Count];
        points = positions.ToPoints();

        // on crée la triangulation
        Delaunator delaunator = new Delaunator(points);

        // on récupère les triangles
        List<int> triangles = delaunator.Triangles.ToList();

        // return triangles;

        // on affiche la liste des triangles
        /* string s="DELAUNAY triangles : \n";
        for (int i=0;i<triangles.Count;i+=3)
        {
            s += triangles[i] + " " + triangles[i+1] + " " + triangles[i+2] + "\n";
        }
        print(s); */

        // on calcule les edges
        List<Edge> edges = new List<Edge>();
        for (int i=0;i<triangles.Count;i+=3)
        {
            // on récupère les points
            Vector2 p1 = positions[triangles[i]];
            Vector2 p2 = positions[triangles[i+1]];
            Vector2 p3 = positions[triangles[i+2]];

            // on crée les edges
            Edge e1 = new Edge(p1, p2);
            Edge e2 = new Edge(p2, p3);
            Edge e3 = new Edge(p3, p1);

            // on les ajoute à la liste si ils n'y sont pas déjà
            bool edge1_in = false;
            bool edge2_in = false;
            bool edge3_in = false;

            foreach (Edge e in edges)
            {
                if (e.isEqual(e1)) { edge1_in = true; }
                if (e.isEqual(e2)) { edge2_in = true; }
                if (e.isEqual(e3)) { edge3_in = true; }
            }
            if (!edge1_in) { edges.Add(e1); }
            if (!edge2_in) { edges.Add(e2); }
            if (!edge3_in) { edges.Add(e3); }
        }

        return edges;
    }

    private List<Edge> calculateMinimumSpanningTree(List<Vector2> vertices,List<Edge> edges)
    {
        // initialisation
        // List<float> weights = new List<float>();
        // Dictionary<int,int> tree = new Dictionary<int, int>();
        List<int> vertices_to_add = vertices.Select((v, i) => i).ToList();
        List<int> added_vertics = new List<int>();
        List<Edge> tree = new List<Edge>();

        // on choisit un sommet au hasard
        int root = Random.Range(0, vertices.Count);
        vertices_to_add.Remove(root);
        added_vertics.Add(root);

        // on parcourt les sommets
        while (vertices_to_add.Count > 0)
        {
            // on cherche les sommets adjacents aux sommets ajoutés
            float min_weight = 10000;
            int min_vertex = -1;
            Edge min_edge = new Edge();
            
            // on parcourt les sommets ajoutés
            foreach (int i in added_vertics)
            {
                // on récupère le vector2
                Vector2 v1 = vertices[i];

                // on récupère les edges qui contiennent ce sommet
                foreach (Edge e in edges)
                {
                    if (e.isPointIn(v1))
                    {
                        // on ajoute le sommet adjacent
                        int adj = vertices.IndexOf(e.otherPoint(v1));
                        if (vertices_to_add.Contains(adj))
                        {
                            // on récupère le poids de l'edge
                            float weight = e.weight;
                            if (weight < min_weight)
                            {
                                min_weight = weight;
                                min_edge = e;
                                min_vertex = adj;
                            }
                        }
                    }
                }
            }

            // on ajoute le sommet au MST
            tree.Add(min_edge);

            // on ajoute le sommet à la liste des sommets ajoutés
            vertices_to_add.Remove(min_vertex);
            added_vertics.Add(min_vertex);
        }

        // on return
        return tree;



        /* // on parcourt les sommets
        for (int i=0;i<vertices.Count;i++)
        {
            // on récupère les sommets
            Vector2 v1 = vertices[i];
         
            float min_dist = 10000;
            int min_edge = -1;
            for (int j=0;j<vertices.Count;j++)
            {
                if (i == j) { continue; }
                Vector2 v2 = vertices[j];

                // on calcule la distance
                float dist = Vector2.Distance(v1, v2);
                if (dist < min_dist) {
                    min_dist = dist;
                    min_edge = j;
                }
            }

            // on ajoute la distance minimale
            cheapest_cost.Add(min_dist);
            tree.Add(i, min_edge);
        }

        // y'a une entrée de trop dans tree parce qu'on a X vertices et qu'il y a X-1 edges
        int root_to_delete = -1;
        foreach (KeyValuePair<int,int> entry in tree)
        {
            foreach (KeyValuePair<int, int> entry2 in tree)
            {
                if (entry.Key == entry2.Value && entry.Value == entry2.Key)
                {
                    root_to_delete = entry.Key;
                    break;
                }
            }
        }

        // on supprime l'entrée
        tree.Remove(root_to_delete);

        // on affiche le minimum spanning tree
        string s = "MINIMUM SPANNING TREE : \n";
        foreach (KeyValuePair<int, int> entry in tree)
        {
            s += entry.Key + " " + entry.Value + "\n";
        }
        print(s); */

    }

    private List<Edge> addSomeLoops(List<Edge> mst, List<Edge> dt, float percentage=0.1f)
    {
        // on ajoute quelques edges qui ne sont pas déjà dans le MST
        List<Edge> loops = new List<Edge>();

        // on parcourt les edges de la DT
        foreach (Edge edge in dt)
        {
            // on vérifie que l'edge n'est pas déjà dans le MST
            bool in_mst = false;
            foreach (Edge mst_edge in mst)
            {
                if (edge.isEqual(mst_edge))
                {
                    in_mst = true;
                    break;
                }
            }

            // si l'edge n'est pas dans le MST
            if (!in_mst)
            {
                // on teste un random pour savoir si on l'ajoute
                float random = Random.Range(0f, 1f);
                if (random < percentage)
                {
                    loops.Add(edge);
                }
            }
        }

        return loops;
    }



    // VISUALISATION
    private void visualizeSectors()
    {
        // on clear le parent des visualisations
        foreach (Transform child in visu_parent)
        {
            Destroy(child.gameObject);
        }

        // on défini la couleur de base
        Color base_color = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color done_color = new Color(0.5f, 1f, 0.5f, 1f);
        Color complexe_color = new Color(0.5f, 0.5f, 1f, 1f);

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
            if (sectors[i] is ComplexeSector)
            {
                sectVisu.GetComponent<SectorVisualiser>().init(sect, complexe_color, done_color);
            }
            else
            {
                sectVisu.GetComponent<SectorVisualiser>().init(sect, base_color, done_color);
            }

            // on l'ajoute au parent
            sectVisu.transform.SetParent(visu_parent);
        }

        // on visualize le MST

        // on récupère les positions
        /* List<Vector2> positions = new List<Vector2>();
        foreach (Sector sect in sectors)
        {
            positions.Add(sect.center());
        } */

        if (MST.Count == 0) { return; }

        // on instancie un MST_visu
        GameObject MST_visu = new GameObject("MST_visu");
        MST_visu.AddComponent<MST_visu>();
        MST_visu.GetComponent<MST_visu>().init(MST, loops);


        // on l'ajoute au parent
        MST_visu.transform.SetParent(visu_parent);
    }


    // SECTOR CONNECTION
    private void connectSectors(List<Edge> mst, List<Edge> loops)
    {
        // connects the sectors together as defined by the MST and the loops

        print(Vector2Int.up + " / " + Vector2Int.down + " / " + Vector2Int.left + " / " + Vector2Int.right);

        // for now we only do the MST
        foreach (Edge edge in mst)
        {
            // on récupère les secteurs
            Sector sect1 = getSectorByCenter(edge.p1);
            Sector sect2 = getSectorByCenter(edge.p2);

            // on vérifie sur quel axe s'effectue la frontière
            string border = sect1.getBorder(sect2);
            print("(WorldGenerator - connectSectors)" + sect1.gameObject.name + " and " + sect2.gameObject.name + " are connecting via border : " + border);

            if (!(new string[] {"no border","collision"}.Contains(border)))
            {
                if (new string[] { "R", "U" }.Contains(border))
                {
                    sect1.connectWithSector(sect2);
                }
                else
                {
                    sect2.connectWithSector(sect1);
                }
            }
            else if (border == "no border")
            {
                // cela signifie que les secteurs sont séparés par du vide
                // on rajoute un petit ConnectorSector entre les deux
                
                // on récupère la frontière (qui contient du vide)
                if (sect1.L() > sect2.R() || sect2.L() > sect1.R())
                {
                    // dans ce cas on a une frontière horizontale
                    Sector gauche = (sect1.L() > sect2.R()) ? sect2 : sect1;
                    Sector droite = (sect1.L() > sect2.R()) ? sect1 : sect2;

                    // on récup la hauteur où on va créer le ConnectorSector
                    int max_y = Mathf.Min(gauche.U(), droite.U());
                    int min_y = Mathf.Max(gauche.D(), droite.D());
                    int y = Random.Range(min_y, max_y);

                    // on crée le ConnectorSector
                    GameObject sect = Instantiate(connector_sector_prefab, world.transform);
                    sect.name = "connector_sector_" + sect1.gameObject.name + "_" + sect2.gameObject.name;
                    sect.GetComponent<ConnectorSector>().init(gauche.R(), y, droite.L() - gauche.R(), 1);

                    // on ajoute le secteur à la liste
                    sectors.Add(sect.GetComponent<Sector>());

                    // on connecte les secteurs
                    gauche.connectWithSector(sect.GetComponent<Sector>());
                    sect.GetComponent<Sector>().connectWithSector(droite);
                }
                else if (sect1.D() > sect2.U() || sect2.D() > sect1.U())
                {
                    // dans ce cas on a une frontière verticale
                    Sector bas = (sect1.D() > sect2.U()) ? sect2 : sect1;
                    Sector haut = (sect1.D() > sect2.U()) ? sect1 : sect2;

                    // on récup la largeur où on va créer le ConnectorSector
                    int max_x = Mathf.Min(bas.R(), haut.R());
                    int min_x = Mathf.Max(bas.L(), haut.L());
                    int x = Random.Range(min_x, max_x);

                    // on crée le ConnectorSector
                    GameObject sect = Instantiate(connector_sector_prefab, world.transform);
                    sect.name = "connector_sector_" + sect1.gameObject.name + "_" + sect2.gameObject.name;
                    sect.GetComponent<ConnectorSector>().init(x, bas.U(), 1, haut.D() - bas.U());

                    // on ajoute le secteur à la liste
                    sectors.Add(sect.GetComponent<Sector>());

                    // on connecte les secteurs
                    bas.connectWithSector(sect.GetComponent<Sector>());
                    sect.GetComponent<Sector>().connectWithSector(haut);
                }
                else
                {
                    // dans ce cas on a une frontière en diagonale
                    // -> COMPLEXE -> il faut créer 2 ConnectorSectors
                    // risque d'empieter sur un autre secteur existant -> on ne fait rien
                    Debug.LogWarning("(WorldGenerator - connectSectors) diagonal border between " + sect1.gameObject.name + " and " + sect2.gameObject.name + " -> COMPLEXE -> we do nothing");
                }
                    
            }
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


    // GETTERS
    public Sector getSectorByCenter(Vector2 pos)
    {
        // on parcourt tous les sectors pour trouver celui qui correspond
        foreach (Sector sect in sectors)
        {
            // on vérifie si le sect correspond
            if (sect.center() == pos)
            {
                return sect;
            }
        }
        return null;
    }
}

public class Edge
{
    public Vector2 p1;
    public Vector2 p2;
    public float weight;

    public Edge()
    {
        this.p1 = new Vector2();
        this.p2 = new Vector2();
        this.weight = 0;
    }

    public Edge(Vector2 p1, Vector2 p2)
    {
        if (p1.x < p2.x)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
        else if (p1.x == p2.x)
        {
            if (p1.y < p2.y)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
            else
            {
                this.p1 = p2;
                this.p2 = p1;
            }
        }
        else
        {
            this.p1 = p2;
            this.p2 = p1;
        }
        this.weight = Vector2.Distance(p1, p2);
    }


    // GETTERS

    public bool isPointIn(Vector2 point)
    {
        if (point == this.p1 || point == this.p2) { return true; }
        return false;
    }

    public bool isEqual(Edge other)
    {
        if (this.p1 == other.p1 && this.p2 == other.p2) { return true; }
        if (this.p1 == other.p2 && this.p2 == other.p1) { return true; }
        return false;
    }

    public Vector2 otherPoint(Vector2 point)
    {
        if (point == this.p1) { return this.p2; }
        if (point == this.p2) { return this.p1; }
        return new Vector2();
    }

}