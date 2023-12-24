using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


public class Sector : MonoBehaviour
{
    [Header("World")]
    public World world;
    private Vector2Int area_size = new Vector2Int(16, 16);


    [Header("HashSet")]
    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();


    [Header("Position")]
    public int x;
    public int y;
    public int w;
    public int h;

    [Header("Lights")]
    [SerializeField] private List<string> walls_light_tiles = new List<string> { "bg_2_7", "walls_1_7", "walls_2_7" };
    [SerializeField] private GameObject light_prefab;
    [SerializeField] private Transform light_parent;
    [SerializeField] private Vector3 light_offset = new Vector3(0.25f, 1f, 0f);



    // UNITY METHODS
    void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();

        // on récupère le prefab de lumiere
        light_prefab = Resources.Load<GameObject>("prefabs/objects/small_light");
        light_parent = transform.Find("decoratives/lights");
    }
    

    // INIT
    public void init(HashSet<Vector2Int> rooms, HashSet<Vector2Int> corridors)
    {
        // on unit les hashsets
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

        // on récupère les hashsets
        this.rooms = rooms;
        this.corridors = corridors;

        // on calcule la position SW du secteur
        x = tiles.Min(x => x.x);
        y = tiles.Min(x => x.y);

        // on calcule la taille du secteur
        w = tiles.Max(x => x.x) - x + 1;
        h = tiles.Max(x => x.y) - y + 1;

        // on recale les tiles pour que le min soit en 0,0
        recalibrateTiles();

        // on calcule le plafond
        ceiling = CreateCeiling();
    }

    private void recalibrateTiles()
    {
        // ROOMS

        // on crée un hashset temporaire
        HashSet<Vector2Int> temp = new HashSet<Vector2Int>();

        // on parcourt les rooms
        foreach (Vector2Int room in rooms)
        {
            // on ajoute le room recalibré
            temp.Add(new Vector2Int(room.x - x, room.y - y));
        }

        // on remplace les rooms
        rooms = temp;

        // CORRIDORS

        // on crée un hashset temporaire
        temp = new HashSet<Vector2Int>();

        // on parcourt les corridors
        foreach (Vector2Int corr in corridors)
        {
            // on ajoute le corr recalibré
            temp.Add(new Vector2Int(corr.x - x, corr.y - y));
        }

        // on remplace les corridors
        corridors = temp;

        // TILES
        tiles = new HashSet<Vector2Int>();
        tiles.UnionWith(rooms);
        tiles.UnionWith(corridors);

    }

    private HashSet<Vector2Int> CreateCeiling()
    {
        // on récupère le plafond
        HashSet<Vector2Int> ceiling = new HashSet<Vector2Int>();

        // renvoit un hashet qui est l'inverse des tiles, de dimension w*h
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                ceiling.Add(new Vector2Int(i, j));
            }
        }

        // on enlève les tiles
        ceiling.ExceptWith(tiles);

        // on retourne le plafond
        return ceiling;
    }

    // GENERATION
    public void GENERATE()
    {
        // on génère les objets
        PlaceLights();
    }

    // OBJETS GENERATION
    private void PlaceLights()
    {
        BoundsInt bounds = getBounds();
        TileBase[] tiles = world.getTiles(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                
                // on récupère le sprite
                Sprite sprite = world.GetSprite(x, y);
                if (sprite == null) { continue; }

                if (walls_light_tiles.Contains(sprite.name))
                {
                    // on récupère la position globale de la tile
                    Vector3 tile_pos = world.CellToWorld(new Vector3Int(x, y, 0));

                    // on met une lumiere
                    GameObject light = Instantiate(light_prefab, tile_pos + light_offset, Quaternion.identity);

                    // on met le bon parent
                    light.transform.SetParent(light_parent);
                }


                /* // on récupère la tile
                TileBase tile = tiles[x + y * bounds.size.x];
                if (tile == null) { continue; }

                // position de la tile
                Vector3Int pos = new Vector3Int(x, y, 0);

                // on vérifie si c'est une tile de mur
                TileData tile_data = new TileData();
                tile.GetTileData(pos, bg_tm, ref tile_data);

                // on regarde si c une tile de lumiere
                if (walls_light_tiles.Contains(tile_data.sprite.name))
                {
                    // on récupère la position globale de la tile
                    Vector3 tile_pos = world.CellToWorld(pos);

                    // on met une lumiere
                    GameObject light = Instantiate(light_prefab, tile_pos + light_offset, Quaternion.identity);

                    // on récupère le parent
                    // Transform parent = getSector(new Vector2Int(x, y)).transform.Find("decoratives/lights");

                    // on met le bon parent
                    light.transform.SetParent(light_parent);
                } */
            }
        }
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



    // GETTERS
    public Vector2Int getSize()
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

    public string getAreaType(Vector2Int pos)
    {
        // on vérifie qu'elle est dans les tiles
        if (tiles.Contains(pos))
        {
            // on vérifie si c'est une room
            if (rooms.Contains(pos))
            {
                // on retourne le type de room
                return "room";
            }
            else
            {
                // on retourne le type de corridor
                return "corridor";
            }
        }
        else if (ceiling.Contains(pos))
        {
            // on retourne le type de ceiling
            return "ceiling";
        }
        else
        {
            // on retourne le type de tile
            return "not found";
        }
    }

    public string getAreaName(Vector2Int pos)
    {
        string type = getAreaType(pos);
        if (type == "ceiling") { return "ceiling"; }

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

        // on parcourt les directions
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

    }

    public BoundsInt getBounds()
    {
        // on retourne les bounds
        return new BoundsInt(x * area_size.x, y * area_size.y, 0, w * area_size.x, h * area_size.y, 1);
    }
}