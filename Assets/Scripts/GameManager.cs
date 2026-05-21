
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;



/// <summary>
/// this class handles few global things related
/// to the game. It means it is created only when entering
/// a game scene (it does not exist in the main menu)
/// It is situated on "/game" gameObject
/// </summary>
public class GameManager : MonoBehaviour
{


    // LAZY INSTANCE
    private static GameManager _instance;
    public static GameManager LazyInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
                if (_instance == null) { Debug.LogError("No (GameManager) instance found in the scene."); }
            }
            return _instance;
        }
    }

    // GAME STATE
    [SerializeField] private GameState _state = GameState.NoGame;
    public static GameState State
    {
        get
        {
            if (LazyInstance == null) { return GameState.NoGame; }
            return LazyInstance._state;
        }
        set
        {
            if (LazyInstance == null) { return; }
            LazyInstance._state = value;
        }
    }
    public static bool IsClosingGame => State == GameState.NoGame;

    [Header("Game Music Theme")]
    [SerializeField] private string game_theme_to_play = "i'm so hungry";


    // AWAKE & START
    private void Awake()
    {
        _state = GameState.Loading;

        // we launch the world from WorldManager
        WorldManager.LazyInstance.LoadSelectedWorld();
    }
    private void Start()
    {
        // if we have a MusicPlayer Instance
        MusicPlayer.Instance.PlayTheme(game_theme_to_play);
    }


    // USEFUL GLOBAL METHODS


    // KIND & TYPE CHECKING
    public static bool IsKind(Type kind, Type ref_kind)
    {
        bool is_same_or_subclass = kind == ref_kind || kind.IsSubclassOf(ref_kind);
        return is_same_or_subclass;
    }
    public static bool IsKind(string kind, string ref_kind)
    {
        Type type_kind = Type.GetType(kind);
        Type type_ref_kind = Type.GetType(ref_kind);
        if (type_kind == null || type_ref_kind == null) { return false; }
        return IsKind(type_kind, type_ref_kind);
    }
    public static bool IsKind(string kind, string ref_kind, out int inheritance_distance)
    {
        if (!IsKind(kind, ref_kind)) { inheritance_distance = -1; return false; }
        inheritance_distance = calculate_type_distance(Type.GetType(kind), Type.GetType(ref_kind));
        return true;
    }
    private static int calculate_type_distance_one_way(Type firstType, Type secondType)
    {
        var chain = new List<Type>();
        while (firstType != typeof(object))
        {
            chain.Add(firstType);
            firstType = firstType.BaseType;
        }

        return chain.IndexOf(secondType);
    }
    public static int calculate_type_distance(Type firstType, Type secondType)
    {
        int result = calculate_type_distance_one_way(firstType, secondType);
        if (result >= 0)
        {
            return result;
        }

        return calculate_type_distance_one_way(secondType, firstType);
    }

    // ON DESTROY
    private void OnDestroy()
    {
        _state = GameState.NoGame;
        _instance = null;
    }
}


public enum GameState
{
    NoGame,
    Loading,
    Gaming,
    Paused,
    Building, // inside the world builder
}