using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{

    [Header("Skins")]
    [SerializeField] private string[] skins;
    [SerializeField] private string[] anims;
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

        // Start the skin change coroutine
        StartCoroutine(SkinChangeCoroutine());
    }

    // SKIN SWITCHING
    private IEnumerator SkinChangeCoroutine()
    {
        // wait a frame to be sure animPlayer is ready
        yield return null;

        // playing run animation

        while (true)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(skinChangeDelay);

            // change to a random skin
            List<string> skinList = new List<string>(skins);
            skinList.Remove(animPlayer.skin);
            animPlayer.skin = skinList[Random.Range(0, skinList.Count)];

            // and to a random anim between "walk","idle","dodge","loop"
            animPlayer.Play(anims[Random.Range(0, anims.Length)], loop_override: true);

        }
    }
}