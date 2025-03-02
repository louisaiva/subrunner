using UnityEngine;
/// <summary>
/// Item is a Movable that can be grabbed by other Capables with GrabCapacity + InteractCapacity.
/// </summary>
public class Item : Movable
{

    // Grabbable
    private bool _grabbed = false;
    public bool Grabbed { get => _grabbed;
        set
        {
            // check if the value is the same
            if (value == _grabbed) { return; }

            // we set the value
            _grabbed = value;
            if (_grabbed) { on_grabbed(); }
            else { on_dropped(); }
        }
    }

    // BEING GRABBED / DROPPED
    private void on_grabbed()
    {
        // we remove the rigidbody
        Destroy(rb);
        
        // we disable the HoverCapacity's collider
        GetCapacity<HoverCapacity>().transform.GetComponent<Collider2D>().enabled = false;

        // we disable the feet collider
        feet_collider.enabled = false;

        // we disable the sprite renderer
        GetComponent<SpriteRenderer>().enabled = false;

        // we set the effect IsBeingCarried to -888f (infinite time)
        AddEffect(Effect.BeingCarried, -888f);
        
        // we remove all the forces
        ClearForces();
    }
    private void on_dropped()
    {
        // we add the rigidbody
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // we enable the HoverCapacity's collider
        GetCapacity<HoverCapacity>().transform.GetComponent<Collider2D>().enabled = true;

        // we enable the feet collider
        feet_collider.enabled = true;

        // we enable the sprite renderer
        GetComponent<SpriteRenderer>().enabled = true;

        // we remove the effect IsBeingCarried
        RemoveEffect(Effect.BeingCarried);
    }


}