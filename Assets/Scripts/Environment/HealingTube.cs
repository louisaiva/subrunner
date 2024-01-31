using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
public class HealingTube : MonoBehaviour, I_Interactable
{

    // ANIMATION
    protected AnimationHandler anim_handler;
    protected HealingTubeAnims anims = new HealingTubeAnims();

    [Header("HEALING")]
    [SerializeField] protected float healing_duration = 1f;
    [SerializeField] protected GameObject healing_target;
    [SerializeField] protected Vector2 healing_target_offset = new Vector3(0f, 0.5f);
    [SerializeField] protected Vector2 end_healing_target_pos = new Vector3(0f, -0.2f);

    // [Header("INTERACTION")]
    protected bool is_interacting = false; // est en train d'interagir


    [Header("consumable")]
    [SerializeField] protected bool is_consumed = false;
    [SerializeField] protected float delay_reconsume = 3f;
    [SerializeField] protected float delay_reconsume_timer = 0f;



    // unity functions
    protected virtual void Awake()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
    }

    void Update()
    {
        if (is_consumed)
        {
            delay_reconsume_timer += Time.deltaTime;
            if (delay_reconsume_timer >= delay_reconsume)
            {
                is_consumed = false;
                delay_reconsume_timer = 0f;
            }
        }
    }

    // HEALING
    protected void heal()
    {
        CancelInvoke();

        // on knocked out le perso et on le déplace dans le tube
        healing_target.GetComponent<Being>().addCapacity("knocked_out");
        healing_target.GetComponent<Being>().addCapacity("invicible");
        healing_target.GetComponent<Being>().MOVE(transform.position + (Vector3)healing_target_offset);

        // on joue l'animation
        anim_handler.ChangeAnim(anims.healing, healing_duration);


        // on finit de healer
        Invoke("success_heal", healing_duration);
    }

    protected virtual void success_heal()
    {

        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle);

        // on heal le perso
        healing_target.GetComponent<Being>().healMax();

        // on arrête le knocked out et on le déplace hors du tube
        healing_target.GetComponent<Being>().MOVE(transform.position + (Vector3)end_healing_target_pos);
        healing_target.GetComponent<Being>().removeCapacity("knocked_out");
        healing_target.GetComponent<Being>().removeCapacity("invicible");

        stopInteract();

        // on remove le target
        healing_target = null;
    }


    // INTERACTIONS
    public bool isInteractable()
    {
        return !is_interacting && !is_consumed;
    }

    public void interact(GameObject target)
    {
        if (!is_interacting)
        {
            print("interact with " + gameObject.name);

            // on set la target
            healing_target = target;

            // on CONSUME le tube
            is_consumed = true;

            // interaction
            is_interacting = true;

            heal();
        }
    }

    public void stopInteract()
    {
        print("stop interact with " + gameObject.name);

        // on arrête l'interaction
        is_interacting = false;
    }

}


public class HealingTubeAnims
{

    // ANIMATIONS
    public string idle = "healing_tube_idle";
    public string healing = "healing_tube_healing";

}