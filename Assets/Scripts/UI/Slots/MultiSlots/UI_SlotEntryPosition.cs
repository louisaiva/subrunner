using UnityEngine;

public class UI_SlotEntryPosition : MonoBehaviour
{
    public Vector2 GetPosition()
    {
        // if we are here we have a canvas slot -> means we have a recttransform
        RectTransform rect_transform = GetComponent<RectTransform>();
        Vector2 position = rect_transform.TransformPoint ( rect_transform.rect.center );
        string s = "(UI_SlotEntryPosition) GetPosition: entry " + name + " position : " + position;

        // on regarde si le slot est positionné dans un canvas world space or screen space
        if (gameObject.layer == LayerMask.NameToLayer("UI_World"))
        {
            // on le convertit en position
            position = Camera.main.WorldToScreenPoint(position);
            s += " position (screen) : " + position + "\n";
        }
        if (UI_Navigator.Instance.log_slot_position) { Debug.Log(s); }

        return position;
    }
}