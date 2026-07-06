using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPooler : MonoBehaviour
{

    // cette classe permet de générer des dégats flottants au dessus d'un objet
    // elle est utilisée par les beings pour afficher les dégats qu'ils reçoivent

    // PREFABS
    private GameObject floating_dmg_prefab;


    [Header("Loaded floating texts")]
    private HashSet<FloatingText> loaded_texts = new HashSet<FloatingText>();
    private Stack<FloatingText> sleeping_texts = new Stack<FloatingText>();

    // AWAKE & INSTANCE
    public static FloatingTextPooler Instance { get; private set; }
    protected void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }
    }

    // START
    protected void Start()
    {
        // on récupère le prefab
        floating_dmg_prefab = Resources.Load<GameObject>("prefabs/ui/floating_text");
    }

    // DMG POOLING
    public FloatingText LoadText(string content, string color, Vector3 position, float size)
    {
        FloatingText text;

        // if we dont have any sleeping dmg, we create a new one
        if (sleeping_texts.Count == 0) { text = Instantiate(floating_dmg_prefab, transform).GetComponent<FloatingText>(); }

        // we pop the dmg if we have one
        else
        {
            text = sleeping_texts.Pop();
            text.gameObject.SetActive(true);
        }

        // we load it with the right text, color and position
        text.transform.position = position;
        text.Init(content, color, size);
        loaded_texts.Add(text);

        return text;
    }
    /// <summary>
    /// calling this method randomly can cause the update loop
    /// to throw an error bcz collection is modified
    /// while iterating
    /// </summary>
    /// <param name="text"></param>
    private void unload_text(FloatingText text)
    {
        // we disable the dmg and push it to the sleeping stack
        text.gameObject.SetActive(false);
        sleeping_texts.Push(text);
        loaded_texts.Remove(text);
    }


    // UPDATE
    private readonly Stack<FloatingText> outdated_texts = new Stack<FloatingText>();
    protected void Update()
    {
        // memorize the text to unload
        outdated_texts.Clear();

        // update all loaded texts
        foreach (FloatingText text in loaded_texts)
        {
            text.ttl -= Time.deltaTime;
            if (text.ttl < 0)
            {
                // on le retient pour le supprimer
                outdated_texts.Push(text);
                continue;
            }

            // on fait monter le texte
            text.transform.position += new Vector3(0, text.speed * Time.deltaTime, 0);

            // on fait disparaitre le texte
            Color color = text.TextMesh.color;
            color.a -= text.fade_out_speed * Time.deltaTime;
            text.TextMesh.color = color;
        }

        // on unload les outdated texts
        foreach (FloatingText text in outdated_texts) { unload_text(text); }
    }

}