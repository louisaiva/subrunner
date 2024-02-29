using UnityEngine;
using System.Collections.Generic;

public class TutorielManager : MonoBehaviour
{
    // this class helps to manage diverse aspects of how we teach the player the mechanics of the game
    // -> helps with Interactable objects

    [Header("First Interaction Labels")]
    [SerializeField] private Dictionary<string, int> shouldInteractionLabelBeDrawn = new Dictionary<string, int>();
    [SerializeField] private int nb_interactions_before_hiding = 3;

    private void Start()
    {
        // initialize the dictionary
        shouldInteractionLabelBeDrawn.Add("door", 0);
        shouldInteractionLabelBeDrawn.Add("computer", 0);
        shouldInteractionLabelBeDrawn.Add("button", 0);
        shouldInteractionLabelBeDrawn.Add("fridge", 0);
        shouldInteractionLabelBeDrawn.Add("trash_container", 0);
        shouldInteractionLabelBeDrawn.Add("play_station", 0);
        shouldInteractionLabelBeDrawn.Add("tv", 0);
        shouldInteractionLabelBeDrawn.Add("item", 0);
        shouldInteractionLabelBeDrawn.Add("xp_chest", 0);
        shouldInteractionLabelBeDrawn.Add("server_rack", 0);
        shouldInteractionLabelBeDrawn.Add("tube_healing", 0);
        shouldInteractionLabelBeDrawn.Add("stand", 0);
    }

    // GETTERS & SETTERS
    public bool shouldInteractLabelAppear(string key)
    {
        if (!shouldInteractionLabelBeDrawn.ContainsKey(key))
        {
            Debug.LogError("(TutorielManager) shouldAppear : " + key + " not found in dictionary");
            return false;
        }

        return shouldInteractionLabelBeDrawn[key] < nb_interactions_before_hiding;
    }

    public void interact(string key)
    {
        if (!shouldInteractionLabelBeDrawn.ContainsKey(key))
        {
            Debug.LogError("(TutorielManager) interact: "+key+" not found in dictionary");
            return;
        }

        shouldInteractionLabelBeDrawn[key]++;
    }

}