using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Animation : MonoBehaviour
{
    public UI_AnimPlayer player;
    public List<string> animations_to_play = new List<string>();
    public float delay_between_animations = 0.5f;
    public bool loop = true;

    private void OnEnable()
    {
        player = GetComponent<UI_AnimPlayer>();
        if (player == null) { Debug.LogWarning($"(UI_Animation) {name} has no UI_AnimPlayer component."); return; }

        StartCoroutine(PlayAnimations());
    }

    private IEnumerator PlayAnimations()
    {
        while (true)
        {
            yield return loop_animations();
            if (!loop) { break; }
        }
    }
    private IEnumerator loop_animations()
    {
        foreach (string anim in animations_to_play)
        {
            if (player == null) { yield break; }

            // checks if we need to make it loop : only if last of animations_to_play
            bool loop = false;
            if (animations_to_play.IndexOf(anim) == animations_to_play.Count - 1) { loop = true; }

            // we play the animation
            player.Play(anim, loop_override: loop);

            // wait until the animation is over
            while (player.IsPlaying) { yield return null; }
            yield return new WaitForSeconds(delay_between_animations);
        }
    }
}