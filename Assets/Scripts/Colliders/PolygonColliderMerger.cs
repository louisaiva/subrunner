using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// create a polygon shape and apply it to the polygon collider
/// from the physics shapes of the children of the sources
/// </summary>

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderMerger : MonoBehaviour
{
    [Header("Merge options")]
    [Tooltip("If true, then it will merge the shapes of the sources, if false it will create multiple paths for each source")]
    public bool merge = true;
    [Tooltip("For merging a convex polygon shape. If false, then it will merge a concave shape")]
    public bool convex = true; // does not work with concave shapes !!
    [Tooltip("If convex is false, the alpha value of the concave hull")]
    public float alpha = 0.1f; // concave hull alpha value

    [Header("Sources")]
    public List<Transform> sources = new List<Transform>();

    [Header("Debug")]
    public bool debug = false;


    // START
    private void Start()
    {
        // we get the polygon collider
        PolygonCollider2D polygon = GetComponent<PolygonCollider2D>();

        // we check if we are merging 
        if (!merge)
        {
            // we get the children paths and we apply it to the polygon
            List<Vector2[]> paths = new();
            foreach (Transform source in sources)
            {
                paths.AddRange(getChildrenPaths(source));
            }
            set_polygon_paths(polygon, paths);
            return;
        }

        // we check if we are convex merging :
        if (convex)
        {
            setConvexPolygonShape(polygon);
        }
        else
        {
            setConcavePolygonShape(polygon);
        }

    }

    // GETTING SOURCES's CHILDREN POINTS
    private List<Vector2> getChildrenPoints(Transform parent)
    {
        List<Vector2> allPoints = new List<Vector2>();

        foreach (Transform child in parent)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) continue;

            Sprite sprite = spriteRenderer.sprite;
            List<Vector2> shape = new List<Vector2>();
            sprite.GetPhysicsShape(0, shape);

            // Convert local shape to parent local space
            for (int i = 0; i < shape.Count; i++)
            {
                shape[i] = parent.InverseTransformPoint(child.TransformPoint(shape[i]));
            }

            allPoints.AddRange(shape);
        }

        return allPoints;
    }

    private List<Vector2[]> getChildrenPaths(Transform parent)
    {
        List<Vector2[]> physicsShapes = new List<Vector2[]>();

        foreach (Transform child in parent)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) continue;

            Sprite sprite = spriteRenderer.sprite;
            List<Vector2> shape = new List<Vector2>();
            sprite.GetPhysicsShape(0, shape);

            // Convert local shape to world space
            for (int i = 0; i < shape.Count; i++)
            {
                shape[i] = parent.InverseTransformPoint(child.TransformPoint(shape[i]));
            }

            physicsShapes.Add(shape.ToArray());
        }

        return physicsShapes;

    }

    // SETTING POLYGON COLLIDER PATHS
    public void set_polygon_paths(PolygonCollider2D polygon,List<Vector2[]> path)
    {
        polygon.pathCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            polygon.SetPath(i, path[i]);
        }
    }

    // CONVEX HULL
    private void setConvexPolygonShape(PolygonCollider2D polygon)
    {
        // we get the physics shapes from the children of all the sources
        List<Vector2> points = new List<Vector2>();
        foreach (Transform source in sources)
        {
            points.AddRange(getChildrenPoints(source));
        }
        List<Vector2> mergedPath = compute_convex_hull(points);

        // we check if we have enough points to create a polygon
        if (mergedPath.Count < 3)
        {
            if (debug) {Debug.LogWarning("Not enough points to create a polygon");}
            return;
        }
        
        // we set the path of the polygon collider
        set_polygon_paths(polygon, new List<Vector2[]> { mergedPath.ToArray() });
    }    
    private List<Vector2> compute_convex_hull(List<Vector2> points)
    {
        if (points.Count < 3) return points; // Not enough points for a polygon

        // Sort points by x, then by y (to ensure deterministic order)
        points = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

        List<Vector2> hull = new List<Vector2>();

        // Lower hull
        foreach (Vector2 p in points)
        {
            while (hull.Count >= 2 && cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Upper hull
        int lowerCount = hull.Count + 1;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i];
            while (hull.Count >= lowerCount && cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1); // Remove duplicate point
        return hull;
    }
    private float cross(Vector2 O, Vector2 A, Vector2 B)
    {
        return (A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x);
    }

    // CONCAVE HULL
    private void setConcavePolygonShape(PolygonCollider2D polygon)
    {
        // we get the physics shapes from the children of all the sources
        List<Vector2> points = new List<Vector2>();
        foreach (Transform source in sources)
        {
            points.AddRange(getChildrenPoints(source));
        }
        List<Vector2> mergedPath = compute_concave_hull(points, alpha);

        // we check if we have enough points to create a polygon
        if (mergedPath.Count < 3)
        {
            if (debug) { Debug.LogWarning("Not enough points to create a polygon"); }
            return;
        }

        // we set the path of the polygon collider
        set_polygon_paths(polygon, new List<Vector2[]> { mergedPath.ToArray() });
    }
    /* private List<Vector2> getConcaveHull(Transform parent)
        {
            List<Vector2> allPoints = new List<Vector2>();

            foreach (Transform child in parent)
            {
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null || spriteRenderer.sprite == null) continue;

                Sprite sprite = spriteRenderer.sprite;
                List<Vector2> shape = new List<Vector2>();
                sprite.GetPhysicsShape(0, shape);

                // Convert shape to parent-local space
                for (int i = 0; i < shape.Count; i++)
                {
                    shape[i] = parent.InverseTransformPoint(child.TransformPoint(shape[i]));
                }

                allPoints.AddRange(shape);
            }

            return compute_concave_hull(allPoints, alpha);
        } */
    List<Vector2> compute_concave_hull(List<Vector2> points, float alpha)
    {
        if (points.Count < 3) return points;

        // Step 1: Sort points
        points = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

        // Step 2: Compute the Delaunay Triangulation (approximated with nearest neighbors)
        List<(Vector2, Vector2)> edges = new List<(Vector2, Vector2)>();

        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                if ((points[i] - points[j]).sqrMagnitude <= alpha * alpha) // Only connect close points
                {
                    edges.Add((points[i], points[j]));
                }
            }
        }

        // Step 3: Build the concave hull
        List<Vector2> hull = new List<Vector2>();
        Vector2 start = points[0];
        hull.Add(start);
        Vector2 current = start;
        Vector2 previous = start + Vector2.right; // Dummy start direction

        while (true)
        {
            Vector2 next = Vector2.zero;
            float smallestAngle = 360f;
            bool found = false;

            foreach (var edge in edges)
            {
                if (edge.Item1 == current || edge.Item2 == current)
                {
                    Vector2 candidate = (edge.Item1 == current) ? edge.Item2 : edge.Item1;
                    float angle = Vector2.SignedAngle(previous - current, candidate - current);

                    if (angle > 0 && angle < smallestAngle)
                    {
                        next = candidate;
                        smallestAngle = angle;
                        found = true;
                    }
                }
            }

            if (!found || next == start) break;

            hull.Add(next);
            previous = current;
            current = next;
        }

        return hull;
    }

}