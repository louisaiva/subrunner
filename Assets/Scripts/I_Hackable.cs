public interface I_Hackable
{
    // hack
    int required_hack_lvl { get; set;}
    string hack_type_self { get; set;}
 
    // timing
    bool is_getting_hacked { get; set;}
    float hacking_duration_base { get; set;} // temps de hack de base -> dépend du niveau de hack
    float hacking_end_time { get; set;} // temps de fin du hack


    // fonctions

    void initHack();
    bool beHacked(int lvl);

    bool IsGettingHacked();

    bool IsHackable(string hack_type, int lvl = 1000);
}