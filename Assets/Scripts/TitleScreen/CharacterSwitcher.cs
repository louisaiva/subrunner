using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("First anim parameters")]
    [SerializeField] private string first_skin = "cat";
    [SerializeField] private string first_capacity = "idle";
    [SerializeField] private float nb_loops_first_anim = 3;

    [Header("Skins")]
    [SerializeField] private string[] skins;
    [SerializeField] private string[] capacities = new string[] { "walk", "idle", "dodge", "attack", "die", "hurted", "run" };
    private List<Anim> anims = new List<Anim>();
    [SerializeField] private float skinChangeDelay = 0.5f; // Delay between skin changes
    private bool auto_switch = false;

    [Header("Components")]
    [SerializeField] private UI_AnimPlayer animPlayer;
    
    // AWAKE
    private void Awake()
    {
        // get the ui_animplayer component
        animPlayer = GetComponent<UI_AnimPlayer>();
    }
    private void Start()
    {
        // we get all the anims we want
        AnimBank bank = AnimBank.Instance;
        foreach (string skin in skins)
        {
            foreach (string capacity in capacities)
            {
                // get the anims for this skin and capacity
                List<Anim> animsForSkin = bank.GetOrientationAnims(skin, capacity);
                if (animsForSkin != null && animsForSkin.Count > 0)
                {
                    anims.AddRange(animsForSkin);
                }
            }
        }


        // Start the skin change coroutine
        StartCoroutine(SkinChangeCoroutine());
    }
    private void OnEnable()
    {
        if (anims.Count == 0) { return; }
        
        // Restart the skin change coroutine
        StartCoroutine(SkinChangeCoroutine());
    }

    // SKIN SWITCHING
    private IEnumerator SkinChangeCoroutine()
    {
        // wait a frame to be sure animPlayer is ready
        yield return null;

        int loops = 0;

        // while !auto_switch we play the cat idle anim
        while (!auto_switch && loops < nb_loops_first_anim)
        {
            loops++;
            if (!animPlayer.IsPlayingCapacity(first_capacity) || animPlayer.skin != first_skin)
            {    
                // we play the cat idle anim
                animPlayer.skin = first_skin;
                animPlayer.Play(first_capacity);
            }
            // Wait for the specified delay
            yield return new WaitForSecondsRealtime(skinChangeDelay);
        }

        while (true)
        {
            // Wait for the specified delay
            yield return new WaitForSecondsRealtime(skinChangeDelay);

            // and to a random anim between "walk","idle","dodge","loop"
            Anim anim = anims[Random.Range(0, anims.Count)];
            animPlayer.skin = anim.skin;
            animPlayer.Play(anim.capacity);
        }
    }
    public void AutoSwitchFromNow()
    {
        auto_switch = true;
    }
}