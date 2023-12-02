using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Skill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    // interface functions
    public void OnPointerEnter(PointerEventData eventData)
    {
        // on met à jour le skill
        // print("on pointer enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // on met à jour le skill
        // print("on pointer exit");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // on met à jour le skill
        // print("on pointer click");
    }
}