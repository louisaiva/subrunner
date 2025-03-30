using UnityEngine;

public interface I_Descriptable
{

    // hoover
    bool is_hovered { get; set; }

    // getters
    string getDescription();
    bool shouldDescriptionBeShown();
}