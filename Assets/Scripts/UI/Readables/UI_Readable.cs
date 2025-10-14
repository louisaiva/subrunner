using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Readable : MonoBehaviour
{
    [Header("Transition parameters")]
    public PoolTransitionSettings transition_settings = PoolTransitionSettings.InMenuDefault;

    [Header("Content showing parameters")]
    public Transitioner content = null;
    [Range(0, 1), SerializeField] private float transition_duration_percentage = 0.5f;
    // if 0.75f the content will start showing after 25% of the Show() duration. and will finish showing at 100%
    // on the Hide() it is the opposite it will start hiding at 0% then be fully hidden at 75%
    Coroutine content_routine = null;

    [Header("Animations parameters")]
    public UI_AnimPlayer player;
    public List<string> show_animations = new List<string>(); // will play these animations on show
    public List<string> loop_animations = new List<string>(); // will play these animations in loop (random one)
    public List<string> hide_animations = new List<string>(); // will play these animations on hide
    Coroutine anim_routine = null;

    // SHOW HIDE 
    public void Show(float duration = 0f)
    {
        if (anim_routine != null) { StopCoroutine(anim_routine); }
        anim_routine = StartCoroutine(show_anims(duration));

        if (content_routine != null) { StopCoroutine(content_routine); }
        content_routine = StartCoroutine(transition_content(duration, show: true));
        GetComponent<Transitioner>()?.Show(duration * (1-transition_duration_percentage)); // we show the whole content for a short time (while playing the anim omg)
    }
    public void Hide(float duration = 0f)
    {
        if (anim_routine != null) { StopCoroutine(anim_routine); }
        anim_routine = StartCoroutine(hide_anims(duration));

        if (content_routine != null) { StopCoroutine(content_routine); }
        content_routine = StartCoroutine(transition_content(duration, show: false));
    }


    // SHOW HIDE LOW LEVEL + CONTENT
    private IEnumerator show_anims(float duration = 0f)
    {
        foreach (string anim in show_animations)
        {
            if (player == null) { yield break; }
            float anim_duration = calculate_anim_duration(anim, animations: show_animations, total_duration: duration);

            // we play the animation
            player.Play(anim, duration_override: anim_duration, loop_override: false);

            // wait until the animation is over
            while (player.IsPlaying) { yield return null; }
        }

        // if we have no loop animations we stop here
        if (loop_animations.Count == 0) { anim_routine = null; yield break; }

        // we loop the loop animations
        while (true)
        {
            // we pick a random animation
            string anim = loop_animations[Random.Range(0, loop_animations.Count)];
            if (player == null) { yield break; }

            // we play the animation
            player.Play(anim, loop_override: false);

            // wait until the animation is over
            while (player.IsPlaying) { yield return null; }
        }
    }
    private IEnumerator hide_anims(float duration = 0f)
    {
        foreach (string anim in hide_animations)
        {
            if (player == null) { yield break; }
            float anim_duration = calculate_anim_duration(anim, animations: hide_animations, total_duration: duration);

            // we play the animation
            player.Play(anim, duration_override: anim_duration, loop_override: false);

            // wait until the animation is over
            while (player.IsPlaying) { yield return null; }
        }
    }
    private IEnumerator transition_content(float duration = 0f, bool show = true)
    {
        float transition_duration = duration * transition_duration_percentage;
        float time_to_wait = show ? duration - transition_duration : 0f;

        if (time_to_wait > 0f) { yield return new WaitForSecondsRealtime(time_to_wait); }

        if (show) { yield return content.Show(transition_duration); }
        else
        {
            yield return content.Hide(transition_duration);
            GetComponent<Transitioner>()?.Hide(duration - transition_duration);

        }

        content_routine = null;
    }



    // DURATION CALCULATION
    private float calculate_anim_duration(string anim, List<string> animations, float total_duration)
    {
        if (player == null) { return 0f; }
        if (total_duration <= 0f) { return 0f; }
        if (animations.Count == 0) { return 0f; }

        // we get the total duration of the animations
        float total_base_duration = 0f;
        for (int i = 0; i < animations.Count; i++)
        {
            Anim a = player.Bank.GetAnim(player.skin + "." + animations[i] + "." + player.orientation);
            if (a != null)
            {
                total_base_duration += a.GetBaseDuration();
            }
        }

        // we get the base duration of the current animation
        Anim anim_obj = player.Bank.GetAnim(player.skin + "." + anim + "." + player.orientation);
        if (anim_obj == null) { return 0f; }
        float anim_base_duration = anim_obj.GetBaseDuration();

        // we calculate the duration of the animation
        float anim_duration = (anim_base_duration / total_base_duration) * total_duration;

        return anim_duration;
    }

}