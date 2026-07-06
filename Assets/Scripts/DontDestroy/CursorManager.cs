using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTex;
    public Vector2 hotSpot = Vector2.zero;
    void Awake()
    {
        Cursor.SetCursor(cursorTex, hotSpot, CursorMode.ForceSoftware);
    }
}