public interface Activable {}

public interface Usable : Activable
{
    void Use(Capable user);
    public string UseLabel { get; }
}

public interface Inspectable : Activable
{
    void Inspect();
    public string InspectLabel { get; }
}