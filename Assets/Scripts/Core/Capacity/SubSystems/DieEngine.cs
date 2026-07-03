using System.Collections.Generic;
using UnityEngine;

public class DieEngine : MonoBehaviour
{

    // SINGLETON LOGIC
    public static DieEngine Instance;
    protected virtual void Awake()
    {
        Instance = this;
    }

    [Header("Die parameters")]
    [SerializeField] private bool show_smiley = true;
    [SerializeField] private List<string> smileys = new List<string> { "RIP", "rip", ";-;", ":(", "://" };

    [Header("Logs")]
    public Loggable<DieEngine> log;


    // USE
    public void Die(Capable dying_capable, Capable killer = null)
    {
        if (log.Verbose >= Verbosity.Extended)
        {
            if (killer != null)
            {
                log.LogExtended($"{killer.ID} killed {dying_capable.ID} !!");
            }
            else
            {
                log.LogExtended($"{dying_capable.ID} is dying !!");
            }
        }

        // on donne un floating dmg
        if (show_smiley)
        {
            Vector3 sprite_center = dying_capable.transform.position + new Vector3(0, dying_capable.AnimPlayer.Renderer.bounds.size.y / 2f, 0);
            float test = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < smileys.Count; i++)
            {
                if (test < 100 / smileys.Count * (i + 1))
                {
                    FloatingDmgProvider.Instance.TextManager.addFloatingText(smileys[i], sprite_center, "red");
                    break;
                }
            }
        }


    }
}