using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    
    // la classe WORLD génère les différents secteurs du jeu
    
    // init des variables d'instance
    public GameObject sector; // le secteur actuel


    // 0 - GAMEOBJECTS

    // 1 - SPRITESHEETS
    private string path_spritesheets = "spritesheets/environments/";
    private Sprite[] spsh_wall;

    // 2 - PROCEDURAL GENERATION
    public int sector_id = 0; // id du secteur
    public int nb_rectangles = 10; // nombre de rectangles générés
    public float min_width; // largeur minimale d'un rectangle
    public float max_width; // largeur maximale d'un rectangle
    public float rayon_base_circle;
    public GameObject rectangles; // le parent des rectangles


    void Start()
    {

        // on récupère le sol du secteur
        // ground = transform.Find("ground").gameObject;

        // on récupère le secteur actuel
        sector = transform.Find("sector_1").gameObject;

        // on génère le secteur actuel en tant que secteur 01
        // Generate(1);
    }

    // generation procédurale
    private void Generate(int sector_id)
    {
        // on met à jour l'id du secteur
        this.sector_id = sector_id;

        // on récupère les spritesheets
        spsh_wall = Resources.LoadAll<Sprite>(path_spritesheets + "wall_s" + sector_id.ToString());


        // TINYKEEP ALGORITHM

        // STEP 1 : rectangles generation
        rectangles = transform.Find("rectangles").gameObject;

        // on crée un sprite pour les rectangles
        Texture2D room_tex = Texture2D.whiteTexture;
        Sprite room_spr = Sprite.Create(room_tex, new Rect(0, 0, room_tex.width, room_tex.height), new Vector2(0.5f, 0.5f), (float)room_tex.width);

        for (int i = 0; i < nb_rectangles; i++)
        {
            // on place le rectangle dans le monde
            GameObject room = new GameObject("room_" + i.ToString());
            room.transform.parent = rectangles.transform;
            room.AddComponent<SpriteRenderer>();
            room.GetComponent<SpriteRenderer>().sprite = room_spr;

            // on lui donne une position aléatoire
            room.transform.position = new Vector3(Random.Range(-rayon_base_circle, rayon_base_circle), Random.Range(-rayon_base_circle, rayon_base_circle), 0);

            // on lui donne une taille aléatoire
            room.transform.localScale = new Vector3(Random.Range(min_width, max_width), Random.Range(min_width, max_width), 1);
        }

        // STEP 2 : separate rooms

        // on récupère les rooms
        List<GameObject> rooms = new List<GameObject>();
        foreach (Transform child in rectangles.transform)
        {
            rooms.Add(child.gameObject);
        }

        // on sépare les rooms
        TKProcGen_SeparateRooms(rooms);

        // STEP 3 : identify main rooms

        // STEP 4 : delaunay triangulation


    }

    private void TKProcGen_SeparateRooms(List<GameObject> rooms)
    {
        do
        {
            for (int current = 0; current < rooms.Count; current++)
            {
                for (int other = 0; other < rooms.Count; other++)
                {
                    if (current == other || !Overlapping(rooms[current], rooms[other])) continue;

                    Rect current_rect = GetRectFromRoom(rooms[current]);
                    Rect other_rect = GetRectFromRoom(rooms[other]);

                    var direction = (other_rect.center - current_rect.center).normalized;

                    TKProcGen_Move(rooms[current], -direction);
                    TKProcGen_Move(rooms[other], direction);
                }
            }
        }
        while (IsAnyRoomOverlapped(rooms));
    }

    public void TKProcGen_Move(GameObject room, Vector2 move, int tileSize = 1)
    {
        Vector2 movin = room.transform.position;
        movin.x += Mathf.RoundToInt(move.x) * tileSize;
        movin.y += Mathf.RoundToInt(move.y) * tileSize;
        room.transform.position = movin;
    }

    private bool Overlapping(GameObject room1, GameObject room2)
    {

        Rect rect1 = GetRectFromRoom(room1);
        Rect rect2 = GetRectFromRoom(room2);
        return rect1.Overlaps(rect2);
    }

    private bool IsAnyRoomOverlapped(List<GameObject> rooms)
    {
        for (int current = 0; current < rooms.Count; current++)
        {
            for (int other = 0; other < rooms.Count; other++)
            {
                if (current == other || !Overlapping(rooms[current], rooms[other])) continue;

                return true;
            }
        }
        return false;
    }

    private Rect GetRectFromRoom(GameObject room)
    {
        return new Rect(room.transform.position.x, room.transform.position.y, room.transform.localScale.x, room.transform.localScale.y);
    }

}
