using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;


public class Area : MonoBehaviour
{
    [Header("Area")]    
    public int x;
    public int y;
    public int w = 16;
    public int h = 16;

    public string type; // exemple : room_U, corridor_LR, sas_ND, ...


    [Header("Zones")]
    private GameObject zone_prefab;
    // private Vector2 zone_pos = new Vector3(4, 4, 0); // position par défaut de la zone (au milieu de l'area)
    private List<GameObject> zones = new List<GameObject>();


    // SECTOR & NEIGHBORS


    [Header("Sector")]
    protected World world;
    protected AreaJsonHandler builder;
    protected Sector sector;
    protected Vector2Int area_size = new Vector2Int(16, 16);
    protected BoundsInt bounds;
    public AreaJson json;
    public string skin = "base_sector";

    [Header("Neighbors")]
    // protected Area[] neighbors = new Area[4]; // 0: top, 1: right, 2: bottom, 3: left

    [Header("Ceilings")]
    // protected Dictionary<string, GameObject> ceilings = new Dictionary<string, GameObject>();






    // OBJECTS


    [Header("Objects")]
    [SerializeField] protected Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    [SerializeField] protected Dictionary<string, Transform> parents = new Dictionary<string, Transform>();
    [SerializeField] protected Dictionary<string, HashSet<Vector2>> emplacements = new Dictionary<string, HashSet<Vector2>>();

    [Header("Lights")]
    [SerializeField] protected List<string> walls_light_tiles = new List<string> { "bg_2_7", "walls_1_7", "walls_2_7", "walls_3_7" };
    [SerializeField] protected Vector3 light_offset = new Vector3(0.25f, 1f, 0f);

    [Header("Posters & Tags")]
    [SerializeField] protected Sprite[] poster_sprites;
    protected List<string> full_walls_tiles = new List<string> {"walls_1_3", "walls_1_4", "walls_1_5", "walls_1_6"
                                                        , "walls_1_7", "walls_1_8",
                                                        "walls_2_3", "walls_2_4", "walls_2_5", "walls_2_6"
                                                        , "walls_2_7", "walls_2_8",
                                                        "walls_3_3", "walls_3_5", "walls_3_6"
                                                        , "walls_3_7", "walls_3_8"};
    [SerializeField] protected float density_posters = 0.3f; // 0.5 = 1 poster tous les 2 tiles compatibles
    [SerializeField] protected Sprite[] tags_sprites;
    [SerializeField] protected Vector3 tag_offset = new Vector3(2.5f, 0.75f, 0f);
    [SerializeField] protected float density_tags = 0.5f;
    // unity functions
    void Awake()
    {
        // on récupère le world
        world = GameObject.Find("/world").GetComponent<World>();

        // on récupère le builder
        builder = GameObject.Find("/generator").transform.Find("builder").GetComponent<AreaJsonHandler>();

        // on récupère les prefabs
        prefabs.Add("light", Resources.Load<GameObject>("prefabs/objects/lights/small_light"));
        prefabs.Add("poster", Resources.Load<GameObject>("prefabs/objects/poster"));
        prefabs.Add("enemy", Resources.Load<GameObject>("prefabs/beings/enemies/zombo"));
        prefabs.Add("chest", Resources.Load<GameObject>("prefabs/objects/chest"));
        prefabs.Add("xp_chest", Resources.Load<GameObject>("prefabs/objects/xp_chest"));
        prefabs.Add("computer", Resources.Load<GameObject>("prefabs/objects/computer"));
        prefabs.Add("playstation", Resources.Load<GameObject>("prefabs/objects/playstation"));
        prefabs.Add("trash_container", Resources.Load<GameObject>("prefabs/objects/trash_container"));
        prefabs.Add("fridge", Resources.Load<GameObject>("prefabs/objects/fridge"));
        prefabs.Add("bin", Resources.Load<GameObject>("prefabs/objects/deco/bin"));
        prefabs.Add("healing_tube", Resources.Load<GameObject>("prefabs/objects/tube_healing"));
        prefabs.Add("doorUD", Resources.Load<GameObject>("prefabs/objects/door"));
        prefabs.Add("doorLR", Resources.Load<GameObject>("prefabs/objects/door_L"));
        prefabs.Add("sector_label", Resources.Load<GameObject>("prefabs/objects/sector_label"));
        prefabs.Add("server_rack", Resources.Load<GameObject>("prefabs/objects/server_rack"));
        prefabs.Add("tag", Resources.Load<GameObject>("prefabs/objects/tag"));
        prefabs.Add("base_ceiling", Resources.Load<GameObject>("prefabs/sectors/ceiling"));

        // on récupère les parents
        parents.Add("light", transform.Find("decoratives/lights"));
        parents.Add("poster", transform.Find("decoratives/posters"));
        parents.Add("enemy", GameObject.Find("/world/enemies").transform);
        parents.Add("chest", transform.Find("interactives"));
        parents.Add("xp_chest", transform.Find("interactives"));
        parents.Add("computer", transform.Find("interactives"));
        parents.Add("playstation", transform.Find("interactives"));
        parents.Add("trash_container", transform.Find("interactives"));
        parents.Add("fridge", transform.Find("interactives"));
        parents.Add("healing_tube", transform.Find("interactives"));
        parents.Add("server_rack", transform.Find("interactives"));
        parents.Add("sas_doors", transform.Find("interactives"));
        parents.Add("sector_label", transform.Find("decoratives"));
        parents.Add("bin", transform.Find("decoratives"));
        parents.Add("tag", transform.Find("decoratives/posters"));
        parents.Add("ceiling", transform.Find("ceilings"));
        parents.Add("decorative", transform.Find("decoratives"));
        parents.Add("interactive", transform.Find("interactives"));


        poster_sprites = Resources.LoadAll<Sprite>("spritesheets/environments/objects/posters");
        tags_sprites = Resources.LoadAll<Sprite>("spritesheets/environments/objects/tags");

        // on récupère le prefab de zone vide
        zone_prefab = Resources.Load<GameObject>("prefabs/zones/empty_zone");
    }

    // init functions
    public void init(int x, int y, int w, int h, string type, Sector sector)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.type = type;
        this.sector = sector;

        // on met le bon nom
        this.gameObject.name = "(" + x + ":" + y + ") " + type;

        // on met le bon parent
        this.transform.parent = sector.transform.Find("areas");

        // on met la bonne position
        Vector2Int real_pos = getGlobalPosition();
        this.transform.position = new Vector3(real_pos.x, real_pos.y, 0);


        // on récup le bounds
        bounds = new BoundsInt(real_pos.x*2, real_pos.y*2, 0, w, h, 1);

        // on récup le skin
        skin = sector.getAreaSkin(new Vector2Int(x, y));

        // on récup les emplacements
        json = builder.LoadAreaJson(type);
        if (json != null)
        {
            emplacements = json.GetEmplacements();

            // on met les tilemaps de world à jour
            // world.PlaceArea(this);
            world.SetAreaTiles(x + sector.x, y + sector.y, json, skin);
        }
        else
        {
            Debug.LogWarning("AreaJson not found for " + type);
            return;
        }

        // on initialise les zones qu'on a en emplacements
        foreach (KeyValuePair<string, HashSet<Vector2>> emplacement in emplacements)
        {
            if (emplacement.Key.Contains("zone"))
            {
                // we get the size of the zone
                string[] parts = emplacement.Key.Split('x');
                parts[0] = parts[0].Replace("zone", "");
                try
                {
                    int.Parse(parts[0]);
                    int.Parse(parts[1]);
                }
                catch
                {
                    Debug.LogWarning("(Area) Zone size not found for " + emplacement.Key);
                    continue;
                }
                Vector2Int zone_size = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
                
                // we get the positions
                foreach (Vector2 pos in emplacement.Value)
                {
                    // on récupère la zone
                    GameObject zone = Instantiate(zone_prefab, Vector3.zero, Quaternion.identity);
                    zone.GetComponent<Zone>().INIT(zone_size, pos, this.transform);
                    // Destroy(zone);

                    // on ajoute la zone à la liste
                    zones.Add(zone);
                }
            }
        }
    }

    /* public void setZone(GameObject zone_prefab, Vector2 pos = default(Vector2))
    {
        this.zone_prefab = zone_prefab;
        
        if (pos != default(Vector2))
        {
            zone_pos = pos;
        }
    } */

    public void GENERATE()
    {

        // on place les zones
        /* if (zone_prefab != null)
        {
            GameObject zone = Instantiate(zone_prefab, Vector3.zero, Quaternion.identity);
            zone.GetComponent<Zone>().PlaceZone(this);
            Destroy(zone);
        } */

        // on regarde si on a un skin qui permet de placer des posters & tags
        if (skin != "server")
        {
            // we need to find 5 tiles in a row to place a tag
            int tags_width_in_tiles = 5;
            int compter_tags = 0;
    
            // on parcourt les tiles et on place les objets
            for (int j = 0; j < bounds.size.y; j++)
            {
                for (int i = 0; i < bounds.size.x; i++)
                {
                    // on récupère la position globale de la tile
                    int x_ = bounds.x + i;
                    int y_ = bounds.y + j;

                    // on récupère le sprite
                    Sprite sprite = world.GetSprite(x_, y_);
                    if (sprite == null)
                    {
                        compter_tags = 0;
                        continue;
                    }

                    // on place les objets
                    Vector3 tile_pos = world.CellToWorld(new Vector3Int(x_, y_, 0));



                    // on place les lumières
                    string[] sprite_name = sprite.name.Split('_');
                    if (sprite_name[0] == "walls" && sprite_name[2] == "7")
                    {
                        PlaceLight(tile_pos);
                    }

                    // on place les posters et les tags
                    if (full_walls_tiles.Contains(sprite.name))
                    {
                        PlacePoster(tile_pos);

                        // on vérifie qu'il y a assez de place pour mettre un tag
                        if (compter_tags == 0)
                        {
                            compter_tags = 1;
                        }
                        else
                        {
                            compter_tags++;
                        }

                        if (compter_tags >= tags_width_in_tiles)
                        {
                            PlaceTag(world.CellToWorld(new Vector3Int(x_-tags_width_in_tiles, y_, 0)));
                            compter_tags = 0;
                        }
                    }
                    else
                    {
                        compter_tags = 0;
                    }


                }
            }
        }

        // on parcourt les zones et on les génère
        foreach (GameObject zone in zones)
        {
            zone.GetComponent<Zone>().GENERATE();
        }

        // on parcourt les emplacements et on place les objets
        /* foreach (KeyValuePair<string, HashSet<Vector2>> emplacement in emplacements)
        {
            if (zone_prefab != null ) { continue; }

            if (emplacement.Key == "interactive")
            {
                foreach (Vector2 pos in emplacement.Value)
                {
                    PlaceInteractive(new Vector3(pos.x, pos.y, 0));
                }
            }
        } */


    }

    /* public void PlaceZoneObject(GameObject obj, Vector3 pos, string parent="decorative")
    {
        // on créee l'objet
        GameObject interactive = Instantiate(obj, pos, Quaternion.identity);

        // on met le bon parent
        interactive.transform.SetParent(parents[parent]);
        interactive.transform.localPosition = pos + new Vector3(zone_pos.x, zone_pos.y, 0);
    } */


    // OBJETS GENERATION
    protected void PlaceLight(Vector3 tile_pos)
    {
        // on récupère la position globale de la tile
        // Vector3 tile_pos = world.CellToWorld(new Vector3Int(x,y, 0));

        // on met une lumiere
        GameObject light = Instantiate(prefabs["light"], tile_pos + light_offset, Quaternion.identity);

        // on met le bon parent
        light.transform.SetParent(parents["light"]);
    }

    protected void PlacePoster(Vector3 tile_pos)
    {
        // on lance un random pour savoir si on met un poster
        if (Random.Range(0f, 1f) > density_posters) { return; }

        // on instancie un poster aleatoire dans une premiere position
        GameObject poster = Instantiate(prefabs["poster"], tile_pos, Quaternion.identity);
        poster.GetComponent<SpriteRenderer>().sprite = poster_sprites[Random.Range(0, poster_sprites.Length)];


        // on calcule un offset plus ou moins aléatoire
        float OFFSET_Y = 0.2f;
        // float OFFSET_RANGE_Y = 0.3f;
        float OFFSET_X = -poster.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        // float OFFSET_RANGE_X = 0f;

        Vector3 offset = Vector3.zero;
        // offset += new Vector3(OFFSET_X, OFFSET_Y, 0f);
        // offset += new Vector3(Random.Range(-OFFSET_RANGE_X, OFFSET_RANGE_X), OFFSET_Y + Random.Range(0f, OFFSET_RANGE_Y), 0f);
        // offset.x += OFFSET_X;
        offset.y += 3 * OFFSET_Y;

        // on déplace le poster
        poster.transform.position += offset;

        // on met le bon parent
        poster.transform.SetParent(parents["poster"]);

        // on relance un random pour savoir si on remet un poster
        if (Random.Range(0f, 1f) < 0.1f)
        {
            PlacePoster(tile_pos);
        }
    }

    protected void PlaceTag(Vector3 tile_pos)
    {
        // on lance un random pour savoir si on met un tag
        if (Random.Range(0f, 1f) > density_tags) { return; }

        // on instancie un tag
        GameObject tag = Instantiate(prefabs["tag"], tile_pos + tag_offset, Quaternion.identity);

        // on choisit un sprite random
        tag.GetComponent<SpriteRenderer>().sprite = tags_sprites[Random.Range(0, tags_sprites.Length)];

        // on met le bon parent
        tag.transform.SetParent(parents["tag"]);
    }

    protected void PlaceInteractive(Vector3 pos)
    {
        string interactive_type = sector.consumeEmplacement(this);

        if (interactive_type == "") { return; }

        // print("placing " + interactive_type + " at " + pos);

        // on créee l'objet
        GameObject interactive = Instantiate(prefabs[interactive_type], pos, Quaternion.identity);

        // on met le bon parent
        interactive.transform.SetParent(parents[interactive_type]);
        interactive.transform.localPosition = pos + new Vector3(area_size.x/4, area_size.y/4, 0);
    }

    // ENEMIES
    public GameObject PlaceEnemy()
    {
        // on vérfie si on a des emplacements pour les ennemis
        if (!hasEnemyEmplacement()) { return null; }

        // on récupère les emplacements
        HashSet<Vector2> enemies_emplacements = emplacements["enemy"];

        // on place un ennemi
        Vector2 pos = enemies_emplacements.ElementAt(Random.Range(0, enemies_emplacements.Count));
        Vector3 global_pos = new Vector3(pos.x + area_size.x / 4 + getGlobalPosition().x
                                        , pos.y + area_size.y / 4 + getGlobalPosition().y, 0);

        GameObject enemy = Instantiate(prefabs["enemy"], global_pos, Quaternion.identity);
        enemy.transform.SetParent(parents["enemy"]);
        // enemy.transform.localPosition = new Vector3(, pos.y + area_size.y / 4, 0);

        return enemy;
    }

    public bool hasEnemyEmplacement()
    {
        return emplacements.ContainsKey("enemy");
    }

    // GETTERS
    public BoundsInt getBounds()
    {
        return bounds;
    }

    /* public Vector2Int getZoneOrigin()
    {
        return (getGlobalPosition() + new Vector2Int((int) zone_pos.x, (int) zone_pos.y))*2;
    } */

    public Vector2Int getGlobalPosition()
    {
        return (new Vector2Int(sector.x + x, sector.y + y))*area_size/2;
    }
}