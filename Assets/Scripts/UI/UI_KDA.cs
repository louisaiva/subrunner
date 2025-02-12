using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_KDA : MonoBehaviour
{
    // GAMEOBJECTS
    public Perso perso;
    public TextMeshProUGUI kda;

    AttackCapacity attack;
    DieCapacity die;

    private void Start()
    {
        perso = GameObject.Find("/perso").GetComponent<Perso>();
        kda = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // we get the capacities
        if (attack == null) { attack = perso.GetCapacity("attack") as AttackCapacity; }
        if (die == null) { die = perso.GetCapacity("die") as DieCapacity; }

        // KDA
        kda.text = "KDA : "+attack?.kills + "/" + die.deaths;
    }
}