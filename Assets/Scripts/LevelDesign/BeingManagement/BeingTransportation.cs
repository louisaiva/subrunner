using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BeingTransportation : MonoBehaviour
{
    // public Room room; // the room to check collisions on
    public World world;
    public Transform TransitionParent;

    public void Start()
    {
        // room = GetComponent<Room>();
        world = GameObject.Find("/world").GetComponent<World>();

        TransitionParent = transform.Find("beings");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if it is a being
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Beings"))) { return; }
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }
        if (being.name == "perso") { return; }

        // we move it to the transition parent
        being.transform.SetParent(TransitionParent);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if it is a being
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Beings"))) { return; }
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }
        if (being.name == "perso") { return; }

        // we move it back to the world
        being.transform.SetParent(world.current_level.being_parent);
    }


}
    