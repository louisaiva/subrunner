
using System;
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

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }


    // USEFUL GLOBAL METHODS
    public bool IsKind(Type kind, Type ref_kind)
    {
        bool is_same_or_subclass = kind == ref_kind || kind.IsSubclassOf(ref_kind);
        return is_same_or_subclass;
    }

}