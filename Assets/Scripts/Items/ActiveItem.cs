using UnityEngine;

public class ActiveItem : Item
{

    // item qui possède une action éxecutable via le bouton d'action
    // exemple : hack, dmg_hack, heal_potion, ...

    [Header("Activation")]
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float current_cooldown = 0f;

    public string action_button = "UseItem";
    // public bool is_mouse_button = false;



    // unity functions

    protected new void Awake()
    {
        base.Awake();
        action_type = "active";
    }

    protected new void Update()
    {
        base.Update();

        // on met à jour le cooldown
        if (current_cooldown > 0f)
        {
            current_cooldown -= Time.deltaTime;
            if (current_cooldown <= 0f)
            {
                current_cooldown = 0f;
                ui_bg.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }


    public virtual bool can_use()
    {
        return current_cooldown <= 0f;
    }

    public virtual void use()
    {
        Debug.Log(item_name + " used");

        // on met à jour le cooldown
        current_cooldown = cooldown;

        // on met à jour le sprite du ui_bg
        ui_bg.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
    }

}