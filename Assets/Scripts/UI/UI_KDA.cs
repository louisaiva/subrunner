using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_KDA : MonoBehaviour
{
    // GAMEOBJECTS
    public TextMeshProUGUI kda;

    AttackCapacity attack;
    DieCapacity die;

    private void Start()
    {
        kda = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (Perso.Instance == null) { return; }

        // we get the capacities
        if (attack == null) { attack = Perso.Instance.GetCapacity("attack") as AttackCapacity; }
        if (die == null) { die = Perso.Instance.GetCapacity("die") as DieCapacity; }

        // KDA
        kda.text = "KDA : "+attack?.kills + "/" + die.deaths;
    }
}