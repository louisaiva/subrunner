using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CursorHandler : MonoBehaviour
{
    [Header("CURSOR")]
    private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private CursorMode cursorMode = CursorMode.Auto;
    private Dictionary<string, Vector2> hotSpots = new Dictionary<string, Vector2>();
    private string currentCursor = "arrow";


    void Awake()
    {
        // on récupère les textures des différents curseurs
        textures.Add("arrow", Resources.Load<Texture2D>("sprites/cursors/arrow"));
        textures.Add("hand", Resources.Load<Texture2D>("sprites/cursors/hand"));
        textures.Add("target", Resources.Load<Texture2D>("sprites/cursors/target"));

        // on définit les hotspots des différents curseurs
        hotSpots.Add("arrow", new Vector2(0, 0));
        hotSpots.Add("hand", new Vector2(16, 0));
        hotSpots.Add("target", new Vector2(15, 15));

        // on définit le curseur par défaut
        SetCursor("arrow");
    }

    /* void Update()
    {
        // on change le cursor toutes les 5 secondes
        if (Time.frameCount % 300 == 0)
        {
            string newCursor = "";
            if (currentCursor == "arrow") { newCursor = "hand"; }
            else if (currentCursor == "hand") { newCursor = "target"; }
            else if (currentCursor == "target") { newCursor = "arrow"; }

            SetCursor(newCursor);
        }
    } */


    public void SetCursor(string cursorName)
    {
        Cursor.SetCursor(textures[cursorName], hotSpots[cursorName], cursorMode);
        currentCursor = cursorName;
    }

}