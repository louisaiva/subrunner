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
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image bg;
    [SerializeField] private GameObject text_prefab;
    [SerializeField] private GameObject text_parents;

    [Header("Transitions")]
    [SerializeField] private float transition_duration = 0.2f;

    [Header("Texts")]
    [SerializeField] private Vector2 text_delay_range = new Vector2(0.1f, 0.5f);
    [SerializeField] private List<string> texts;

    [Header("Title Screen Elements")]
    [SerializeField] private JoystickFeedback joystickFeedback;
    public HomeInputsController HIC;
    public CharacterOrientationController CharacOrienter;


    [Header("Logs")]
    [SerializeField] private bool log;

    // AWAKE
    private void Awake()
    {
        // if (Instance != null) { Instance.FinishTransitionAndDestroy(); return; }
        Instance = this;
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

        // we disable the input action
        HIC.RemoveCallbacks();

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
    }

    // LOAD GAME LINUX STYLE
    private IEnumerator load_game_linux_style()
    {
        if (log) { Debug.Log("LOADING THE GAME - LINUX STYLE"); }

        // we disable the input action
        HIC.RemoveCallbacks();

        // we wait one frame
        yield return null;

        // we show the loading screen
        Tween show_bg = Tween.Custom(0f, 1f, duration: transition_duration, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));

        bg.GetComponent<PauseMenuBackgroundEffect>().TransitionEffect(true,transition_duration);
        while (show_bg.isAlive) { yield return null; }

        // load loading scene
        AsyncOperation loading_scene = SceneManager.LoadSceneAsync(2);
        while (!loading_scene.isDone) { yield return null; }

        // we pause the game
        Time.timeScale = 0f;
        float timer = Time.realtimeSinceStartup;

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

        // we wait a little time
        yield return new WaitForSecondsRealtime(0.2f);


        // we fade the texts away
        float fade_duration = 2f;
        fade_texts_away(fade_duration);

        // we hide the transitionner on the linux texts
        text_parents.GetComponent<Transitioner>().Hide(fade_duration);

        // we wait for the fade to finish
        yield return new WaitForSecondsRealtime(fade_duration/2f);

        // we hide the bg
        Time.timeScale = 1f; // we resume the game

        // we hide the bg
        bg.GetComponent<PauseMenuBackgroundEffect>().TransitionEffect(false, fade_duration);
        Tween hide_bg = Tween.Custom(1f, 0f, duration: fade_duration, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));
        while (hide_bg.isAlive) { yield return null; }

        if (log) { Debug.Log("GAME LOADED - LINUX STYLE"); }
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
        bg.GetComponent<PauseMenuBackgroundEffect>().TransitionEffect(true, transition_duration);
        await Tween.Custom(0f, 1f, duration: transition_duration, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));

        // load loading scene
        await SceneManager.LoadSceneAsync(2);

        // we load the main menu scene
        await SceneManager.LoadSceneAsync(0);

        // we hide the loading screen
        UI_Manager.Instance.TransitionBackground(0f, transition_duration/2f);
        bg.GetComponent<PauseMenuBackgroundEffect>().TransitionEffect(false, transition_duration);
        await Tween.Delay(transition_duration/2f); // we wait for half duration to avoid a sudden cut
        await Tween.Custom(1f, 0f, duration: transition_duration / 2f, useUnscaledTime: true,
            onValueChange: ctx => bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, ctx));

        // we destroy this object
        Destroy(this.gameObject);
    }
}