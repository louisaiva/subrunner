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
    public int max_zombies_in_absence_of_player = 4;
    public bool perso_is_in_range = false;
    public float perso_range = 10f;

    // ZOMBO
    public GameObject zombo_prefab;

    // PERSO
    public GameObject perso;


    // unity functions
    void Start(){

        // on récupère le prefab du zombo
        if (!zombo_prefab) { zombo_prefab = Resources.Load("prefabs/beings/enemies/zombo") as GameObject; }

        // on récupère le perso
        perso = GameObject.Find("/perso");

    }

    // update de d'habitude
    void Update()
    {
        
        // on update le timer
        spawn_timer += Time.deltaTime;

        // on vérifie si le perso est dans le range
        perso_is_in_range = Vector2.Distance(transform.position, perso.transform.position) < perso_range;

        // si le perso n'est pas dans le range et qu'il y a trop de zombos, on quitte
        if (!perso_is_in_range && transform.childCount >= max_zombies_in_absence_of_player)
        {
            return;
        }

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
