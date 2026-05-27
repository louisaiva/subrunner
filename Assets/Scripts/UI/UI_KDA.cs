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
        if (Controller.Perso == null) { return; }

        // we get the capacities
        if (attack == null) { attack = Controller.Perso.GetCapacity<AttackCapacity>(); }
        if (die == null) { die = Controller.Perso.GetCapacity<DieCapacity>(); }

        // KDA
        kda.text = "KDA : "+attack?.kills + "/" + die.deaths;
    }
}