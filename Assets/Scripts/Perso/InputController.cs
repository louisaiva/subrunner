using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class InputController : MonoBehaviour
{

    [Header("Endless Inputs")]
    [SerializeField] protected List<EndlessInput<float>> endless_floats = new List<EndlessInput<float>>();
    [SerializeField] protected List<EndlessInput<Vector2>> endless_vectors = new List<EndlessInput<Vector2>>();

    [Header("Logs")]
    public bool log_endless_inputs = false;


    // ENDLESS INPUTS
    public EndlessInput<T> add_endless_input<T>(EndlessInput<T> input) where T : struct
    {
        if (typeof(T) == typeof(float))
        {
            endless_floats.Add(input as EndlessInput<float>);
            return input;
        }
        else if (typeof(T) == typeof(Vector2))
        {
            endless_vectors.Add(input as EndlessInput<Vector2>);
            return input;
        }
        if (log_endless_inputs) { Debug.LogWarning($"(PersoInputsController) Can't add {input.name} EndlessInput of type " + typeof(T) + " is not supported"); }
        return null; // not supported type
    }
    public EndlessInput<T> get_endless_input<T>(string name) where T : struct
    {
        // todo : optimizable via caching into a dictionary ?
        if (typeof(T) == typeof(float))
        {
            return endless_floats.FirstOrDefault(inp => inp.name == name) as EndlessInput<T>;
        }
        else if (typeof(T) == typeof(Vector2))
        {
            return endless_vectors.FirstOrDefault(inp => inp.name == name) as EndlessInput<T>;
        }

        if (log_endless_inputs) { Debug.LogWarning($"(PersoInputsController) Can't find {name} EndlessInput of type " + typeof(T) + " may be not supported"); }
        return null;
    }

}