using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Hackray : MonoBehaviour
{
    
    [SerializeField] protected GameObject hacker;
    [SerializeField] protected GameObject target;

    // offsets
    protected Vector3 hacker_offset;
    protected Vector3 target_offset;

    protected bool isHacking = false;

    // unity functions
    void Update()
    {
        
    }

    // main functions
    public void SetHackerAndTarget(GameObject hacker, GameObject target)
    {
        // set hacker and target
        this.hacker = hacker;
        this.target = target;

        // set offsets
        hacker_offset = hacker.transform.Find("center").transform.localPosition;
        target_offset = target.transform.Find("hack_point").transform.localPosition;

        // lesgo
        isHacking = true;

        // enable sprite renderer
        GetComponent<SpriteRenderer>().enabled = true;
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