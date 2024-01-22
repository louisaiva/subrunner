using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ProceduralSector : Sector
{



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



}