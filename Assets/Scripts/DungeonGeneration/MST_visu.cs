using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MST_visu : MonoBehaviour {

    public List<Edge> mst;
    public List<Edge> loops;
    public GameObject linePrefab;
    public GameObject linePrefabLoops;

    // unity
    public void init(List<Edge> mst, List<Edge> loops) {


        // line prefabs
        linePrefab = Resources.Load<GameObject>("prefabs/ui/line2");
        linePrefabLoops = Resources.Load<GameObject>("prefabs/ui/line");

        // mst and loops
        this.mst = mst;
        this.loops = loops;

        drawMST();
    }

    void drawMST()
    {
        foreach (Edge edge in mst)
        {
            Vector2 p1 = edge.p1;
            Vector2 p2 = edge.p2;
            GameObject line = Instantiate(linePrefab, transform);

            line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(p1.x, p1.y, 0));
            line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(p2.x, p2.y, 0));
        }

        foreach (Edge edge in loops)
        {
            Vector2 p1 = edge.p1;
            Vector2 p2 = edge.p2;
            GameObject line = Instantiate(linePrefabLoops, transform);

            line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(p1.x, p1.y, 0));
            line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(p2.x, p2.y, 0));
        }
    }

}