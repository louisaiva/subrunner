using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_Writer : MonoBehaviour
{
    [Header("Auto writing parameters")]
    [SerializeField] private bool write_on_enable = false;
    [SerializeField] private string raw_writing_on_enable = "";
    [SerializeField] private float on_enable_delay = 0f;

    [Header("Writing")]
    private int cursor = 0;
    private string current_raw_writing = "";
    private string target_writing = "";
    private List<int> pause_indexes = new List<int>();
    private List<int> dot_indexes = new List<int>();
    private List<TextColorIndex> color_indexes = new List<TextColorIndex>();
    
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
    public bool log_color_tags = false;

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
        if (write_on_enable && !string.IsNullOrEmpty(raw_writing_on_enable))
        {
            // we empty the label
            label.text = "";

            if (on_enable_delay <= 0) { Write(raw_writing_on_enable); }
            else { StartCoroutine(write_on_enable_delay()); }
        }
        else if (cursor == 0) { return; } // we finished writing last time so we don't write again
        writing_coroutine = StartCoroutine(write());
    }
    private IEnumerator write_on_enable_delay()
    {
        yield return new WaitForSecondsRealtime(on_enable_delay);
        Write(raw_writing_on_enable);
    }

    // COLOR
    public void SetColor(Color color)
    {
        label.color = color;
    }

    // WRITING
    private Coroutine writing_coroutine = null;
    public bool IsWriting => writing_coroutine != null;
    public void Write(string raw_writing)
    {
        // if disabled we don't start the coroutine
        if (!gameObject.activeInHierarchy) { return; }
        if (raw_writing == null) { raw_writing = ""; }

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
        string writing = refine_writing(raw_writing, out List<int> pause_indexes, out List<int> dot_indexes, out List<TextColorIndex> color_indexes);

        // starts writing the refined description
        if (log_writing) { Debug.Log($"(UI_Writer) Writing : raw '{raw_writing}'\nrefined '{writing}'"); }
        this.current_raw_writing = raw_writing;
        this.target_writing = writing;
        this.pause_indexes = pause_indexes;
        this.dot_indexes = dot_indexes;
        this.color_indexes = color_indexes;
        this.cursor = 0;
        label.text = "";
        writing_coroutine = StartCoroutine(write());
    }

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

            // we get the final text to write
            // then we add the color tags and finally
            // we update the label
            label.text = apply_color_tags(target_writing.Substring(0, next_cursor), next_cursor);

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

    // COLORS
    private string apply_color_tags(string text, int next_cursor)
    {
        // then we add the color tags if needed
        int offset = 0;
        foreach (TextColorIndex ci in color_indexes)
        {
            if (ci.start_index >= next_cursor) { break; } // we don't need to add any more color tags

            // we add the start tag at the start index
            text = text.Insert(ci.start_index + offset, ci.GetStartTag());
            offset += ci.StartTagLength;

            // we check if we can add the end tag (if the end index is below the next cursor)
            if (ci.end_index >= next_cursor) { break; } // we don't need to add any more end tags
            text = text.Insert(ci.end_index + offset, ci.GetEndTag());
            offset += ci.EndTagLength;
        }
        return text;
    }

    // REFINING TEXT
    // todo : this method has an issue, when gathering colors we remove some characters,
    // todo : which offsets the indexes, and so will offset the /l & /. special characters,
    // todo : which will screw up the pauses and dots positions
    private string refine_writing(string raw_writing, out List<int> pause_indexes, out List<int> dot_indexes, out List<TextColorIndex> color_indexes)
    {
        string refined_writing = "";
        pause_indexes = new List<int>();
        dot_indexes = new List<int>();
        color_indexes = new List<TextColorIndex>();
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

        refined_writing = raw_writing.Replace("/.", "...").Replace("/l", string.Empty).Replace("\\n", "\n");

        if (log_color_tags) { Debug.Log($"(UI_Writer) refined writing before colors : '{refined_writing}'\n"); }

        // we find all the color tags and store their indexes
        int search_index = 0;
        int cumulated_offset = 0; // bcz we will remove the color tags, each color tag index needs to be offsetted by the previous number of characters removed
        while (search_index < refined_writing.Length)
        {
            int start_tag_index = refined_writing.IndexOf("<color=", search_index);
            if (start_tag_index == -1) { break; }

            // we have found a color tag ! we first gather the color code
            string color_code = "";
            int tmp_index;
            for (tmp_index = start_tag_index + 7; tmp_index < refined_writing.Length; tmp_index++)
            {
                if (refined_writing[tmp_index] == '>') { break; }
                color_code += refined_writing[tmp_index];
            }
            int start_tag_length = tmp_index + 1 - start_tag_index; // we add 1 to include the '>' character

            // now we find the end tag
            int end_tag_index = refined_writing.IndexOf("</color>", tmp_index);
            if (end_tag_index == -1) { break; }

            // we have everything ! we can store the color index and continue searching
            color_indexes.Add(new TextColorIndex(start_tag_index - cumulated_offset, end_tag_index - cumulated_offset - start_tag_length, color_code));
            if (log_color_tags) { Debug.Log($"(UI_Writer) found color tag : {color_code} at {start_tag_index} to {end_tag_index} (length {start_tag_length}), cumulated offset is {cumulated_offset}"); }
            search_index = end_tag_index + 8; // we add 8 to skip the "</color>" tag
            cumulated_offset += start_tag_length + 8;
        }

        // now we remove all the color tags from the refined writing
        foreach (TextColorIndex color_index in color_indexes)
        {
            refined_writing = refined_writing.Remove(color_index.start_index, color_index.GetStartTag().Length);
            refined_writing = refined_writing.Remove(color_index.end_index, color_index.GetEndTag().Length);
        }

        if (log_color_tags) { Debug.Log($"(UI_Writer) final refined writing : '{refined_writing}'\n"); }

        return refined_writing;
    }
    private float calculate_chars_per_frame()
    {
        if (log_writing) { Debug.Log($"(UI_Writer) calculating chars per frame : {caracters_per_sec * Time.unscaledDeltaTime} cpf"); }
        return caracters_per_sec * Time.unscaledDeltaTime;
    }

    // CLEAR
    public void Clear()
    {
        if (writing_coroutine != null)
        {
            StopCoroutine(writing_coroutine);
            writing_coroutine = null;
        }
        current_raw_writing = "";
        target_writing = "";
        pause_indexes = new List<int>();
        dot_indexes = new List<int>();
        cursor = 0;
        label.text = "";
    }

}

public class TextColorIndex
{
    public int start_index;
    public int end_index;
    public string color_code;

    public TextColorIndex(int start, int end, string color_code)
    {
        this.start_index = start;
        this.end_index = end;
        this.color_code = color_code;
    }

    public string GetStartTag() => $"<color={color_code}>";
    public int StartTagLength => 17; // "<color=#FF0000>".Length
    public string GetEndTag() => "</color>";
    public int EndTagLength => 8; // "</color>".Length
}