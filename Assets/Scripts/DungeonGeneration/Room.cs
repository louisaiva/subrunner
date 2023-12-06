using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {

    // cette classe sert à créer des salles
    // elle prend en paramètre les dimensions de la salle
    // et elle remplit les tilemaps avec des tiles
    // puis génère des lights etc apropriées
    // puis génère les portes
    // puis embellit le tout avec des vieux posters, etc

    // tilemaps
    public GameObject fg;
    public GameObject bg;
    public GameObject gd;

    // dimensions
    public int width;
    public int height;
    public int x;
    public int y;

    // light tiles
    private List<string> walls_light_tiles = new List<string> {"bg_2_7","walls_1_7","walls_2_7"};
    public GameObject light_prefab;
    public Transform light_parent;
    private Vector3 light_offset = new Vector3(0.25f, 1f, 0f);

    // doors
    public Transform doors_parent;
    public GameObject door_prefab;

    // objects
    public Transform objects_parent;

    // unity functions
    void Start()
    {
        // on récupère les prefabs
        light_prefab = Resources.Load<GameObject>("prefabs/objects/small_light");
        door_prefab = Resources.Load<GameObject>("prefabs/objects/door");

        // on récupère les parents
        light_parent = transform.Find("lights");
        objects_parent = transform.Find("objects");
        doors_parent = transform.Find("doors");

        // on récupère les tilemaps
        fg = transform.Find("fg_tilemap").gameObject;
        bg = transform.Find("bg_tilemap").gameObject;
        gd = transform.Find("gd_tilemap").gameObject;

        // on met les bonnes dimensions (pour l'instant)
        // ! à changer lorsque les salles seront générées
        bg.GetComponent<Tilemap>().CompressBounds();
        width = bg.GetComponent<Tilemap>().size.x;
        height = bg.GetComponent<Tilemap>().size.y;

        x = (int) bg.GetComponent<Tilemap>().cellBounds.xMin;
        y = (int) bg.GetComponent<Tilemap>().cellBounds.yMin;
        print("tilemap dimensions : " + width + " " + height + " " + x + " " + y);

        // on initialise la salle
        init();

    }

    // functions

    public void init()
    {
        // on met des lumieres sur les murs
        // là où les tiles correspondent à tile_bg_light
        place_lights();
    }

    private void place_lights()
    {
        // on parcourt les tiles de bg_tilemap
        Tilemap bg_tilemap = bg.GetComponent<Tilemap>();
        for (int j = x; j < x + width; j++)
        {
            for (int i = y; i < y + height; i++)
            {
                // position de la tile
                Vector3Int pos = new Vector3Int(j, i, 0);

                // on récupère la tile
                TileBase tile = bg_tilemap.GetTile(pos);
                if (tile == null) { continue; }

                // on regarde si c'est une tile de mur
                TileData tile_data = new TileData();
                tile.GetTileData(pos, bg_tilemap, ref tile_data);

                // on regarde si c une tile de lumiere
                if (walls_light_tiles.Contains(tile_data.sprite.name))
                {

                    // on récupère la position globale de la tile
                    Vector3 tile_pos = bg_tilemap.CellToWorld(pos);

                    // on met une lumiere
                    GameObject light = Instantiate(light_prefab, tile_pos + light_offset, Quaternion.identity);

                    // on met le bon parent
                    light.transform.SetParent(light_parent);
                }
            }
        }
    }

}