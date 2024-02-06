using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ComplexeSector : Sector
{
    [Header("Hand Made Sector")]
    [SerializeField] private Tilemap fg_tm;
    [SerializeField] private Tilemap bg_tm;
    [SerializeField] private Tilemap gd_tm; 
    [SerializeField] private Vector2Int area_start; // position de la première handmade area (en bas à gauche)
    [SerializeField] private Vector2Int wh_handmade; // taille de la zone handmade (en area)
    [SerializeField] private List<Vector2Int> handmade_connectors_pos; // position de la handmade area qui connecte avec les corridors situés sur le périmètre du secteur (en tiles)
    [SerializeField] private List<Vector2Int> handmade_connectors_direction; // direction de la connexion (ex: connecte via le haut du secteur -> handmade_connector_direction = (0,1))


    [Header("spawn position")]
    [SerializeField] private bool is_spawn_sector = false;
    [SerializeField] private Vector2 spawn_pos;

    // unity functions
    new void Awake()
    {
        base.Awake();

        // on récupère les tilemaps
        bg_tm = transform.Find("bg_sector").gameObject.GetComponent<Tilemap>();
        fg_tm = transform.Find("fg_sector").gameObject.GetComponent<Tilemap>();
        gd_tm = transform.Find("gd_sector").gameObject.GetComponent<Tilemap>();
    }


    // init functions
    public void initHashets()
    {
        // on remplit les hashsets en fonction des tilemaps et d'area_size
        bg_tm.CompressBounds();
        fg_tm.CompressBounds();
        gd_tm.CompressBounds();

        // on récupère le point max de chaque tilemap
        float bg_max_x = bg_tm.localBounds.max.x;
        float bg_max_y = bg_tm.localBounds.max.y;
        float fg_max_x = fg_tm.localBounds.max.x;
        float fg_max_y = fg_tm.localBounds.max.y;
        float gd_max_x = gd_tm.localBounds.max.x;
        float gd_max_y = gd_tm.localBounds.max.y;

        // et donc le point max du secteur
        float max_x = Mathf.Max(bg_max_x, fg_max_x, gd_max_x);
        float max_y = Mathf.Max(bg_max_y, fg_max_y, gd_max_y);

        // print("(ComplexeSector) max_x : " + max_x + " max_y : " + max_y);

        // on multiplie par 2 pour avoir la taille du secteur en tiles (chaque tile fait 0.5 unité)
        float w_tiles = max_x*2;
        float h_tiles = max_y*2;

        // print("(ComplexeSector) w_tiles : " + w_tiles + " h_tiles : " + h_tiles);

        // on divise par area_size pour avoir le nombre d'area
        int w_area = Mathf.CeilToInt(w_tiles / area_size.x);
        int h_area = Mathf.CeilToInt(h_tiles / area_size.y);
        wh_handmade = new Vector2Int(w_area, h_area);

        // print("(ComplexeSector) w_area : " + w_area + " h_area : " + h_area + " / w_area : " + w_tiles / area_size.x + " h_area : " + h_tiles / area_size.y);

        // on ajoute les tiles dans le hashset en partant de 1,1
        // -> comme ça on a un hashet avec un contour de 1 tile dans toutes les directions
        for (int i = 1; i < w_area+1; i++)
        {
            for (int j = 1; j < h_area+1; j++)
            {
                rooms.Add(new Vector2Int(i, j));
            }
        }


        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on calcule la position SW du secteur
        x = 0;
        y = 0;

        // on calcule la taille du secteur avec le contour de 1 tile
        w = w_area +2;
        h = h_area +2;

        // on calcule la position de la première handmade area
        area_start = new Vector2Int(1, 1);

    }

    public override void initAreas()
    {
        // on crée des areas pour chaque tile
        foreach (Vector2Int tile in getGeneratedAreas())
        {
            // on récup le type de l'area
            string type = getAreaName(tile);

            // on crée l'area
            GameObject area_go = Instantiate(area_prefab, Vector3.zero, Quaternion.identity);
            Area area = area_go.GetComponent<Area>();
            area.init(tile.x, tile.y, area_size.x, area_size.y, type, this);


            // on lui donne une zone ou pas
            if (rooms.Contains(tile) && Random.Range(0f, 1f) < .1f)
            {
                // on lui donne une zone aléatoire
                area.setZone(zones[Random.Range(0, zones.Count)]);
            }

            // on ajoute l'area au dictionnaire
            areas[tile] = area;
        }
    }


    public void LAUNCH()
    {
        // we delete our tilemaps
        Destroy(bg_tm.gameObject);
        Destroy(fg_tm.gameObject);
        Destroy(gd_tm.gameObject);

        // we move the sector to the right position
        transform.position = new Vector3(((x +area_start.x)* area_size.x)/2, ((y +area_start.y)* area_size.y)/2, 0);
    }

    public void updateExtensions()
    {
        // ici on vérifie si les connections internes sont bien connectées
        // si une connection interne n'est pas connectée, on ajoute l'extension appropriée

        // on récupère les positions des connections internes
        for (int i=0;i<handmade_connectors_pos.Count;i++)
        {
            Vector2Int connector_pos = handmade_connectors_pos[i];
            Vector2Int connector_dir = handmade_connectors_direction[i];

            // on vérifie si une tile est présente à la position de la connection
            Vector2Int tile_pos = connector_pos + connector_dir;
            if (tiles.Contains(tile_pos)) { continue; }

            // on ajoute une extension
            addExtension(tile_pos);
        }
    }

    protected void addExtension(Vector2Int pos)
    {
        // dans un premier temps on ajoute juste une salle de 1x1
        // -> on verra plus tard pour ajouter des salles déjà générées (extension)
        corridors.Add(pos);
        tiles.Add(pos);
    }


    // SECTOR CONNECTIONS
    public override Vector2Int findClosestInsideConnectingArea(Sector other, string border = "")
    {
        // on récupère la position la plus proche de la handmade area
        Vector2Int other_center = new Vector2Int((int) other.cx(), (int) other.cy());
        return findClosestConnectingArea(other_center);
    }

    public override Vector2Int findClosestConnectingArea(Vector2Int area)
    {
        // verify is there is a handmade area
        if (handmade_connectors_pos.Count == 0)
        {
            Debug.LogError("(ComplexeSector - findClosestConnectingArea) Erreur : pas de handmade area");
            return base.findClosestConnectingArea(area);
        }

        // returns the closest corridor connected to the handmade area
        float min_dist = 100000;
        Vector2Int closest_area = handmade_connectors_pos[0];

        // on parcourt les tiles
        // foreach (Vector2Int pos in handmade_connectors_pos)
        for (int i=0; i < handmade_connectors_pos.Count; i++)
        {

            // on récupère la position du corridor
            Vector2Int handmade_area_pos = handmade_connectors_pos[i];
            Vector2Int handmade_area_dir = handmade_connectors_direction[i];
            Vector2Int pos = handmade_area_pos + handmade_area_dir;
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

        print("(ComplexeSector - findClosestConnectingArea) on a trouvé la position " + closest_area + " avec une distance de " + min_dist);

        // on retourne l'area la plus proche
        return closest_area;
    }

    protected override List<Vector2Int> createPath(Vector2Int start, Vector2Int end)
    {
        // finds the closest path to connect the 2 areas
        // WITHOUT disturbing the handmade areas
        // -> we don't want to destroy the handmade areas
        // we connect the handmade areas & new corridors at the handmade_connector room

        // ! attention handmade_connector_pos doit avoir un accès sur le périmètre du secteur

        // on crée un path entre deux areas dans le secteur
        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on crée un path
        List<Vector2Int> path = new List<Vector2Int>();

        // on vérifie si le start est dans le secteur
        bool is_start = collidesWithRoomPoint(start);

        // on ajoute le start
        path.Add(start);

        Vector2Int last_last_pos = new Vector2Int(-1, -1);

        print("(ComplexeSector - createPath) on crée un path entre " + start + " et " + end + " / start appartient au secteur : " + is_start);

        int nb_tours = 100;
        // on boucle tant qu'on a pas atteint la fin
        while (path.Last() != end && nb_tours > 0)
        {
            // on vérifie qu'on fait pas inlassablement des tours de boucle
            Vector2Int last_pos = path.Last();
            if (last_pos == last_last_pos)
            {
                Debug.LogError("(ComplexeSector - createPath) Erreur dans la création du path");
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
                    best_direction = direction;
                    print("(ComplexeSector - createPath) on a atteint la fin en " + path.Count + " tours et à la position " + adjacentPosition + " avec la direction " + best_direction);
                    break;
                }

                // on vérifie si la position adjacente n'est pas une handmade area
                if (isGlobalHandmadeArea(adjacentPosition))
                {
                    continue;
                }

                // on vérifie que la position trouvée n'est pas déjà dans le path
                if (path.Contains(adjacentPosition))
                {
                    continue;
                }


                // on calcule la distance
                float dist = Vector2Int.Distance(adjacentPosition, end);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    best_direction = direction;
                }
            }

            // on vérifie que la position trouvée n'est pas déjà dans le path
            /* if (path.Contains(last_pos + best_direction))
            {
                Debug.LogError("(ComplexeSector - createPath) Erreur : on a déjà cette position dans le path");
                break;
            } */

            // on ajoute la direction au path
            path.Add(last_pos + best_direction);

            // on décrémente le nombre de tours
            nb_tours--;
        }

        if (nb_tours <= 0)
        {
            Debug.LogWarning("(ComplexeSector - createPath) Erreur : on a pas atteint la fin du path");
        }

        // on retourne le path
        return path;
    }


    // SECTOR GETTERS
    public override string getAreaType(Vector2Int pos)
    {
        if (pos.x >= area_start.x && pos.x < area_start.x + wh_handmade.x && pos.y >= area_start.y && pos.y < area_start.y + wh_handmade.y)
        {
            return "handmade";
        }
        string area_type = base.getAreaType(pos);
        // print("(ComplexeSector - getAreaType) area_type : " + area_type);
        return area_type;
    }

    public override Dictionary<Vector2Int, string> getAreaNeighborsType(Vector2Int pos)
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

            if (type == "ceiling") { continue; }
            
            // on vérifie que la position adjacente n'est pas une handmade area non connectable
            if (isLocalHandmadeArea(adjacentPosition))
            {
                if (!handmade_connectors_pos.Contains(adjacentPosition)) { continue; }

                // si on est ici, alors adjacentPosition est une handmade area connectable
                // on vérifie si les connections de cette handmade area sont dans la bonne direction
                bool found_connection = false;
                for (int i=0;i<handmade_connectors_pos.Count;i++)
                {
                    Vector2Int connector_pos = handmade_connectors_pos[i];

                    // on vérifie qu'on regarde bien adjacentPosition
                    if (connector_pos != adjacentPosition) { continue; }
                    
                    Vector2Int connector_dir = handmade_connectors_direction[i];

                    // on l'ajoute si "pos" est le corridor qui connecte la handmade area dans la bonne direction
                    if (connector_pos + connector_dir == pos)
                    {
                        found_connection = true;
                        break;
                    }

                }/* 

                Vector2Int connector_pos = handmade_connectors_pos[handmade_connectors_pos.IndexOf(adjacentPosition)];
                Vector2Int connector_dir = handmade_connectors_direction[handmade_connectors_pos.IndexOf(adjacentPosition)]; */

                // on l'ajoute si "pos" est le corridor qui connecte la handmade area dans la bonne direction
                if (!found_connection) { continue; }
            }

            // on ajoute la direction de la room
            neighbors.Add(direction, type);
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

    // GETTERS
    public List<Tilemap> GetTilemaps()
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        tilemaps.Add(fg_tm);
        tilemaps.Add(bg_tm);
        tilemaps.Add(gd_tm);
        return tilemaps;
    }
    
    public TileBase[] getTiles(string tm = "bg")
    {
        BoundsInt bounds = new BoundsInt(0,0, 0, w * area_size.x, h * area_size.y, 1);
        // on récupère les tiles
        if (tm == "bg")
        {
            return bg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "fg")
        {
            return fg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "gd")
        {
            return gd_tm.GetTilesBlock(bounds);
        }

        return null;
    }
    public TileBase[] getHandmadeTiles(string tm = "bg")
    {
        BoundsInt bounds = new BoundsInt(0, 0, 0, wh_handmade.x * area_size.x, wh_handmade.y * area_size.y, 1);
        // on récupère les tiles
        if (tm == "bg")
        {
            return bg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "fg")
        {
            return fg_tm.GetTilesBlock(bounds);
        }
        else if (tm == "gd")
        {
            return gd_tm.GetTilesBlock(bounds);
        }

        return null;
    }

    public bool isSpawnSector()
    {
        return is_spawn_sector;
    }

    public Vector2Int getAreaStart()
    {
        return area_start;
    }

    public Vector2 getSpawnPos()
    {
        return new Vector2(spawn_pos.x + ((x + area_start.x) * area_size.x)/2f, spawn_pos.y + ((y + area_start.y) * area_size.y)/2f);
    }

    public BoundsInt getHandmadeBounds()
    {
        return new BoundsInt((x + area_start.x) * area_size.x, (y + area_start.y) * area_size.y, 0, wh_handmade.x * area_size.x, wh_handmade.y * area_size.y, 1);
    }

    public bool isGlobalHandmadeArea(Vector2Int pos)
    {
        // on vérifie si on est dans la handmade area
        if (pos.x >= area_start.x + x && pos.x < area_start.x + x + wh_handmade.x && pos.y >= area_start.y + y && pos.y < area_start.y + y + wh_handmade.y)
        {
            return true;
        }
        return false;
    }

    public bool isLocalHandmadeArea(Vector2Int pos)
    {
        // on vérifie si on est dans la handmade area
        if (pos.x >= area_start.x && pos.x < area_start.x + wh_handmade.x && pos.y >= area_start.y && pos.y < area_start.y + wh_handmade.y)
        {
            return true;
        }
        return false;
    }

    public HashSet<Vector2Int> getGeneratedAreas()
    {
        // on récupère les areas générées
        HashSet<Vector2Int> generated_areas = new HashSet<Vector2Int>();
        foreach (Vector2Int pos in tiles)
        {
            if (!isLocalHandmadeArea(pos))
            {
                generated_areas.Add(pos);
            }
        }
        return generated_areas;
    }
}