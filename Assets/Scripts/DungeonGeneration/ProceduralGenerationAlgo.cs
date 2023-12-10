using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;


public static class ProceduralGenerationAlgo
{
    
    // room functions

    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength, int seed=0)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        // on initialise le random
        System.Random random = new System.Random(seed);

        // on ajoute la position de départ
        path.Add(startPosition);
        var previousPosition = startPosition;

        // on génère le chemin
        for (int i = 0; i < walkLength; i++)
        {
            // on génère une nouvelle position
            var newPosition = previousPosition + GetRandomDirection(random);

            // on ajoute la nouvelle position
            path.Add(newPosition);

            // on met à jour la position précédente
            previousPosition = newPosition;
        }

        return path;
    }

    public static HashSet<Vector2Int> WalkFromAToB(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        // on ajoute la position de départ
        path.Add(start);
        var previousPosition = start;

        // on génère le chemin
        while (previousPosition != end)
        {
            // on génère une nouvelle position
            var newPosition = previousPosition + GetDirectionTo(previousPosition, end);

            // on ajoute la nouvelle position
            path.Add(newPosition);

            // on met à jour la position précédente
            previousPosition = newPosition;
        }

        return path;
    }


    // corridor functions

    public static HashSet<Vector2Int> RandomWalkCorridorNetwork(Vector2Int start, int corridor_nb, Vector2Int corridors_length_bounds,float corr_random_start=0f)
    {
        // génère un réseau de corridors aléatoires
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        // on ajoute les corridors
        for (int i = 0; i < corridor_nb; i++)
        {
            // on vérifie si on a un random start
            if (Random.Range(0f, 1f) < corr_random_start && corridors.Count > 0)
            {
                // on génère une nouvelle position de départ
                // Random randomizer = new Random();
                // Vector2Int[] asArray = corridors.ToList();
                // start = asArray[Random.Next(asArray.length)];
                start = corridors.ElementAt(Random.Range(0, corridors.Count));
            }

            // on génère un corridor
            var corridor = RandomSingleWalkCorridor(start, Random.Range(corridors_length_bounds.x, corridors_length_bounds.y));

            // on ajoute le corridor
            corridors.UnionWith(corridor);

            // on met à jour la position de départ
            start = corridor[corridor.Count - 1];
        }

        return corridors;
    }

    public static List<Vector2Int> RandomSingleWalkCorridor(Vector2Int start, int corridor_length)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        // on ajoute la position de départ
        path.Add(start);
        var previousPosition = start;
        
        // on choisit une direction aléatoire
        var direction = GetRandomDirection(new System.Random());

        // on génère le chemin
        for (int i = 0; i < corridor_length; i++)
        {
            // on génère une nouvelle position
            var newPosition = previousPosition + direction;

            // on ajoute la nouvelle position
            path.Add(newPosition);

            // on met à jour la position précédente
            previousPosition = newPosition;
        }

        return path;
    }

    // direction functions

    public static Vector2Int GetRandomDirection(System.Random random)
    {
        // on récupère un nombre aléatoire entre 0 et 3
        int randomNumber = random.Next(0, 4);

        // on retourne la direction correspondante
        switch (randomNumber)
        {
            case 0:
                return Vector2Int.up;
            case 1:
                return Vector2Int.right;
            case 2:
                return Vector2Int.down;
            default:
                return Vector2Int.left;
        }
    }

    public static Vector2Int GetDirectionTo(Vector2Int start, Vector2Int end)
    {
        // on récupère la direction
        Vector2Int direction = end - start;

        // on normalise la direction
        direction = new Vector2Int(Mathf.Clamp(direction.x, -1, 1), Mathf.Clamp(direction.y, -1, 1));

        // si on a un vecteur diagonal, on le transforme en vecteur cardinal
        if (direction.x != 0 && direction.y != 0)
        {
            // on récupère un nombre aléatoire entre 0 et 1
            int randomNumber = Random.Range(0, 2);

            // on retourne la direction correspondante
            switch (randomNumber)
            {
                case 0:
                    return new Vector2Int(direction.x, 0);
                default:
                    return new Vector2Int(0, direction.y);
            }
        }

        return direction;
    }

}