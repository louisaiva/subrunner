using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameLoader : MonoBehaviour {

    [SerializeField] private GameObject loading_ui;
    [SerializeField] private TextMeshProUGUI loading_text;


    // PROGRESS BAR
    [SerializeField] private RectTransform loading_bar;
    private float bar_max_width = 512f;
    private float progress = 0f;
    private int generation_steps = 0;
    private float generation_max_steps = 26f;


    public void BeginProgress()
    {
        Debug.Log("begin progress");
        // on affiche le loading ui
        loading_ui.SetActive(true);
        progress = 0f;
        generation_steps = 0;
    }

    public void AddProgress(string text)
    {
        generation_steps++;
        progress = generation_steps / generation_max_steps;

        // on met à jour la barre de progression
        loading_bar.sizeDelta = new Vector2(progress * bar_max_width, loading_bar.sizeDelta.y);
        // Debug.Log("progress : " + progress + " / " + generation_steps + " / " + generation_max_steps + " / " + progress * bar_max_width);

        // on met à jour le texte
        loading_text.text = text + "...";
    }

    public void AddText(string text)
    {
        // on met à jour le texte
        loading_text.text = text + "...";
    }

    public void EndProgress()
    {
        // we can now hide the loading bar
        loading_ui.SetActive(false);
    }

}
