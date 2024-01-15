using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface I_UI_Slottable
{
    // void clickOnItem(Item item);
    List<GameObject> GetSlots(ref Vector2 base_position, ref float angle_threshold, ref float angle_multiplicator);
}