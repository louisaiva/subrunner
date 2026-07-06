using UnityEngine;

public class UI_AnchorLerp : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float lerp_speed = 5f;
    
    [Header("Lerp Parameters")]
    [SerializeField] private bool lerp_y_anchors = false;
    [SerializeField] private Vector2 y_anchors_target;
    [SerializeField] private bool lerp_x_anchors = false;
    [SerializeField] private Vector2 x_anchors_target;

    private Vector2 initial_y_anchors;
    private Vector2 initial_x_anchors;

    private Vector2? y_target;
    private Vector2? x_target;
    private void OnEnable()
    {
        initial_y_anchors = new Vector2(target.anchorMin.y, target.anchorMax.y);
        initial_x_anchors = new Vector2(target.anchorMin.x, target.anchorMax.x);

        if (lerp_y_anchors) { y_target = y_anchors_target; }
        if (lerp_x_anchors) { x_target = x_anchors_target; }
    }

    private void Update()
    {
        if (target == null) { return; }

        if (y_target != null)
        {
            target.anchorMin = Vector2.Lerp(target.anchorMin, new Vector2(target.anchorMin.x, y_target.Value.x), Time.deltaTime * (lerp_speed/4f));
            target.anchorMax = Vector2.Lerp(target.anchorMax, new Vector2(target.anchorMax.x, y_target.Value.y), Time.deltaTime * (lerp_speed/4f));
        }

        if (x_target != null)
        {
            target.anchorMin = Vector2.Lerp(target.anchorMin, new Vector2(x_target.Value.x, target.anchorMin.y), Time.deltaTime * (lerp_speed/4f));
            target.anchorMax = Vector2.Lerp(target.anchorMax, new Vector2(x_target.Value.y, target.anchorMax.y), Time.deltaTime * (lerp_speed/4f));
        }
    }

    public void ResetAnchors()
    {
        if (lerp_x_anchors) { x_target = initial_x_anchors; }
        if (lerp_y_anchors) { y_target = initial_y_anchors; }
    }
}