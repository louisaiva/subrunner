using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class InputController : MonoBehaviour
{

    [Header("Endless Inputs")]
    [SerializeField] private List<EndlessInput<float>> endless_floats = new List<EndlessInput<float>>();

    [Header("Logs")]
    public bool log_endless_inputs = false;


    // ENDLESS INPUTS
    protected EndlessInput<T> add_endless_input<T>(EndlessInput<T> input) where T : struct
    {
        if (typeof(T) == typeof(float))
        {
            endless_floats.Add(input as EndlessInput<float>);
            return input;
        }
        if (log_endless_inputs) { Debug.LogWarning($"(PersoInputsController) Can't add {input.name} EndlessInput of type " + typeof(T) + " is not supported"); }
        return null; // not supported type
    }
    protected EndlessInput<T> get_endless_input<T>(string name) where T : struct
    {
        // todo : optimizable via caching into a dictionary ?
        if (typeof(T) == typeof(float))
        {
            return endless_floats.FirstOrDefault(inp => inp.name == name) as EndlessInput<T>;
        }

        if (log_endless_inputs) { Debug.LogWarning($"(PersoInputsController) Can't find {name} EndlessInput of type " + typeof(T) + " may be not supported"); }
        return null;
    }

}