using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Description : MonoBehaviour
{
    [Header("Description")]
    [SerializeField] private string target_description = "description";
    [SerializeField] private float caractere_delay = 0.05f;
    [SerializeField] private float slow_caractere_delay = 0.5f;
    [SerializeField] private float pause_caractere_delay = 0.5f;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        // on récupère le label
        label = GetComponent<TextMeshProUGUI>();

        // on initialise le label
        label.text = target_description;
    }

    public void SetDescription(string description)
    {
        // on remet à zéro le label
        StopAllCoroutines();
        label.text = string.Empty;

        // check if description is active
        if (gameObject.activeSelf == false) {return;}

        // set the description of the item
        target_description = description;
        StartCoroutine(write(description));
    }

    IEnumerator write(string target)
    {
        // on ajoute les caractères un par un
        for (int j = 0; j < target.Length; j++)
        {
            if (target[j] == '/' && j < target.Length - 1 && target[j + 1] == '.')
            {
                yield return new WaitForSecondsRealtime(caractere_delay);
                label.text += '.';
                yield return new WaitForSecondsRealtime(caractere_delay);
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
                // on fait une grosse pause
                yield return new WaitForSecondsRealtime(pause_caractere_delay);

                // on saute le caractère suivant
                j++;

                continue;
            }

            yield return new WaitForSecondsRealtime(caractere_delay);
            label.text += target[j];
        }
    }
}