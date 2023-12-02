using UnityEngine;

public interface I_Descriptable
{

    // hoover
    bool is_hoovered { get; set; }

    // getters
    string getDescription();
    bool shouldDescriptionBeShown();
}