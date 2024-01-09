using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HackrayHoover : Hackray
{

    // targets
    private GameObject perso;

    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = transform.parent.gameObject;
    }

    void Update()
    {
        // base.Update();

        if (!isHacking || hacker == null || target == null)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }

        // on verifie si le y du hacker est plus grand que le y de la target
        if (hacker.transform.position.y > target.transform.position.y)
        {
            // si c'est le cas, on switch les deux
            switchTargetAndHacker();
        }

        // set position
        transform.position = hacker.transform.position + hacker_offset;
        Vector3 target_pos = target.transform.position + target_offset;

        // set scale
        float distance = Vector3.Distance(transform.position, target_pos);
        transform.localScale = new Vector3(transform.localScale.x, distance * GetComponent<SpriteRenderer>().sprite.pixelsPerUnit, transform.localScale.z);

        // set rotation (from hacker to target)
        Vector3 dir = target_pos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // functions
    public void setTarget(GameObject target)
    {
        SetHackerAndTarget(perso, target);
    }

    // showing
    public void show()
    {
        // on active le sprite rrenderer
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void hide()
    {
        // on désactive le sprite rrenderer
        GetComponent<SpriteRenderer>().enabled = false;
    }
}