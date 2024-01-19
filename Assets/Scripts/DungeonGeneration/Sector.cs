using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


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


    // UNITY METHODS
    protected void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();
        world_generator = GameObject.Find("/generator").GetComponent<WorldGenerator>();
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

    // MAIN FUNCTIONS
    public void move(Vector2Int movement)
    {
        x += movement.x;
        y += movement.y;
    }

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

    // CONNECTIONS
    public void connectWithSector(Sector other)
    {
        // créé le plus petit chemin entre les deux secteurs

        // trouve l'area la plus proche de l'autre secteur
        Vector2Int closest_area = new Vector2Int(0, 0);

        // on trouve la frontière entre les secteurs
        string border = getBorder(other);

        // on vérifie si les secteurs sont collés
        if (border == "R")
        {
            // on trouve l'area la plus proche de l'autre secteur par la droite
            float min_dist = 100000;
            for (int y_area = D(); y_area < U(); y_area++)
            {
                Vector2Int tile = new Vector2Int(R()-2, y_area);

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
                Vector2Int tile = new Vector2Int(x_area, U()-2);

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
            Debug.LogError("Les secteurs collisionnent");
            return;
        }
        else if (border == "no border")
        {
            Debug.LogError("Les secteurs ne sont pas voisins");
            return;
        }
        else
        {
            Debug.LogError("Erreur dans la recherche de la frontière entre les secteurs");
            return;
        }


        // on récupère la tile de l'autre secteur la plus proche de l'area
        Vector2Int closest_area_other = other.findClosestArea(closest_area);

        print(gameObject.name + " : " + closest_area + " / " + other.gameObject.name + " : " + closest_area_other);

        // on choisit un sas dans notre secteur qui est sur la border
        Vector2Int sas = closest_area;
        Vector2Int other_sas = closest_area_other;
        if (border == "R")
        {

            // on cherche les dimensions de la frontière partagée
            int border_D = Mathf.Max(D(), other.D());
            int border_U = Mathf.Min(U(), other.U());

            // on choisit un sas random dans la frontière
            sas = new Vector2Int(R()-1, Random.Range(border_D, border_U));

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(other.L(), sas.y);
        }
        else if (border == "U")
        {
            // on cherche les dimensions de la frontière partagée
            int border_L = Mathf.Max(L(), other.L());
            int border_R = Mathf.Min(R(), other.R());

            // on choisit un sas random dans la frontière
            sas = new Vector2Int(Random.Range(border_L, border_R), U()-1);

            // on choisit un sas dans l'autre secteur
            other_sas = new Vector2Int(sas.x, other.D());
        }
        else
        {
            Debug.LogError("Erreur dans la recherche des sas entre les secteurs");
            return;
        }


        // on crée un path entre les deux areas qui est obligatoirement compris dans les 2 secteurs
        List<Vector2Int> path = createPath(closest_area, sas);
        List<Vector2Int> other_path = other.createPath(other_sas, closest_area_other);

        // on ajoute les paths
        addPath(path);
        other.addPath(other_path);

        
        string s= "path :";
        foreach (Vector2Int pos in path)
        {
            s += " " + pos;
        }
        foreach (Vector2Int pos in other_path)
        {
            s += " " + pos;
        }
        print(s);

        // on ajoute le path aux secteurs
        // Vector2Int sas = addPath(path);
        // Vector2Int other_sas = other.addPath(path);

        // todo on ajoute un sas à la fin
        // sas.Add(path.Last());
        // Vector2Int sas = path.Last();

        // on ajoute une connection
        addConnection(sas, other_sas);
        other.addConnection(other_sas, sas);
    }

    public Vector2Int findClosestArea(Vector2Int area)
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

    private List<Vector2Int> createPath(Vector2Int start, Vector2Int end)
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
                Debug.LogError("Erreur dans la création du path");
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

    public Vector2Int addPath(List<Vector2Int> path)
    {

        // on renvoit la position du sas
        Vector2Int sas = new Vector2Int(-1000, -1000);

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

            if (!is_start && sas == new Vector2Int(-1000, -1000))
            {
                // la position actuelle est notre sas
                sas = path[i];
            }
            else if (is_start)
            {
                sas = path[i];
            }

            // on modifie la tile
            Vector2Int tile = path[i];
            tile.x -= x;
            tile.y -= y;

            // on ajoute un corridor si ce n'est pas déjà une room
            if (!rooms.Contains(tile))
            {
                corridors.Add(tile);
                tiles.Add(tile);
            }
            else if (i == 0 || i == path.Count - 1)
            {
                // on ajoute un sas
                rooms.Remove(tile);
                corridors.Add(tile);
            }
        }

        return sas;
    }

    public void addConnection(Vector2Int start, Vector2Int end)
    {
        Vector2Int tile = new Vector2Int(start.x - x, start.y - y);

        // on ajoute une connection
        if (!connections.ContainsKey(tile))
        {
            connections.Add(tile,new List<Vector2Int>());
        }
        connections[tile].Add(end - start);
        // print("connection added : " + start + " / " + end + " / " + (end - start));
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
        // pos en area

        string type = getAreaType(pos);
        if (type == "ceiling") { return "ceiling"; }
        if (type == "handmade") { return "handmade"; }

        // on récupère les noms des salles
        string name = type + "_";

        // on récupère le voisinage
        HashSet<Vector2Int> room_directions;
        HashSet<Vector2Int> corr_directions;
        getAreaNeighbors(pos, out room_directions, out corr_directions);

        if (type == "room")
        {
            // on concatène les directions
            HashSet<Vector2Int> directions = new HashSet<Vector2Int>();
            directions.UnionWith(room_directions);
            directions.UnionWith(corr_directions);

            // on parcourt les directions des rooms puis on ajoute le nom de la direction
            if (directions.Contains(Vector2Int.up)) { name += "U"; }
            if (directions.Contains(Vector2Int.down)) { name += "D"; }
            if (directions.Contains(Vector2Int.left)) { name += "L"; }
            if (directions.Contains(Vector2Int.right)) { name += "R"; }
        }
        else if (type == "corridor")
        {
            // on concatène les directions
            HashSet<Vector2Int> directions = new HashSet<Vector2Int>();
            directions.UnionWith(room_directions);
            directions.UnionWith(corr_directions);

            // on parcourt les directions des rooms puis on ajoute le nom de la direction
            if (directions.Contains(Vector2Int.up)) { name += "U"; }
            if (directions.Contains(Vector2Int.down)) { name += "D"; }
            if (directions.Contains(Vector2Int.left)) { name += "L"; }
            if (directions.Contains(Vector2Int.right)) { name += "R"; }
        }

        // on retourne le nom
        return name;
    }

    public void getAreaNeighbors(Vector2Int pos, out HashSet<Vector2Int> room_directions, out HashSet<Vector2Int> corr_directions)
    {
        // on crée un hashset d'areas similaires adjacentes
        room_directions = new HashSet<Vector2Int>();
        corr_directions = new HashSet<Vector2Int>();

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on parcourt les directions dans le secteur
        foreach (var direction in directions)
        {
            // on récupère la position adjacente
            Vector2Int adjacentPosition = pos + direction;

            // on vérifie que la position adjacente est une salle
            if (rooms.Contains(adjacentPosition))
            {
                // on ajoute la direction de la room
                room_directions.Add(direction);
            }
            else if (corridors.Contains(adjacentPosition))
            {
                // on ajoute la direction du corridor
                corr_directions.Add(direction);
            }
        }

        // on vérifie les connections
        if (connections.ContainsKey(pos))
        {
            // on ajoute toutes les directions de la connection
            foreach (Vector2Int direction in connections[pos])
            {
                corr_directions.Add(direction);
            }

            // print("connection found : " + pos + " / " + connections[pos]);
        }

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

    public string getBorder(Sector other)
    {
        if (isColliding(other)) { return "collision"; }

        string s = "self / other : \n";
        s += "L : " + L() + " / " + other.L() + "\n";
        s += "R : " + R() + " / " + other.R() + "\n";
        s += "U : " + U() + " / " + other.U() + "\n";
        s += "D : " + D() + " / " + other.D() + "\n";
        // Debug.Log(s);

        // on vérifie si les secteurs sont voisins
        if (other.L() == R()) { return "R"; }
        if (other.R() == L()) { return "L"; }
        if (other.U() == D()) { return "D"; }
        if (other.D() == U()) { return "U"; }

        // si aucun des tests n'a renvoyé false, c'est que les secteurs sont voisins
        return "no border";
    }
}