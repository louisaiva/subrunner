using UnityEngine;
using TMPro;
using System.Collections;

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
    public void addFloatingText(string text, Vector3 position, string color = "white")
    {
        // crée un texte qui monte et disparait
        // GameObject floating_text = Instantiate(floating_text_prefab, position, Quaternion.identity);
        FloatingText floating_text = GetComponent<FloatingTextPooler>().LoadText(text, color, position + offset, 30f);
        floating_text.Init(text, color, 30f, 0.1f, 0.2f, 16f);
        // floating_text.transform.SetParent(transform);
    }

    public GameObject addStaticText(string text, Vector3 position, string color = "white", float ttl = -1f)
    {
        // crée un texte qui reste puis disparait d'un coup
        FloatingText floating_text = GetComponent<FloatingTextPooler>().LoadText(text, color, position + offset, 30f);
        // GameObject static_text = Instantiate(static_text_prefab, position+offset, Quaternion.identity) as GameObject;
        floating_text.Init(text, color, 20f, 0f, 0f, ttl);
        // floating_text.transform.SetParent(transform);

        return floating_text.gameObject;
    }

    public IEnumerator TalkLines(string text, Transform voice)
    {
        // Transform voice = talker.transform.Find("talk");
        if (voice == null) { yield break; }

        string[] lines = text.Split("/l");

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
            GameObject sentence = addStaticText("", voice.transform.position + offset, "white", 1000000f);
            sentence.transform.SetParent(voice);
            TextMeshPro text_mesh = sentence.GetComponent<TextMeshPro>();
            FloatingText floating_text = sentence.GetComponent<FloatingText>();

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
                    text_mesh.text += '.';
                    yield return new WaitForSeconds(caractere_delay);
                    text_mesh.text += '.';
                    string old_text = text_mesh.text;
                    yield return new WaitForSeconds(slow_caractere_delay);
                    text_mesh.text += '.';

                    // on clignote un petit peu
                    for (int arghfsdf = 0; arghfsdf < 2; arghfsdf++)
                    {
                        yield return new WaitForSeconds(slow_caractere_delay);
                        text_mesh.text = old_text;
                        yield return new WaitForSeconds(slow_caractere_delay);
                        text_mesh.text += '.';
                    }
                    text_mesh.text += ' ';

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
                text_mesh.text += line[j];
            }
            // on met à jour le ttl
            floating_text.SetTTL(ttl_sentence);

            // on attend un peu (sauf si on a sauté la ligne)
            if (skip_line_waiting) { continue; }
            yield return new WaitForSeconds(line_delay);
        }
    }

}