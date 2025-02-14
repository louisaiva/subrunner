using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public interface I_UI_Slot : I_Descriptable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    System.Action<InputAction.CallbackContext> ActivateCallback { get;}
}
