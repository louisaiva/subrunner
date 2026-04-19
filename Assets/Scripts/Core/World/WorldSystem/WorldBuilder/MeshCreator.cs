using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(PolygonCollider2D))]
public class PolyColliderToMesh: MonoBehaviour
{

    PolygonCollider2D _collider;
    Vector2[] _cachedPoints;
    List<int> _triangles = new List<int>();
    Mesh _myMesh;

    // In-editor, poll for collider updates so we can react 
    // to shape changes with realtime interactivity.
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_collider == null)
            Awake();
        else {
            var colliderPoints = _collider.GetPath(0);
            if(colliderPoints.Length == _cachedPoints.Length) {
                bool mismatch = false;
                for(int i = 0; i < colliderPoints.Length; i++) {
                    if (colliderPoints[i] != _cachedPoints[i]) {
                        mismatch = true;
                        break;
                    }
                }
                if (mismatch == false)
                    return;
            }

            Reshape();
        }
    }
    #endif

    // Wire up references and set initial shape.
    private void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        var filter = GetComponent<MeshFilter>();

        // This creates a unique mesh per instance. If you re-use shapes
        // frequently, then you may want to look into sharing them in a pool.
        _myMesh = new Mesh();
        _myMesh.MarkDynamic();

        Reshape();

        filter.sharedMesh = _myMesh;
    }

    // Call this if you edit the collider at runtime 
    // and need the visual to update.
    public void Reshape()
    {
    // For simplicity, we'll only handle colliders made of a single path.
    // This method can be extended to handle multi-part colliders and
    // colliders with holes, but triangulating these gets more complex.
    _cachedPoints = _collider.GetPath(0);

    // Triangulate the loop of points around the collider's perimeter.
    LoopToTriangles();

    // Populate our mesh with the resulting geometry.
    Vector3[] vertices = new Vector3[_cachedPoints.Length];
    for (int i = 0; i < vertices.Length; i++)
        vertices[i] = _cachedPoints[i];

    // We want to make sure we never assign fewer verts than we're indexing.
    if (vertices.Length <= _myMesh.vertexCount)
    {
        _myMesh.triangles = _triangles.ToArray();
        _myMesh.vertices = vertices;
        _myMesh.uv = _cachedPoints;
    }
    else
    {
        _myMesh.vertices = vertices;
        _myMesh.uv = _cachedPoints;
        _myMesh.triangles = _triangles.ToArray();
    }
}

    void LoopToTriangles()
    {
        // This uses a naive O(n^3) ear clipping approach for simplicity.
        // Higher-performance triangulation methods exist if you need to
        // do this at runtime or with high-vertex-count polygons, or
        // polygons with holes & self-intersections.
        _triangles.Clear();

        // Mode switch for clockwise/counterclockwise paths.
        int winding = ComputeWinding(_cachedPoints);

        List<Vector2> ring = new List<Vector2>(_cachedPoints);
        List<int> indices = new List<int>(ring.Count);
        for (int i = 0; i < ring.Count; i++)
            indices.Add(i);

        while (indices.Count > 3)
        {
            int tip;
            for (tip = 0; tip < indices.Count; tip++)
                if (IsEar(ring, tip, winding))
                    break;

            int count = indices.Count;
            int cw = (tip + count + winding) % count;
            int ccw = (tip + count - winding) % count;
            _triangles.Add(indices[cw]);
            _triangles.Add(indices[ccw]);
            _triangles.Add(indices[tip]);
            ring.RemoveAt(tip);
            indices.RemoveAt(tip);
        }

        if (winding < 0)
        {
            _triangles.Add(indices[2]);
            _triangles.Add(indices[1]);
            _triangles.Add(indices[0]);
        }
        else _triangles.AddRange(indices);
    }

    // Returns -1 for counter-clockwise, +1 for clockwise.
    int ComputeWinding(Vector2[] ring)
    {
        float windingSum = 0;
        Vector2 previous = ring[ring.Length - 1];
        for (int i = 0; i < ring.Length; i++)
        {
            Vector2 next = ring[i];
            windingSum += (next.x - previous.x) * (next.y + previous.y);
            previous = next;
        }

        return windingSum > 0f ? 1 : -1;
    }

    // Checks if a given point forms an "ear" of the polygon.
    // (A convex protrusion with no other vertices inside it)
    bool IsEar(List<Vector2> ring, int tip, int winding)
    {
        int count = ring.Count;
        int cw = (tip + count + winding) % count;
        int ccw = (tip + count - winding) % count;
        Vector2 a = ring[cw];
        Vector2 b = ring[tip];
        Vector2 c = ring[ccw];

        Vector2 ab = b - a;
        Vector2 bc = c - b;
        Vector2 ca = a - c;

        // Early-out for concave vertices.
        if (DotPerp(ab, bc) < 0f)
            return false;

        float abThresh = DotPerp(ab, a);
        float bcThresh = DotPerp(bc, b);
        float caThresh = DotPerp(ca, c);

        for (int i = (ccw + 1) % count; i != cw; i = (i + 1) % count)
        {
            Vector2 test = ring[i];
            if (DotPerp(ab, test) > abThresh
                && DotPerp(bc, test) > bcThresh
                && DotPerp(ca, test) > caThresh)
                return false;
        }

        return true;
    }

    // Dot product of the perpendicular of vector a against vector b.
    float DotPerp(Vector2 a, Vector2 b)
{
    return a.x * b.y - a.y * b.x;
}
}