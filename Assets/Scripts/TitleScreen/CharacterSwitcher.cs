using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{

    [Header("Skins")]
    [SerializeField] private string[] skins;
    [SerializeField] private string[] capacities = new string[] { "walk", "idle", "dodge", "attack", "die", "hurted", "run" };
    private List<Anim> anims = new List<Anim>();
    [SerializeField] private float skinChangeDelay = 0.5f; // Delay between skin changes

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

    // SKIN SWITCHING
    private IEnumerator SkinChangeCoroutine()
    {
        // wait a frame to be sure animPlayer is ready
        yield return null;

        while (true)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(skinChangeDelay);

            // and to a random anim between "walk","idle","dodge","loop"
            Anim anim = anims[Random.Range(0, anims.Count)];
            animPlayer.skin = anim.skin;
            animPlayer.Play(anim.capacity);
        }
    }
}