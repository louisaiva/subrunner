using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IF_Bank : MonoBehaviour
{
    public List<IF_Prefab> kb_prefabs;
    public List<IF_Prefab> gm_prefabs;

    [Header("Keyboard dynamically generated IFs icons")]
    public List<IF_Icon> icons;
    public InputFeedback key_prefab;
    private Dictionary<string,InputAction> dynamic_actions = new Dictionary<string, InputAction>();

    [Header("Logs")]
    [SerializeField] private bool log_dynamic_bindings = false;


    private List<string> dynamic_kb_bindings = new List<string>
    {
        "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","1","2","3","4","5","6","7","8","9","0"
    };
    public void GenerateDynamicKeyboardBindings(InputActionMap map)
    {
        Sprite icon;
        string debug = "";
        foreach (string bind in dynamic_kb_bindings)
        {
            // we check if we have an icon for it
            icon = get_icon(bind);
            if (icon == null) { continue; }
            var action = map.AddAction("dynamic_kb_"+bind);
            action.AddBinding("<Keyboard>/"+bind, groups:"keyboard");
            debug += $"\n  - '{bind}' binding generated for action : {action}";
            dynamic_actions.Add(bind,action);
        }

        if (log_dynamic_bindings)
        {
            Debug.Log($"(IF_Bank) generated {dynamic_actions.Count} dynamic keyboard bindings : {debug}");
        }
    }
    private Sprite get_icon(string bind)
    {
        foreach (IF_Icon ifi in icons)
        {
            if (ifi.binding != bind) { continue; }
            return ifi.icon;
        }
        return null;
    }




    // INSTANCIATOR
    public InputFeedback Instantiate(string binding, Transform parent, bool gamepad = true)
    {
        InputFeedback prefab;
        InputFeedback ef;
        if (gamepad == false)
        {
            prefab = GetKeyboardIF(binding);
            if (prefab != null) { return instantiate_and_anchor(prefab, parent); }

            // here we may want a dynamic input feedback
            if (!dynamic_actions.TryGetValue(binding, out InputAction action))
            {
                Debug.LogError($"(IF_Bank) Tried to instantiate a keyboard IF for binding '{binding}' but could not found existing KB or dynamically generated one...");
                return null;
            }
            ef = instantiate_and_anchor(key_prefab, parent);
            ef.InitializeWithAction(action);
            ef.transform.Find("icon").GetComponent<Image>().sprite = get_icon(bind:binding);
            return ef;
        }

        prefab = GetGamepadIF(binding);
        ef = instantiate_and_anchor(prefab,parent);
        return ef;
    }
    private InputFeedback instantiate_and_anchor(InputFeedback prefab, Transform parent)
    {
        InputFeedback ef = Instantiate(prefab, parent);
        // ef.transform.localScale = .25f * Vector3.one;
        RectTransform rect = ef.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f,0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return ef;
    }

    // GETTERS
    private InputFeedback GetKeyboardIF(string binding)
    {
        foreach (IF_Prefab ifp in kb_prefabs)
        {
            if (ifp.binding != binding) { continue; }
            return ifp.prefab;
        }
        // Debug.LogWarning("(IF_Bank) No InputFeedback found on manual keyboards IFs for binding : " + binding);
        return null;
    }
    private InputFeedback GetGamepadIF(string binding)
    {
        foreach (IF_Prefab ifp in gm_prefabs)
        {
            if (ifp.binding != binding) { continue; }
            return ifp.prefab;
        }
        Debug.LogError("(IF_Bank) No InputFeedback found on gamepad for binding : " + binding);
        return null;
    }
}

[Serializable] public class IF_Prefab
{
    public string binding;
    public InputFeedback prefab;
}

[Serializable] public class IF_Icon
{
    public string binding;
    public Sprite icon;
}