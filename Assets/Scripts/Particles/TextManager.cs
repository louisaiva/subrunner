using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TextManager : MonoBehaviour
{

    // permet de créer des textes flottants de différents types

    private GameObject floating_text_prefab;
    private GameObject static_text_prefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);

    [Header("Talking")]
    [SerializeField] private float line_offset = 0.2f;
    [SerializeField] private float line_delay = 1f;
    [SerializeField] private float caractere_delay = 0.05f;
    [SerializeField] private float slow_caractere_delay = 0.5f;
    [SerializeField] private float ttl_sentence = 5f;

    // unity functions
    void Start()
    {
        floating_text_prefab = Resources.Load("prefabs/ui/floating_text") as GameObject;
        static_text_prefab = Resources.Load("prefabs/ui/text_line") as GameObject;
    }

    // main functions
    public void addFloatingText(string text, Vector3 position, string color="white")
    {
        // crée un texte qui monte et disparait
        GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity) as GameObject;
        floating_text.GetComponent<FloatingText>().init(text, color, 30f, 0.1f, 0.2f, 16f);
        floating_text.transform.SetParent(transform);
    }

    public GameObject addStaticText(string text, Vector3 position, string color="white",float ttl=-1f)
    {
        // crée un texte qui reste puis disparait d'un coup
        GameObject static_text = Instantiate(static_text_prefab, position+offset, Quaternion.identity) as GameObject;
        static_text.GetComponent<FloatingText>().init(text, color, 20f, 0f, 0f, ttl);
        static_text.transform.SetParent(transform);

        return static_text;
    }

    public void talk(string text, Being being)
    {
        // we make the being talk

        // on crée un texte static qui reste accroché au being.
        // parle line par line
        // text = "/./l" + text;
        string[] lines = text.Split("/l");
        StartCoroutine(talkLines(lines, being));
    }

    IEnumerator talkLines(string[] lines, Being being)
    {
        Transform voice = being.transform.Find("voice");

        // on affiche les lignes
        for (int i = 0; i < lines.Length; i++)
        {
            // skip lines' waiting
            bool skip_line_waiting = false;


            string line = lines[i];

            // on calcule la taille de la ligne
            // float height = (line.Split('\n').Length) * line_offset;

            // on monte tous les sentences déjà présentes dans le voice
            for (int j = 0; j < voice.childCount; j++)
            {
                Transform old_sentence = voice.GetChild(j);
                old_sentence.position += new Vector3(0, line_offset, 0);
            }

            // on ajoute une ligne de texte
            GameObject sentence = addStaticText("", being.transform.position + offset, "white", 1000000f);
            sentence.transform.SetParent(voice);
            // on ajoute les caractères un par un
            for (int j = 0; j < line.Length; j++)
            {
                if (line[j] == '\n')
                {
                    // on monte tous les sentences déjà présentes dans le voice
                    for (int k = 0; k < voice.childCount; k++)
                    {
                        Transform old_sentence = voice.GetChild(k);
                        if (old_sentence == sentence.transform) { continue; }
                        old_sentence.position += new Vector3(0, line_offset, 0);
                    }
                }
                else if (line[j] == '/' && j < line.Length - 1 && line[j + 1] == '.')
                {
                    yield return new WaitForSeconds(caractere_delay);
                    sentence.GetComponent<TextMeshPro>().text += '.';
                    yield return new WaitForSeconds(caractere_delay);
                    sentence.GetComponent<TextMeshPro>().text += '.';
                    string old_text = sentence.GetComponent<TextMeshPro>().text;
                    yield return new WaitForSeconds(slow_caractere_delay);
                    sentence.GetComponent<TextMeshPro>().text += '.';

                    // on clignote un petit peu
                    for (int arghfsdf =0; arghfsdf<2; arghfsdf++)
                    {
                        yield return new WaitForSeconds(slow_caractere_delay);
                        sentence.GetComponent<TextMeshPro>().text = old_text;
                        yield return new WaitForSeconds(slow_caractere_delay);
                        sentence.GetComponent<TextMeshPro>().text += '.';
                    }
                    sentence.GetComponent<TextMeshPro>().text += ' ';

                    // on saute le caractère suivant
                    j++;
                    if (j >= line.Length - 1)
                    {
                        // on met à jour le ttl
                        skip_line_waiting = true;
                    }

                    continue;
                }

                yield return new WaitForSeconds(caractere_delay);
                sentence.GetComponent<TextMeshPro>().text += line[j];
            }
            // on met à jour le ttl
            sentence.GetComponent<FloatingText>().setTTL(ttl_sentence);

            // on attend un peu (sauf si on a sauté la ligne)
            if (skip_line_waiting) { continue; }
            yield return new WaitForSeconds(line_delay);
        }
    }    

}