using UnityEngine;

public class Hackray : MonoBehaviour
{
    
    [SerializeField] protected GameObject hacker;
    [SerializeField] protected GameObject target;

    // offsets
    protected Vector3 hacker_offset;
    protected Vector3 target_offset;

    protected bool isHacking = false;

    // SR
    protected GameObject sr;

    // unity functions
    void Start()
    {
        // on récupère le sprite renderer
        sr = transform.Find("sr").gameObject;
    }

    // unity functions
    void Update()
    {

        updateTransform();
    }

    protected void updateTransform()
    {
        if (sr == null)
        {
            // on récupère le sprite renderer
            sr = transform.Find("sr").gameObject;
        }
        
        if (!isHacking || hacker == null || target == null)
        {
            sr.GetComponent<SpriteRenderer>().enabled = false;
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
        sr.transform.localScale = new Vector3(sr.transform.localScale.x, distance * sr.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit, sr.transform.localScale.z);

        // set rotation (from hacker to target)
        Vector3 dir = target_pos - sr.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        sr.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // main functions
    public void SetHackerAndTarget(GameObject hacker, GameObject target)
    {
        if (sr == null)
        {
            // on récupère le sprite renderer
            sr = transform.Find("sr").gameObject;
        }

        // set hacker and target
        this.hacker = hacker;
        this.target = target;

        // set offsets
        hacker_offset = hacker.transform.Find("center").transform.localPosition;
        target_offset = target.transform.Find("hack_point").transform.localPosition;

        // lesgo
        isHacking = true;

        // enable sprite renderer
        sr.GetComponent<SpriteRenderer>().enabled = true;

        // update transform
        updateTransform();

    }

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

        // on arrête le hack
        isHacking = false;
    }

    protected void switchTargetAndHacker()
    {
        // switch hacker and target
        GameObject temp = hacker;
        hacker = target;
        target = temp;

        Vector3 temp_offset = hacker_offset;
        hacker_offset = target_offset;
        target_offset = temp_offset;
    }

}