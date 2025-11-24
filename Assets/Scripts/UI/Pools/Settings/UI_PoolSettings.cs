
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PoolSettings", menuName = "UI/Pool Settings")]
[Serializable] public class UI_PoolSettings : ScriptableObject
{
    public float Duration = 0.05f; // default duration of the transition
    public bool CanBeHidden = true; // if true, the pool can be hidden when switching to another pool
    public bool CanBeCanceled = false; // if true, the UI_Manager will save the last pool and go back to it with B
    public bool UsePersoInputs = true; // if true, the UI_Manager will activate the inputs.perso when the pool is showed
    public float TimeScale = 1f; // time scale when the pool is showed
    public float BackgroundAlpha = 0f; // alpha of the background when the pool is showed
    public float ChromaticAberration = 0f; // chromatic aberration effect intensity when the pool is showed
    public float Bloom = 0.3f; // bloom effect intensity when the pool is showed
}
