public interface Usable
{
    void Use(Capable user);
    public string UseLabel { get; }
}

public interface Inspectable
{
    void Inspect();
    public string InspectLabel { get; }
}