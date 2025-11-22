using UnityEngine;

public class UI_AlwaysKeepSiblingIndex : MonoBehaviour
{
    [SerializeField] private int sibling_index = 0;
    [SerializeField] private bool from_end = false;

    private void LateUpdate()
    {
        int index = transform.GetSiblingIndex();
        int desired_index = from_end ? transform.parent.childCount - 1 - sibling_index : sibling_index;
        if (index == desired_index) { return; }
        transform.SetSiblingIndex(desired_index);
    }
}