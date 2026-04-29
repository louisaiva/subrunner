
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
    public static bool IsClosingGame = false;

    [Header("Game Music Theme")]
    [SerializeField] private string game_theme_to_play = "i'm so hungry";
    [SerializeField] private string world_to_load = "";

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
        IsClosingGame = false;

        // we instantly load the world even if World.Instance is not defined yet.
        // so we use World.StaticInstance which will find the world instance with FindObjectByType
        World world = World.StaticInstance;
        if (world == null) { return; }

        // we load the good world.
        // for this we have multiple choices :
        // 1. if WorldManager.SelectedWorld is defined, we load it (this is set by the world selector in the main menu)
        // 2. else if world_to_load is defined in the inspector
        // 3. else if the world instance has a world_id defined, we load it

        if (WorldManager.Instance != null && !string.IsNullOrEmpty(WorldManager.Instance.SelectedWorld)) { world.LoadWorld(WorldManager.Instance.SelectedWorld); }
        else if (world_to_load != "") { world.LoadWorld(world_to_load); }
        else if (world.world_id != "") { world.LoadWorld(world.world_id); }
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
        IsClosingGame = true;
        Instance = null;
    }
}