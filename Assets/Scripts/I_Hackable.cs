public interface I_Hackable
{
    int required_hack_lvl { get; }
    // bool is_getting_hacked { get; }

    bool beHacked(int lvl);

    bool IsGettingHacked();
}