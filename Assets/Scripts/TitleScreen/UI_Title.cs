using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


/// <summary>
/// UI_Title is the script for handling the title of the game
/// in the title screen.
/// it is the first shown element of the program, shows the
/// appear animation with the help of UI_AnimPlayer, then
/// is responsible for switching UI_Manager to home (+ adds itself to home)
/// </summary>
/// 

public class UI_Title : MonoBehaviour
{
    [Header("Appearance Parameters")]
    public float delay_before_appearing = 0.5f;
    [SerializeField] private float home_appearance_duration = 1f;

    [Header("Loop parameters")]
    [SerializeField] private Vector2 delay_between_glitches = new Vector2(5f, 15f);
    [SerializeField] private List<string> glitch_animations = new List<string>() { "glitch1" };

    [Header("Components")]
    private UI_AnimPlayer player;
    private Coroutine current_coroutine = null;
    private Image image;

    [Header("Logs")]
    public bool log = false;

    // START
    private void Awake()
    {
        player = GetComponent<UI_AnimPlayer>();
        image = GetComponent<Image>();

        if (AppManager.Instance == null || AppManager.Instance.LoadedSceneCount == 0)
        {
            current_coroutine = StartCoroutine(appear_coroutine());
            return;
        }
    }

    // ON ENABLE
    void OnEnable()
    {
        if (current_coroutine != null) { return; }
        // we relaunch the loop coroutine
        current_coroutine = StartCoroutine(loop_coroutine(run_instantly: true));
    }

    // APPEARING
    private IEnumerator appear_coroutine()
    {
        if (log) { Debug.Log($"(UI_Title) Starting appear coroutine."); }

        // we prepare home
        UI_Pool home = UI_Manager.Instance.GetPool("home");
        float old_transition_duration = home.Settings.Duration;
        home.PreparePool();

        // we hide the image first
        image.enabled = false;
        if (delay_before_appearing > 0f) { yield return new WaitForSecondsRealtime(delay_before_appearing); }
        image.enabled = true;

        // play appear animation
        player.Play("appear", loop_override: false);
        float appear_duration = player.current_anim.GetDuration();

        // we wait appear_duration - home_appearance_duration to arrive on home
        // just when the appear anim is done
        yield return new WaitForSecondsRealtime(appear_duration - home_appearance_duration);
        if (log) { Debug.Log($"(UI_Title) Registering to home + showing home."); }

        // we show the ui_manager home
        home.Settings.Duration = home_appearance_duration;
        UI_Manager.Instance.SwitchTo("home");
        yield return null; // wait one frame to be sure that the manager took the home_appearance_duration value
        home.Settings.Duration = old_transition_duration;
        while (player.IsPlaying) { yield return null; }

        // once appear is done, we auto switch the charac switcher (si it won't only play the cat)
        CharacterSwitcher switcher = SceneLoader.Instance.CharacOrienter.GetComponent<CharacterSwitcher>();
        switcher.AutoSwitchFromNow();

        // we launch the loop coroutine
        yield return loop_coroutine();
    }

    // LOOPING
    private IEnumerator loop_coroutine(bool run_instantly = false)
    {
        if (run_instantly) { yield return run_coroutine(1); }

        if (log) { Debug.Log($"(UI_Title) Starting loop coroutine."); }
        while (true)
        {
            // we choose a random duration between 
            float delay = Random.Range(delay_between_glitches.x, delay_between_glitches.y);
            if (log) { Debug.Log($"(UI_Title) playing idle for {delay} seconds."); }

            // we play idle animation
            player.Play("idle", loop_override: false, duration_override: delay);
            while (player.IsPlaying) { yield return null; }

            // we choose a random glitch animation
            string glitch_anim = glitch_animations[Random.Range(0, glitch_animations.Count)];
            if (log) { Debug.Log($"(UI_Title) playing glitch animation {glitch_anim}"); }
            player.Play(glitch_anim, loop_override: false);
            while (player.IsPlaying) { yield return null; }

            // once on 150 we run for 1 to 5 times
            if (Random.Range(0, 150) == 0)
            {
                int run_times = Random.Range(1, 3);
                yield return run_coroutine(run_times);
            }
        }
    }

    // RUN
    private IEnumerator run_coroutine(int times = 1,string run_anim = "run")
    {
        if (log) { Debug.Log($"(UI_Title) Starting run coroutine for {times} times."); }
        for (int i = 0; i < times; i++)
        {
            // play run animation
            player.Play(run_anim, loop_override: false);
            while (player.IsPlaying) { yield return null; }
        }
    }
    public void Run()
    {
        if (current_coroutine != null) { StopCoroutine(current_coroutine); }
        current_coroutine = StartCoroutine(run_coroutine(5, "rerun"));
    }
}