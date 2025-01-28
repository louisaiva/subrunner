using UnityEngine;

public class XP_Emitter : MonoBehaviour
{
    // emit some random particles at a given position
    public XPProvider xp_provider;
    public float emission_speed = 1f; // particles per second
    public float timer = 0f;

    private void Start()
    {
        xp_provider = GameObject.Find("/utils/particles/xp_provider").GetComponent<XPProvider>();
    }

    void Update()
    {
        if (timer <= 0)
        {
            xp_provider.EmitXP(1, transform.position, 1f);
            timer = 1f / emission_speed;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}