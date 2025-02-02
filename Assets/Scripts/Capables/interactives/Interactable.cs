public interface Interactable
{
    void OnInteract();
}

public interface Openable
{
    public bool is_moving { get; set; }
    public bool is_open { get; set; }
}