#pragma warning disable 4014
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading screen")]
    [SerializeField] private bool linux_style_loading = false;
    // [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image bg;
    private PauseMenuBackgroundEffect fx;
    [SerializeField] private GameObject text_prefab;
    [SerializeField] private GameObject text_parents;

    [Header("Transitions")]
    [SerializeField] private float transition_duration = 0.2f;
    [SerializeField] private UI_PoolSettings Transition;

    [Header("Texts")]
    [SerializeField] private Vector2 text_delay_range = new Vector2(0.1f, 0.5f);
    [SerializeField] private List<string> texts;

    [Header("Components")]
    private CharacterOrientationController charac_orienter = null;
    public CharacterOrientationController CharacOrienter
    {
        get
        {
            if (charac_orienter == null)
            {
                charac_orienter = GameObject.Find("ui_chroma/charac/character").GetComponent< CharacterOrientationController>();
                if (charac_orienter == null) { Debug.LogError("(SceneLoader) could not find CharacterOrientationController at /ui_chroma/charac/character!"); }
            }
            return charac_orienter;
        }
    }

    private PostProcessManager ppm => PostProcessManager.Instance;

    [Header("Logs")]
    [SerializeField] private bool log;

    // AWAKE
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        fx = bg.GetComponent<PauseMenuBackgroundEffect>();
    }

    // LOAD GAME
    public void LoadGame()
    {
        // we start the coroutine to load the game
        // StopAllCoroutines(); // we stop all coroutines to avoid multiple calls
        /* StartCoroutine(linux_style_loading
            ? load_game_linux_style()
            : load_game_simplest()); */

        if (!linux_style_loading) { load_game_simplest(); return; }
        StartCoroutine(load_game_linux_style());
    }

    // LOAD GAME SIMPLEST
    private async Awaitable load_game_simplest()
    {
        if (log) { Debug.Log("LOADING THE GAME"); }

        // we pause the game
        Time.timeScale = 0f;

        // we show the loading screen
        await Tween.Custom(0f, 1f, duration: transition_duration, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));

        // load loading scene
        await SceneManager.LoadSceneAsync(2);

        // load the main clean scene    
        await SceneManager.LoadSceneAsync(1);

        // we hide the loading screen
        await Tween.Custom(1f, 0f, duration: transition_duration, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));

        if (log) { Debug.Log("GAME LOADED"); }
        AppManager.Instance.LoadedSceneCount++;
    }

    // LOAD GAME LINUX STYLE
    private IEnumerator load_game_linux_style()
    {
        if (log) { Debug.Log("LOADING THE GAME - LINUX STYLE"); }

        // we get the title
        UI_Title title = FindFirstObjectByType<UI_Title>();
        if (title != null) { title.Run(); }

        // we show the loading screen
        ppm.TransitionChroma(Transition.ChromaticAberration, transition_duration);
        ppm.TransitionBloom(Transition.Bloom, transition_duration);
        yield return new WaitForSecondsRealtime(transition_duration - 0.1f);
        fx.TransitionAlpha(true, 0.1f, override_final_alpha: Transition.BackgroundAlpha);
        yield return new WaitForSecondsRealtime(0.1f);

        // load loading scene
        AsyncOperation loading_scene = SceneManager.LoadSceneAsync(2);
        while (!loading_scene.isDone) { yield return null; }

        // we pause the game
        Time.timeScale = 0f;
        float timer = Time.realtimeSinceStartup;
        ppm.SetToneMapping(aces:false);

        // load the main clean scene
        AsyncOperation loading_game = SceneManager.LoadSceneAsync(1);

        // we show the very first text
        GameObject text = Instantiate(text_prefab, text_parents.transform);
        text.GetComponent<TextMeshProUGUI>().text = "loading subrunner_alpha_" + Application.version;

        // show the transitionner on the linux texts
        text_parents.GetComponent<Transitioner>().Show();

        // starts showing texts
        for (int i = 0; i < texts.Count; i++)
        {
            // we add a text
            text = Instantiate(text_prefab, text_parents.transform);
            text.GetComponent<TextMeshProUGUI>().text = texts[i];

            // we wait for a long time if we have no chance
            if (Random.Range(0f, 1f) < 0.1f)
            {
                // we wait for a long time
                yield return new WaitForSecondsRealtime(.5f);
            }

            // we wait for a little time
            float delay = Random.Range(text_delay_range.x, text_delay_range.y);
            yield return new WaitForSecondsRealtime(delay);
        }

        // verify that the game has finish loading
        if (!loading_game.isDone) { yield return wait_for_loading_to_finish(loading_game); }

        // we show the final "game loaded in x seconds text"
        text = Instantiate(text_prefab, text_parents.transform);
        text.GetComponent<TextMeshProUGUI>().text = "game loaded in " + (Time.realtimeSinceStartup - timer).ToString("F2") + " seconds";

        // we fade the texts away
        fade_texts_away(transition_duration);

        // we hide the transitionner on the linux texts
        text_parents.GetComponent<Transitioner>().Hide(transition_duration);

        // we show the ui_manager hud pool
        UI_Pool hud = UI_Manager.Instance.GetPool("hud");
        float old_transition_duration = hud.Settings.Duration;
        hud.Settings.Duration = transition_duration;
        UI_Manager.Instance.SwitchTo("hud");
        yield return null; // wait one frame to be sure that the manager took the home_appearance_duration value
        hud.Settings.Duration = old_transition_duration;

        // we hide the bg
        fx.TransitionAlpha(false, 0f);
        yield return new WaitForSecondsRealtime(transition_duration);

        if (log) { Debug.Log("GAME LOADED - LINUX STYLE"); }
        AppManager.Instance.LoadedSceneCount++;
    }
    private IEnumerator wait_for_loading_to_finish(AsyncOperation loading_game)
    {
        TextMeshProUGUI last_text = text_parents.transform.GetChild(text_parents.transform.childCount - 1).GetComponent<TextMeshProUGUI>();
        string base_text = last_text.text;

        // we wait until the scene is loaded
        while (!loading_game.isDone)
        {
            yield return new WaitForSecondsRealtime(text_delay_range.y);

            // we update the last text to show ...
            last_text.text += ".";
            if (last_text.text.Length > base_text.Length + 3)
            {
                // we reset the text
                last_text.text = base_text;
            }
        }
    }
    private void fade_texts_away(float duration)
    {
        // we fade the texts away
        for (int i = 0; i < text_parents.transform.childCount; i++)
        {
            TextMeshProUGUI text = text_parents.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
            text.CrossFadeAlpha(0f, duration, false);
        }
    }


    // GO BACK TO MAIN MENU
    public async Awaitable GoBackToMainMenu()
    {
        // we pause the game
        Time.timeScale = 0f;

        // we show the loading screen
        ppm.TransitionChroma(Transition.ChromaticAberration, transition_duration);
        ppm.TransitionBloom(Transition.Bloom, transition_duration);
        TransitionTimeScale(0f, transition_duration);
        await System.Threading.Tasks.Task.Delay((int)((transition_duration-0.1f) * 1000));
        await fx.TransitionAlpha(true, 0.1f, override_final_alpha: Transition.BackgroundAlpha);
        
        // load loading scene
        await SceneManager.LoadSceneAsync(2);
        ppm.SetToneMapping(aces:true);

        // we load the main menu scene
        await SceneManager.LoadSceneAsync(0);

        // we show the ui_manager home
        UI_Pool home = UI_Manager.Instance.GetPool("home");
        home.PreparePool();
        float old_transition_duration = home.Settings.Duration;
        home.Settings.Duration = transition_duration;
        UI_Manager.Instance.SwitchTo("home");
        await System.Threading.Tasks.Task.Yield(); // wait one frame to be sure that the manager took the home_appearance_duration value
        home.Settings.Duration = old_transition_duration;

        // we hide the bg
        fx.TransitionAlpha(false, 0f);

        if (log) { Debug.Log("BACK TO MAIN MENU"); }
        AppManager.Instance.LoadedSceneCount++;
    }

    // HELPER METHODS
    private async Awaitable TransitionTimeScale(float target, float duration)
    {
        if (Mathf.Approximately(Time.timeScale, target)) { return; }
        if (duration <= 0f)
        {
            Time.timeScale = target;
            return;
        }
        await Tween.GlobalTimeScale(target, duration, Ease.OutQuad);
    }

}