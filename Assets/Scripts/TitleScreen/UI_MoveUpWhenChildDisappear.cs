using UnityEngine;
/// <summary>
/// this class is a simple class that checks if the last child rect is below screen and rise
/// our transform accordingly (to simulate a linux loading screen style)
/// </summary>
public class UI_MoveUpWhenChildDisappear : MonoBehaviour
{
    [SerializeField] private bool log = false;

    private void Update()
    {
        // we check if the transform has children
        if (transform.childCount == 0) { return; }

        // we check if the last child is below the screen
        RectTransform lastChild = transform.GetChild(transform.childCount - 1).GetComponent<RectTransform>();

        // normally it is automatically in screen pos bcz parent is fullscreen + pivot in center
        float child_min_y = lastChild.localPosition.y + Screen.height/2f + transform.localPosition.y;

        if (log) { Debug.Log("Child " + lastChild.name + " min y: " + child_min_y); }

        if (child_min_y > 0) { return; }

        // if the child is below the screen, we move up
        GetComponent<RectTransform>().anchoredPosition += new Vector2(0, Mathf.Abs(child_min_y));
        // Debug.Log("Moving up " + name + " by " + Mathf.Abs(child_min_y) + " pixels because last child is below screen");
    }
}