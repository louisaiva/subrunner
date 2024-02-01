using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class ConnectorSector : Sector
{

    // INIT FUNCTIONS
    public void init(int x,int y,int w,int h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;


        if (w >= 3 && h == 1)
        {
            // on remplit le milieu avec des corridors
            for (int i = 0; i < w; i++)
            {
                corridors.Add(new Vector2Int(i, 0));
            }
        }
        else if (w == 1 && h >= 3)
        {
            // on remplit le milieu avec des corridors
            for (int i = 0; i < h; i++)
            {
                corridors.Add(new Vector2Int(0, i));
            }
        }

        tiles.UnionWith(corridors);
    }

}