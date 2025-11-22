using UnityEngine;

public class Hackray : MonoBehaviour
{
    
    [Header("Hackray Settings")]
    [SerializeField] protected Capable hacker;
    [SerializeField] protected Capable target;

    // offsets
    protected Vector3 hacker_offset;
    protected Vector3 target_offset;
    [SerializeField] protected float base_thickness = 1.8f;
    [SerializeField] protected float waiting_thickness = 1f;


    [Header("Components")]
    protected SpriteRenderer sr;

    // AWAKE
    void Awake()
    {
        sr = transform.Find("sr").GetComponent<SpriteRenderer>();
        sr.transform.localScale = new Vector3(base_thickness, sr.transform.localScale.y, sr.transform.localScale.z);
    }

    // UPDATE
    void Update()
    {
        if (hacker == null || target == null)
        {
            sr.enabled = false;
            return;
        }

        // on verifie si le y du hacker est plus grand que le y de la target
        if (hacker.transform.position.y > target.transform.position.y)
        {
            // si c'est le cas, on switch les deux
            switchTargetAndHacker();
        }

        // set position
        transform.position = hacker.transform.position;
        sr.transform.localPosition = hacker_offset;

        Vector3 target_pos = target.transform.position + target_offset;

        // set scale
        float distance = Vector3.Distance(sr.transform.position, target_pos);
        sr.transform.localScale = new Vector3(sr.transform.localScale.x, distance * sr.sprite.pixelsPerUnit, sr.transform.localScale.z);

        // set rotation (from hacker to target)
        Vector3 dir = target_pos - sr.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        sr.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    protected void switchTargetAndHacker()
    {
        // switch hacker and target
        Capable temp = hacker;
        hacker = target;
        target = temp;

        Vector3 temp_offset = hacker_offset;
        hacker_offset = target_offset;
        target_offset = temp_offset;
    }

    // SETTERS
    public void SetConnectors(ConnectCapacity hacker, ConnectCapacity target)
    {
        // set hacker and target
        this.hacker = hacker.capable;
        this.target = target.capable;

        // set hacker offset
        if (this.hacker is Item hacker_item)
        {
            // calculates the offset
            hacker_offset = calculate_item_offset(hacker_item);

            // sets the callbacks
            hacker_item.OnDropped += handleConnectorDropped;
            hacker_item.OnGrabbed += handleConnectorGrabbed;
        }
        else
        {
            hacker_offset = hacker.transform.localPosition;
        }

        // set target offset
        if (this.target is Item target_item)
        {
            target_offset = calculate_item_offset(target_item);

            // sets the callbacks
            target_item.OnDropped += handleConnectorDropped;
            target_item.OnGrabbed += handleConnectorGrabbed;
        }
        else
        {
            target_offset = target.transform.localPosition;
        }

        // enable sprite renderer
        sr.GetComponent<SpriteRenderer>().enabled = true;

        // update transform
        Update();
    }
    public void SetMaterial(Material material)
    {
        // set the material of the sprite renderer
        sr.GetComponent<SpriteRenderer>().material = material;
    }
    public void SetColor(Color color)
    {
        // set the color of the sprite renderer
        sr.GetComponent<SpriteRenderer>().color = color;
    }
    public void SetThickness(bool waiting = true)
    {
        sr.transform.localScale = new Vector3(waiting ? waiting_thickness : base_thickness, sr.transform.localScale.y, sr.transform.localScale.z);
    }

    // CALLBACKS
    private void handleConnectorDropped(Item item)
    {
        // checks which item it is
        if (item == hacker) { this.hacker_offset = calculate_item_offset(item); }
        else if (item == target) { this.target_offset = calculate_item_offset(item); }
    }
    private void handleConnectorGrabbed(Item item, Capable grabber)
    {
        handleConnectorDropped(item);
    }
    private Vector3 calculate_item_offset(Item item)
    {
        // if it is on the ground we return the connect local_offset
        if (!item.Grabbed)
        {
            return item.GetCapacity<ConnectCapacity>().transform.localPosition;
        }

        // if it is in a Inventory the offset depends on the Holder typer
        // -> being ? -> offset is the body of the skin offset
        Capable holder = item.Holder;
        if (holder is Being being)
        {
            return new Vector3(0f, AnimBank.Instance.GetBodyOffset(being.Skin), 0f);
        }

        // -> capable ? -> offset is the item connect capa local pos
        return item.GetCapacity<ConnectCapacity>().transform.localPosition;
    }
    private void OnDisable()
    {

        if (hacker is Item hacker_item)
        {
            hacker_item.OnDropped -= handleConnectorDropped;
            hacker_item.OnGrabbed -= handleConnectorGrabbed;
        }
        if (target is Item target_item)
        {
            target_item.OnDropped -= handleConnectorDropped;
            target_item.OnGrabbed -= handleConnectorGrabbed;
        }
    }
}