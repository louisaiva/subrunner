using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public interface I_UI_Slot : I_Descriptable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // MonoBehaviour functions
    GameObject gameObject { get; }
}
