using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SectorGenerator : MonoBehaviour
{
    [Header("GENERATION PARAMETERS")]
    protected Vector2Int startPosition = new Vector2Int(0, 0);

    [SerializeField] private int iterations = 20;
    [SerializeField] private int walkLength = 5;
    [SerializeField] private bool startRandomlyEachIteration = false;


    [Header("ROOMS PARAMETERS")]
    [SerializeField] protected int nb_rooms = 5; // ! cela ne veut pas dire qu'on aura 5 salles, mais qu'on va essayer d'en avoir 5, il peut y avoir des doublons
    // [SerializeField]
    protected Vector2 roomDimensions = new Vector2(8f, 7.5f);



    [Header("TILEMAPS VISUALISATION")]
    protected TilemapVisualiser tilemapVisualiser;
    // private bool visualise = false;

    [Header("WORLD")]
    protected GameObject world;
    protected bool already_generated = false;


    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère le tilemap visualiser
        tilemapVisualiser = transform.Find("grid").GetComponent<TilemapVisualiser>();

        // on récupère le world
        world = GameObject.Find("/world");
    }

    
    public void GenerateSectorHashSets(ref HashSet<Vector2Int> roomsPositions, ref HashSet<Vector2Int> corridorsPositions)
    {
        // on génère un premier brouillon
        HashSet<Vector2Int> floorPositions = RandomWalkWorldGeneration();

        // on séléctionne N positions aléatoires -> les salles
        roomsPositions = RandomlyChooseRooms(floorPositions);
        floorPositions.ExceptWith(roomsPositions);

        // on séléctionne les corridors
        corridorsPositions = BuildCorridors(floorPositions, roomsPositions);
        floorPositions.ExceptWith(corridorsPositions);
        corridorsPositions.ExceptWith(roomsPositions);
    }




    // GENERATION FUNCTIONS
    protected HashSet<Vector2Int> RandomWalkWorldGeneration()
    {

        // on génère n fois le chemin (n = iterations)
        // pour avoir une salle conséquente

        // on pose la position de départ
        var currentPosition = startPosition;

        // on génère le chemin total
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < iterations; i++)
        {
            // on récupère le seed
            int seed = Random.Range(0, 1000000);

            // on génère le chemin
            var path = ProceduralGenerationAlgo.SimpleRandomWalk(currentPosition, walkLength, seed);

            // on ajoute le chemin a floorPositions
            floorPositions.UnionWith(path);

            // on choisit une position de départ (si on veut démarrer aléatoirement)
            if (startRandomlyEachIteration)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }
        }

        return floorPositions;
    }

    protected HashSet<Vector2Int> RandomlyChooseRooms(HashSet<Vector2Int> floorPositions)
    {
        // on crée un hashset de positions de salles
        HashSet<Vector2Int> roomsPositions = new HashSet<Vector2Int>();

        // on choisit N positions aléatoires dans floorPositions
        for (int i = 0; i < nb_rooms; i++)
        {
            // on choisit une position aléatoire
            Vector2Int randomPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));

            // on ajoute la position à roomsPositions
            roomsPositions.Add(randomPosition);
        }

        return roomsPositions;
    }

    protected HashSet<Vector2Int> BuildCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> roomsPositions)
    {
        // on crée un hashset de positions de corridors
        HashSet<Vector2Int> corridorsPositions = new HashSet<Vector2Int>();

        // le but est de relier les salles entre elles
        

        // on crée une List de positions de salles
        List<Vector2Int> roomsPositionsList = roomsPositions.ToList();

        // on choisit une salle au hasard
        Vector2Int current_room = roomsPositionsList[Random.Range(0, roomsPositionsList.Count)];

        // on supprime la salle de la liste
        roomsPositionsList.Remove(current_room);

        while (roomsPositionsList.Count > 0)
        {
            // on récupère la salle la plus proche
            Vector2Int closest_room = GetClosestRoomPosition(current_room, roomsPositionsList);

            // on génère le chemin entre les deux salles
            HashSet<Vector2Int> path = ProceduralGenerationAlgo.WalkFromAToB(current_room, closest_room);

            // on ajoute le chemin à corridorsPositions
            corridorsPositions.UnionWith(path);

            // on met à jour la salle courante
            current_room = closest_room;

            // on supprime la salle de la liste
            roomsPositionsList.Remove(current_room);
        }

        /* // on parcourt les salles
        foreach (var roomPosition in roomsPositions)
        {
            // on récupère la salle la plus proche
            Vector2Int closestRoomPosition = GetClosestRoomPosition(roomPosition, roomsPositions);

            // on génère le chemin entre les deux salles
            HashSet<Vector2Int> path = ProceduralGenerationAlgo.WalkFromAToB(roomPosition, closestRoomPosition);

            // on ajoute le chemin à corridorsPositions
            corridorsPositions.UnionWith(path);
        } */

        return corridorsPositions;

    }

    // ROOMS FUNCTIONS

    public Dictionary<Vector2Int,Room> GenerateRooms(HashSet<Vector2Int> roomsPositions, HashSet<Vector2Int> corridorsPositions, Vector2Int sectorPos, GameObject parent = null, bool extended_rooms=false )
    {
        // on crée un dictionnaire de salles
        Dictionary<Vector2Int,Room> areas = new Dictionary<Vector2Int,Room>();

        // on parcourt les positions de salles
        foreach (var position in roomsPositions)
        {
            // on récupère les directions vers les salles/corridors adjacent.e.s
            HashSet<Vector2Int> openinRoomsDirections = GetRoomOpeninDirections(position, roomsPositions);
            HashSet<Vector2Int> openinCorridorsDirections = GetRoomOpeninDirections(position, corridorsPositions);
            // openinRoomsDirections.UnionWith(openinCorridorsDirections);

            // on génère la salle
            Room area = GenerateRoom(position, openinCorridorsDirections, openinRoomsDirections, parent);

            // on ajoute la salle au dictionnaire
            areas.Add(position+sectorPos, area);
        }

        return areas;
    }

    protected Room GenerateRoom(Vector2Int position, HashSet<Vector2Int> corrDirections, HashSet<Vector2Int> roomDirections, GameObject parent = null, bool extended_room=false)
    {
        // on choisit la bonne salle en fonctions des ouvertures
        GameObject room = ChooseRoom(corrDirections, roomDirections, extended_room);

        // on récupère la position du monde
        Vector3 worldPosition = GetWorldPositionFromTileworldPosition(position);

        // on instancie la salle
        GameObject roomInstance = Instantiate(room, worldPosition, Quaternion.identity);

        // on met le bon parent
        if (parent != null)
        {
            roomInstance.transform.parent = parent.transform;
        }
        else
        {
            roomInstance.transform.parent = world.transform;
        }
        
        return roomInstance.GetComponent<Room>();
    }

    protected GameObject ChooseRoom(HashSet<Vector2Int> corrDirections, HashSet<Vector2Int> roomDirections, bool extended_room=false)
    {
        // on récupère les noms des salles
        string roomName = "room_";

        // on parcourt les directions ouvertes puis on ajoute le nom de la direction
        if (corrDirections.Contains(Vector2Int.up)) { roomName += "U"; }
        else if (roomDirections.Contains(Vector2Int.up)) { roomName += (extended_room? "N":"U"); }
        if (corrDirections.Contains(Vector2Int.down)) { roomName += "D"; }
        else if (roomDirections.Contains(Vector2Int.down)) { roomName += (extended_room? "S":"D"); }
        if (corrDirections.Contains(Vector2Int.left)) { roomName += "L"; }
        else if (roomDirections.Contains(Vector2Int.left)) { roomName += (extended_room?"W":"L"); }
        if (corrDirections.Contains(Vector2Int.right)) { roomName += "R"; }
        else if (roomDirections.Contains(Vector2Int.right)) { roomName += (extended_room?"E":"R"); }

        // on récupère la salle
        GameObject room = Resources.Load<GameObject>("prefabs/rooms/" + roomName);

        // on vérifie que la salle existe
        if (room == null)
        {
            Debug.LogError("room " + roomName + " doesn't exist");
        }

        // print("chosen room name : " + roomName + " " + room);

        return room;
    }


    // CORRIDORS FUNCTIONS

    public Dictionary<Vector2Int, Room> GenerateCorridors(HashSet<Vector2Int> corridorsPositions, HashSet<Vector2Int> roomsPositions, Vector2Int sectorPos, GameObject parent = null)
    {
        // on crée un dictionnaire de salles
        Dictionary<Vector2Int, Room> areas = new Dictionary<Vector2Int, Room>();

        // on parcourt les positions des corridors
        foreach (var position in corridorsPositions)
        {
            // on récupère les directions vers les salles/corridors adjacent.e.s
            HashSet<Vector2Int> openinDirections = GetRoomOpeninDirections(position, roomsPositions);
            HashSet<Vector2Int> openinCorridorsDirections = GetRoomOpeninDirections(position, corridorsPositions);
            openinDirections.UnionWith(openinCorridorsDirections);

            // on génère la salle
            Room area = GenerateCorridor(position, openinDirections, parent);

            // on ajoute la salle au dictionnaire
            areas.Add(position+sectorPos, area);
        }

        return areas;
    }

    protected Room GenerateCorridor(Vector2Int position, HashSet<Vector2Int> openinDirections, GameObject parent = null)
    {
        // on choisit la bonne salle en fonctions des ouvertures
        GameObject corr = ChooseCorridor(openinDirections);

        // on récupère la position du monde
        Vector3 worldPosition = GetWorldPositionFromTileworldPosition(position);

        // on instancie la salle
        GameObject corrInstance = Instantiate(corr, worldPosition, Quaternion.identity);

        // on met le bon parent
        if (parent != null)
        {
            corrInstance.transform.parent = parent.transform;
        }
        else
        {
            corrInstance.transform.parent = world.transform;
        }

        return corrInstance.GetComponent<Room>();
    }

    protected GameObject ChooseCorridor(HashSet<Vector2Int> openinDirections)
    {
        // on récupère les noms des salles
        string corrName = "corridor_";

        // on parcourt les directions ouvertes puis on ajoute le nom de la direction
        if (openinDirections.Contains(Vector2Int.up)) { corrName += "U"; }
        if (openinDirections.Contains(Vector2Int.down)) { corrName += "D"; }
        if (openinDirections.Contains(Vector2Int.left)) { corrName += "L"; }
        if (openinDirections.Contains(Vector2Int.right)) { corrName += "R"; }

        // on récupère la salle
        GameObject corr = Resources.Load<GameObject>("prefabs/rooms/" + corrName);

        // print("chosen corr name : " + corrName + " " + corr);

        return corr;
    }



    // ROOMS HELPERS

    protected HashSet<Vector2Int> GetRoomAdjacentPositions(Vector2Int position, HashSet<Vector2Int> positions)
    {
        // on crée un hashset de positions adjacentes
        HashSet<Vector2Int> adjacentPositions = new HashSet<Vector2Int>();

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on parcourt les directions
        foreach (var direction in directions)
        {
            // on récupère la position adjacente
            Vector2Int adjacentPosition = position + direction;

            // on vérifie que la position adjacente est une salle
            if (positions.Contains(adjacentPosition))
            {
                // on ajoute la position adjacente
                adjacentPositions.Add(adjacentPosition);
            }
        }

        return adjacentPositions;
    }

    protected HashSet<Vector2Int> GetRoomOpeninDirections(Vector2Int position, HashSet<Vector2Int> positions)
    {
        // on crée un hashset de positions adjacentes
        HashSet<Vector2Int> openinDirections = new HashSet<Vector2Int>();

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // on parcourt les directions
        foreach (var direction in directions)
        {
            // on récupère la position adjacente
            Vector2Int adjacentPosition = position + direction;

            // on vérifie que la position adjacente est une salle
            if (positions.Contains(adjacentPosition))
            {
                // on ajoute la position adjacente
                openinDirections.Add(direction);
            }
        }

        return openinDirections;
    }


    // HELPERS

    protected Vector2Int GetClosestRoomPosition(Vector2Int position, List<Vector2Int> roomsPositions)
    {
        // on initialise la position la plus proche
        Vector2Int closestRoomPosition = new Vector2Int(0, 0);
        float closestDistance = 1000000f;

        // on parcourt les positions de salles
        foreach (var roomPosition in roomsPositions)
        {
            // on vérifie que ce n'est pas la même position
            if (roomPosition == position) { continue; }

            // on calcule la distance entre la position et la salle
            float distance = Vector2Int.Distance(position, roomPosition);

            // si la distance est inférieure à la distance la plus proche
            if (distance < closestDistance)
            {
                // on met à jour la position la plus proche
                closestRoomPosition = roomPosition;
                closestDistance = distance;
            }
        }

        return closestRoomPosition;
    }

    protected Vector3 GetWorldPositionFromTileworldPosition(Vector2Int position)
    {
        return new Vector3(position.x*roomDimensions.x, position.y * roomDimensions.y, 0);
    }

    protected void Clean()
    {
        if (already_generated)
        {
            // on clean le world
            foreach (Transform child in world.transform)
            {
                // on vérifie qu'on supprime pas la global light
                if (child.gameObject.name == "global_light") { continue; }

                // on supprime les objets qui ont setactive(true)
                if (child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // on supprime le dernier chemin visualisé
        tilemapVisualiser.Clear();
    }

}