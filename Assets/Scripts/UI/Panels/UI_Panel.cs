using System.Collections.Generic;
using UnityEngine;

public class UI_Panel : MonoBehaviour
{
    [HideInInspector] public RectTransform panelTransform;
    public List<Vector2> anchors;

    private void Awake()
    {
        this.panelTransform = GetComponent<RectTransform>();
    }
}