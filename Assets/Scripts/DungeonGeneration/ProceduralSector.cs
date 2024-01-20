using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ProceduralSector : Sector
{



    [Header("Doors")]
    [SerializeField] private Dictionary<Vector2, Vector2Int> doors = new Dictionary<Vector2, Vector2Int>();

    // unity functions

    // INIT
    public void init(HashSet<Vector2Int> rooms, HashSet<Vector2Int> corridors)
    {

        // on récupère les hashsets
        this.rooms = rooms;
        this.corridors = corridors;

        // on calcule le nombre de zombies basé sur le nombre de salles dans room
        nb_enemies = rooms.Count;

        base.init();

        // on recale les tiles pour que le min soit en 0,0
        recalibrateTiles();
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

    // OBJETC GENERATION
    public void GENERATE(List<Vector2> empl_enemies, List<Vector2> empl_interactives, Dictionary<Vector2, Vector2Int> empl_doors)
    {
        base.GENERATE(empl_enemies, empl_interactives);

        this.doors = empl_doors;
        PlaceDoors();
    }
    private void PlaceDoors()
    {
        int i = 0;

        // on parcourt les connections
        foreach (KeyValuePair<Vector2, Vector2Int> empl in doors)
        {

            // on instancie une porte
            Vector3 pos = new Vector3(empl.Key.x, empl.Key.y, 0);
            GameObject door = null;


            if (new Vector2Int[] { Vector2Int.up, Vector2Int.down }.Contains(empl.Value))
            {
                print("instantiate a UD door at " + empl.Key);
                // on instancie une porte verticale (up ou down)
                door = Instantiate(prefabs["doorUD"], pos, Quaternion.identity);
            }
            else if (new Vector2Int[] { Vector2Int.left, Vector2Int.right }.Contains(empl.Value))
            {
                print("instantiate a LR door at " + empl.Key);
                // on instancie une porte horizontale (left ou right
                door = Instantiate(prefabs["doorLR"], pos, Quaternion.identity);
            }

            // on met le bon parent
            door.transform.SetParent(parents["doors"]);

            i++;
        }
    }



}