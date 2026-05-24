using UnityEngine;

public class CooldownCapacity : Capacity
{
    public override bool Able
    {
        get
        {
            // if there is no cooldown, we return true
            if (cooldown == 0) { return true; }

            // if the cooldown is done, we return true
            return cooldown_timer <= 0;
        }
    }

    [Header("Cooldown")]
    [SerializeField] protected float cooldown = 0;
    protected float cooldown_timer;

    // UPDATE & START COOLDOWN
    protected virtual void Update()
    {
        // if the cooldown is not set, we return
        if (cooldown == 0) { return; }

        // if the cooldown is running, we update it
        if (cooldown_timer > 0)
        {
            cooldown_timer -= Time.deltaTime;
        }
    }
    protected void startCooldown(float? custom_cooldown = null)
    {
        // we set the cooldown timer if there is one
        if (custom_cooldown != null)
        {
            cooldown = (float)custom_cooldown;
        }

        if (cooldown > 0)
        {
            cooldown_timer = cooldown;
        }
    }
}