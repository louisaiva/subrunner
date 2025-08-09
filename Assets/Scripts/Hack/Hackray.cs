using UnityEngine;

public class Hackray : MonoBehaviour
{
    [Header("Laptop")]
    [SerializeField] protected Laptop laptop;

    [Header("Hackray Settings")]
    [SerializeField] protected Transform hacker;
    [SerializeField] protected Transform target;

    // offsets
    protected Vector3 hacker_offset;
    protected Vector3 target_offset;


    [Header("Components")]
    protected SpriteRenderer sr;

    // AWAKE
    void Awake()
    {
        sr = transform.Find("sr").GetComponent<SpriteRenderer>();
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
        Transform temp = hacker;
        hacker = target;
        target = temp;

        Vector3 temp_offset = hacker_offset;
        hacker_offset = target_offset;
        target_offset = temp_offset;
    }

    // SETTERS
    public void SetLaptopAndHackable(Laptop laptop, Hackable hackable)
    {
        // todo checks if laptop is on ground or not & if on ground the hacker is the laptop
        // otherwise it is its holder, and subscribe to item.OnGroundDropped & item.OnGrabbed
        // to change it. for this we need to store callbacks and properly removed them ondisable

        this.laptop = laptop;

        // set hacker and target
        this.hacker = laptop.transform;
        this.target = hackable.transform;

        // set offsets
        target_offset = hackable.transform.Find("processor").localPosition;
        if (laptop.Grabbed) { handleLaptopGrabbed(laptop.Holder); }
        else { handleLaptopDropped(); }

        // register to events
        laptop.OnDropped += handleLaptopDropped;
        laptop.OnGrabbed += handleLaptopGrabbed;

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

    // CALLBACKS
    private void handleLaptopDropped()
    {
        this.hacker_offset = laptop.transform.Find("processor").localPosition;
    }
    private void handleLaptopGrabbed(Capable grabber)
    {
        if (grabber is not Being)
        {
            this.hacker_offset = laptop.transform.Find("processor").localPosition;
            return;
        }

        // the grabber is a Being. we find the processor
        this.hacker_offset = grabber.transform.Find("processor").localPosition;
    }
    private void OnDisable()
    {
        if (laptop == null) { return; }

        laptop.OnDropped -= handleLaptopDropped;
        laptop.OnGrabbed -= handleLaptopGrabbed;
    }
}