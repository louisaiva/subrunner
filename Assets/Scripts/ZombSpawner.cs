using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombSpawner : MonoBehaviour 
{

    // a ZombSpawner is a spawner of zombos
    // spawns zombos in a circle around the spawner

    // SPAWNIN
    public float spawn_rate = 1f; // zombos per second
    public float spawn_timer = 0f;
    public float spawn_radius = 2f;

    // ZOMBO
    public GameObject zombo_prefab;

    /*


    */

    // unity functions
    void Start(){

        // on récupère le prefab du zombo
        zombo_prefab = Resources.Load("prefabs/zombo") as GameObject;

    }

    // update de d'habitude
    void Update()
    {
        
        // on update le timer
        spawn_timer += Time.deltaTime;

        // on spawn des zombos
        if (spawn_timer > 1f / spawn_rate)
        {
            spawn_zombo();
            spawn_timer = 0f;
        }

    }

    // SPAWNIN

    void spawn_zombo(){

        // spawn un zombo

        // on calcule la position du zombo
        float angle = Random.Range(0f, 2f * Mathf.PI);
        Vector2 spawn_pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawn_radius + (Vector2)transform.position;

        // on instancie le zombo
        GameObject zombo = Instantiate(zombo_prefab, spawn_pos, Quaternion.identity);

        // on lui donne un parent
        zombo.transform.parent = transform;
    }

}
