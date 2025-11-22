using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Description : MonoBehaviour
{
    [Header("Description")]
    [SerializeField] private string target_description = "description";
    [SerializeField] private float caracters_per_sec = 200f;
    [SerializeField] private float slow_caractere_delay = 0.1f;
    [SerializeField] private float pause_caractere_delay = 0.5f;
    private int cursor = 0;

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
        // on récupère le label
        // label = GetComponent<TextMeshProUGUI>();
        // if (label == null) { Debug.LogError("(Description) missing label on " + name); }

        // on initialise le label
        label.text = target_description;
    }

    // ON ENABLE
    private void OnEnable()
    {
        if (cursor == 0) { return; } // we finished writing last time so we don't write again
        StartCoroutine(write(target_description, cursor));
    }

    // COLOR
    public void SetColor(Color color)
    {
        label.color = color;
    }

    // WRITING
    public void SetDescription(string description)
    {
        if (description == target_description) { return; }

        // on remet à zéro le label
        StopAllCoroutines();
        label.text = string.Empty;

        // check if description is active
        if (gameObject.activeSelf == false) { return; }
        if (description == null || description == "") { return; }

        // set the description of the item
        target_description = description;
        StartCoroutine(write(description));
    }
    IEnumerator write(string target, int cursor = 0)
    {
        if (log_writing) { Debug.Log($"(Description) writing {target}"); }

        this.cursor = cursor;
        // on ajoute les caractères un par un
        for (int j = cursor; j < target.Length; j++)
        {
            if (log_writing) { Debug.Log($"(Description) writing char {target[j]} at pos {j}"); }

            if (target[j] == '/' && j < target.Length - 1 && target[j + 1] == '.')
            {
                this.cursor = j + 2; // we set the cursor before waiting so we ensure that we won't write this caracter till the infinite
                yield return new WaitForSecondsRealtime(1f / caracters_per_sec);
                label.text += '.';
                yield return new WaitForSecondsRealtime(1f / caracters_per_sec);
                label.text += '.';
                string old_text = label.text;
                yield return new WaitForSecondsRealtime(slow_caractere_delay);
                label.text += '.';

                // on clignote un petit peu
                for (int arghfsdf = 0; arghfsdf < 2; arghfsdf++)
                {
                    yield return new WaitForSecondsRealtime(slow_caractere_delay);
                    label.text = old_text + ' ';
                    yield return new WaitForSecondsRealtime(slow_caractere_delay);
                    label.text = old_text + '.';
                }
                label.text += ' ';

                // on saute le caractère suivant
                j++;

                continue;
            }
            else if (target[j] == '/' && j < target.Length - 1 && target[j + 1] == 'l')
            {
                this.cursor = j + 2; // we set the cursor before waiting so we ensure that we won't write this caracter till the infinite
                // on fait une grosse pause
                yield return new WaitForSecondsRealtime(pause_caractere_delay);

                // on saute le caractère suivant
                j++;

                continue;
            }

            yield return new WaitForSecondsRealtime(1f / caracters_per_sec);
            this.cursor = j + 1;
            label.text += target[j];
        }

        // on reset le cursor
        this.cursor = 0;
    }
}