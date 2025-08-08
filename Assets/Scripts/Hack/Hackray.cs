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

    // callbacks
    // private event System.Action laptop_dropped_callback;
    // private event System.Action<Capable> laptop_grabbed_callback;


    [Header("Components")]
    protected SpriteRenderer sr;

    // AWAKE
    void Awake()
    {
        sr = transform.Find("sr").GetComponent<SpriteRenderer>();

        // create callbacks
        // laptop_dropped_callback = handleLaptopDropped;
        // laptop_grabbed_callback = handleLaptopGrabbed;
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
        target_offset = hackable.HackPoint;
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


    /* public void SetHackerAndTarget(Transform hacker, Transform target, Vector3 hacker_offset = default, Vector3 target_offset = default)
    {
        // set hacker and target
        this.hacker = hacker;
        this.target = target;

        // set offsets
        // hacker_offset = hacker.transform.Find("center").transform.localPosition;
        // target_offset = target.transform.Find("hack_point").transform.localPosition;

        if (hacker_offset != default) { this.hacker_offset = hacker_offset; }
        this.target_offset = target_offset;

        // enable sprite renderer
        sr.GetComponent<SpriteRenderer>().enabled = true;

        // update transform
        // updateTransform();

    } */
    public void RemoveHackerAndTarget()
    {
        // on désactive le sprite rrenderer
        sr.GetComponent<SpriteRenderer>().enabled = false;

        // on enlève le hacker et la target
        hacker = null;
        target = null;

        // on enlève les offsets
        hacker_offset = Vector3.zero;
        target_offset = Vector3.zero;

    }


    // CALLBACKS

    private void handleLaptopDropped()
    {
        this.hacker_offset = laptop.GetCapacity<HackCapacity>().transform.localPosition;
    }
    private void handleLaptopGrabbed(Capable grabber)
    {
        if (grabber is not Being)
        {
            this.hacker_offset = laptop.GetCapacity<HackCapacity>().transform.localPosition;
            return;
        }

        // the grabber is a Being. we find the body
        this.hacker_offset = grabber.transform.Find("body").localPosition;
    }
    private void OnDisable()
    {
        if (laptop == null) { return; }

        laptop.OnDropped -= handleLaptopDropped;
        laptop.OnGrabbed -= handleLaptopGrabbed;
    }

}