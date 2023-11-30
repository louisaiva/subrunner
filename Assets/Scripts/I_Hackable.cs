using UnityEngine;

public interface I_Hackable
{

    // bit_provider
    GameObject bit_provider { get; set; }

    // hack
    string hack_type_self { get; set;}
 
    // bits necessaires pour hacker
    int required_bits_base { get; set;}
    int required_bits { get; set;}
    int security_lvl { get; set;}

    // timing
    bool is_getting_hacked { get; set;}
    float hacking_duration_base { get; set;} // temps de hack de base -> dépend du niveau de hack
    float hacking_end_time { get; set;} // temps de fin du hack
    float hacking_current_duration { get; set;} // temps de hack actuel



    // fonctions
    void initHack();
    int beHacked(); // renvoie le nombre de bits nécessaires pour hacker
    bool isGettingHacked();
    bool isHackable(string hack_type, int lvl = 1000);
    void updateHack();
    void cancelHack(); // annule le hack et drop les bits restants (non utilisés par le hack) (en fonction du temps de hack restant)
    void succeedHack(); // réussit le hack
}