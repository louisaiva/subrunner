using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{

    [Header("Loading screen")]
    [SerializeField] private bool linux_style_loading = false;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject bg;
    [SerializeField] private GameObject text_prefab;
    [SerializeField] private GameObject text_parents;

    [Header("Texts")]
    [SerializeField] private Vector2 text_delay_range = new Vector2(0.1f, 0.5f);
    [SerializeField] private List<string> texts;

    [Header("Title Screen Elements")]
    [SerializeField] private JoystickFeedback joystickFeedback;
    [SerializeField] private CharacterOrientationController controller;
    // private AnimPlayer animPlayer;

    // START
    private void Start()
    {
        // we hide the loading screen
        loadingScreen.SetActive(false);
    }


    // LOAD GAME
    public void LoadGame()
    {
        // we start the coroutine to load the game
        StopAllCoroutines(); // we stop all coroutines to avoid multiple calls
        StartCoroutine( linux_style_loading
            ? load_game_linux_style()
            : load_game_simplest());
    }

    // LOAD GAME SIMPLEST
    private IEnumerator load_game_simplest()
    {
        Debug.Log("LOADING THE GAME");

        // we disable the input action
        controller.RemoveCallbacks();
        Destroy(controller.gameObject);

        // we disable the input feedback
        Destroy(joystickFeedback.gameObject);

        // we show the loading screen
        loadingScreen.SetActive(true);

        // we wait one frame
        yield return null;

        // we pause the game
        Time.timeScale = 0f;

        // load the main clean scene
        AsyncOperation loading_game = SceneManager.LoadSceneAsync(1);
        while (!loading_game.isDone)
        {
            // we wait for the loading to finish
            yield return null;
        }

        // we hide the loading screen
        loadingScreen.SetActive(false);
    }

    // LOAD GAME LINUX STYLE
    private IEnumerator load_game_linux_style()
    {
        Debug.Log("LOADING THE GAME");

        // we disable the input action
        controller.RemoveCallbacks();
        Destroy(controller.gameObject);

        // we disable the input feedback
        Destroy(joystickFeedback.gameObject);

        // we wait one frame
        yield return null;

        // we show the loading screen
        loadingScreen.SetActive(true);

        // we pause the game
        Time.timeScale = 0f;
        float timer = Time.realtimeSinceStartup;

        // load the main clean scene
        AsyncOperation loading_game = SceneManager.LoadSceneAsync(1);

        // we show the very first text
        GameObject text = Instantiate(text_prefab, text_parents.transform);
        text.GetComponent<TextMeshProUGUI>().text = "loading subrunner_alpha_" + Application.version;

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

        // we hide the bg
        Time.timeScale = 1f; // we resume the game
        bg.SetActive(false);

        // we fade the texts away
        float fade_duration = 2f;
        fade_texts_away(fade_duration);
        yield return new WaitForSecondsRealtime(fade_duration);

        // we finally hide all loading things
        loadingScreen.SetActive(false);
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
}