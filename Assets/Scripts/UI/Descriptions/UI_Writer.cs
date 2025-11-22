using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_Writer : MonoBehaviour
{
    [Header("Writing")]
    private int cursor = 0;
    private string current_raw_writing = "";
    private string target_writing = "";
    private List<int> pause_indexes = new List<int>();
    private List<int> dot_indexes = new List<int>();
    
    [Header("Speed parameters")]
    [SerializeField] private float caracters_per_sec = 50f;
    [SerializeField] private float slow_caractere_delay = 0.1f; // good for cps = 50F
    [SerializeField] private float pause_caractere_delay = 0.5f; // good for cps = 50F

    [Header("Components")]
    private TextMeshProUGUI _label;
    public TextMeshProUGUI label
    {
        get
        {
            if (_label == null) { _label = GetComponent<TextMeshProUGUI>(); }
            return _label;
        }
    }

    [Header("Logs")]
    public bool log_writing = false;

    // AWAKE
    private void Awake()
    {
        // on initialise le label
        label.text = "";
    }

    // ON ENABLE / DISABLE
    private void OnDisable()
    {
        if (writing_coroutine == null) { return; }
        StopCoroutine(writing_coroutine);
        writing_coroutine = null;
    }
    private void OnEnable()
    {
        if (cursor == 0) { return; } // we finished writing last time so we don't write again
        writing_coroutine = StartCoroutine(write());
    }

    // COLOR
    public void SetColor(Color color)
    {
        label.color = color;
    }

    // WRITING
    public void Write(string raw_writing)
    {
        // if disabled we don't start the coroutine
        if (!gameObject.activeInHierarchy) { return; }

        // checks if we need to write again (not necessary if this is the same description)
        // the good thing is the coroutine will continue executing so it's perfect
        if (raw_writing == current_raw_writing) { return; }

        // if we are still writing we stop it
        if (writing_coroutine != null)
        {
            StopCoroutine(writing_coroutine);
            writing_coroutine = null;
        }

        // we refine the description
        string writing = refine_writing(raw_writing, out List<int> pause_indexes, out List<int> dot_indexes);

        // starts writing the refined description
        if (log_writing) { Debug.Log($"(UI_Writer) Writing : raw '{raw_writing}'\nrefined '{writing}'"); }
        this.current_raw_writing = raw_writing;
        this.target_writing = writing;
        this.pause_indexes = pause_indexes;
        this.dot_indexes = dot_indexes;
        this.cursor = 0;
        label.text = "";
        writing_coroutine = StartCoroutine(write());
    }
    private Coroutine writing_coroutine = null;
    public IEnumerator write()
    {
        while (cursor < target_writing.Length)
        {
            // we get the number of characters we want to add this frame
            float chars_this_frame = calculate_chars_per_frame();

            // if the cpf is below 1 then we need to wait some time to match the cpf (and so we will write only 1 letter)
            if (chars_this_frame < 1f)
            {
                float wait_time = 1 / caracters_per_sec; // time to wait to write 1 character
                if (log_writing) { Debug.Log($"(UI_Writer) cpf below 1 ({chars_this_frame}), waiting {wait_time} seconds"); }
                yield return new WaitForSecondsRealtime(wait_time);
                chars_this_frame = 1f;
            }

            int next_cursor = Mathf.Min(cursor + Mathf.FloorToInt(chars_this_frame), target_writing.Length);

            // checks if there is a dot or a pause between cursor & cursor + chars_this_frame
            bool play_pause = false;
            bool play_dot = false;
            if (pause_indexes.Count > 0 && pause_indexes[0] < next_cursor)
            {
                // we found a pause
                next_cursor = pause_indexes[0];
                play_pause = true;
            }
            if (dot_indexes.Count > 0 && dot_indexes[0] < next_cursor)
            {
                // we found a dot and it is closer than any found pause !
                next_cursor = dot_indexes[0];
                play_dot = true;
                play_pause = false; // we don't want to play both
            }

            // we update the label
            label.text = target_writing.Substring(0, next_cursor);

            // we play pause / dot if needed
            if (play_dot) { yield return write_dot(); dot_indexes.RemoveAt(0); }
            else if (play_pause) { yield return write_pause(); pause_indexes.RemoveAt(0); }

            // we update the cursor
            cursor = next_cursor;

            // we wait for the next frame
            yield return null;
        }

        writing_coroutine = null;
        cursor = 0;
    }

    // PAUSE & DOT
    private IEnumerator write_pause()
    {
        float pause_delay = (pause_caractere_delay * 50f) / caracters_per_sec;

        // on fait une grosse pause
        if (log_writing) { Debug.Log($"(UI_Writer) pausing at cursor : {cursor}"); }
        yield return new WaitForSecondsRealtime(pause_delay);
    }
    private IEnumerator write_dot()
    {
        float dot_delay = (slow_caractere_delay * 50f) / caracters_per_sec;

        // on écrit les 3 points
        if (log_writing) { Debug.Log($"(UI_Writer) writing dot at cursor : {cursor}"); }
        yield return new WaitForSecondsRealtime(1f / caracters_per_sec);
        label.text += '.';
        yield return new WaitForSecondsRealtime(1f / caracters_per_sec);
        label.text += '.';
        string old_text = label.text;
        yield return new WaitForSecondsRealtime(dot_delay);
        label.text += '.';

        // on clignote un petit peu
        for (int arghfsdf = 0; arghfsdf < 2; arghfsdf++)
        {
            yield return new WaitForSecondsRealtime(dot_delay);
            label.text = old_text + ' ';
            yield return new WaitForSecondsRealtime(dot_delay);
            label.text = old_text + '.';
        }
    }

    // REFINING TEXT
    private string refine_writing(string raw_writing, out List<int> pause_indexes, out List<int> dot_indexes)
    {
        pause_indexes = new List<int>();
        dot_indexes = new List<int>();
        int refined_cursor = 0;

        // we go through all the description and try to find some special characters
        for (int i = 0; i < raw_writing.Length - 2; i++)
        {
            // checks for \n
            if (raw_writing[i] == '\\' && raw_writing[i + 1] == 'n')
            {
                refined_cursor--; // we remove 1 character bcz we will convert a 2 char string to a 1 char string
                i++;
                continue;
            }

            // checks for pauses
            if (raw_writing[i] == '/' && raw_writing[i + 1] == 'l')
            {
                pause_indexes.Add(refined_cursor);
                // refined_cursor-=2; // we remove 2 characters bcz we will convert a 2 char string to a 0 char string
                i++;
                continue;
            }
            
            // checks for dots
            if (raw_writing[i] == '/' && raw_writing[i + 1] == '.')
            {
                dot_indexes.Add(refined_cursor);
                refined_cursor+=3; // we add 1 character bcz we will convert a 2 char string to a 3 char string
                i++;
                continue;
            }
            refined_cursor++;
        }

        return raw_writing.Replace("/.", "...").Replace("/l", string.Empty).Replace("\\n", "\n");
    }
    private float calculate_chars_per_frame()
    {
        if (log_writing) { Debug.Log($"(UI_Writer) calculating chars per frame : {caracters_per_sec * Time.unscaledDeltaTime} cpf"); }
        return caracters_per_sec * Time.unscaledDeltaTime;
    }
}