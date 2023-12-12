using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class PreSectorSimulatedVisualiser : MonoBehaviour
{
    [Header("pre-sectors")]
    [SerializeField] private PreSector preSector;

    [Header("colors")]
    [SerializeField] private Color base_color;
    [SerializeField] private Color done_color;

    // [Header("physics")]
    // [SerializeField] private LayerMask layerMask;

    [Header("end of simulation")]
    public bool end_of_simu = false;
    [SerializeField] private float time_to_sleep = 0.5f;
    [SerializeField] private Vector3 last_position;
    [SerializeField] private float last_time;


    void Start()
    {
        // on applique la couleur de base
        GetComponent<SpriteRenderer>().color = base_color;

        // on applique les dimensions du pre-sector
        // GetComponent<BoxCollider2D>().size = preSector.getSize();
        transform.localScale = new Vector3(preSector.w, preSector.h, 0.1f);

        // on applique la position du pre-sector
        transform.position = new Vector3(preSector.x, preSector.y, 3f);

        // on met un sprite
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("sprites/utils/white");

        // on met un material
        GetComponent<SpriteRenderer>().material = Resources.Load<Material>("materials/sprite_unlit_default");

        // on change le renderlayer à up pour que ça soit au dessus de tout
        GetComponent<SpriteRenderer>().sortingLayerName = "up";

        // on met un rigidbody en dynamic
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        // on désactive la gravité
        GetComponent<Rigidbody2D>().gravityScale = 0f;

        // on désactive la rotation
        GetComponent<Rigidbody2D>().freezeRotation = true;
    }

    public void init(PreSector preSector, Color base_color, Color done_color)
    {
        this.preSector = preSector;
        this.base_color = base_color;
        this.done_color = done_color;
    }

    void Update()
    {


        // on vérifie si on est en train de bouger
        if (transform.position == last_position)
        {
            // on vérifie combien de temps on est resté immobile
            if (Time.time - last_time > time_to_sleep)
            {
                endOfSimu();
            }
        }
        else
        {
            // on met à jour la dernière position
            last_position = transform.position;

            // on met à jour le dernier temps
            last_time = Time.time;

            // on met à jour la couleur
            GetComponent<SpriteRenderer>().color = base_color;

            // on met à jour l'end of simu
            end_of_simu = false;
        }


        // on colle le box collider à la grille
        // on calcule l'offset entre la grille et notre position
        Vector2 offset = new Vector2(
            Mathf.RoundToInt(transform.position.x) - transform.position.x,
            Mathf.RoundToInt(transform.position.y) - transform.position.y
        );

        // on applique l'offset au box collider
        GetComponent<BoxCollider2D>().offset = offset;
    }

    void PhysicsUpdate()
    {
        // on colle le box collider à la grille
        // on calcule l'offset entre la grille et notre position
        Vector2 offset = new Vector2(
            transform.position.x - Mathf.RoundToInt(transform.position.x),
            transform.position.y - Mathf.RoundToInt(transform.position.y)
        );

        // on applique l'offset au box collider
        GetComponent<BoxCollider2D>().offset = offset;



    }

    public void endOfSimu()
    {
        // on applique la couleur de done
        GetComponent<SpriteRenderer>().color = done_color;
        
        // on récupère la position du visu
        Vector3 pos = transform.position;

        // on met à jour la position du pre-sector
        /* if (pos.x > 0) { preSector.x = Mathf.CeilToInt(pos.x); }
        else { preSector.x = Mathf.FloorToInt(pos.x); }
        if (pos.y > 0) { preSector.y = Mathf.CeilToInt(pos.y); }
        else { preSector.y = Mathf.FloorToInt(pos.y); } */
        preSector.x = Mathf.RoundToInt(pos.x);
        preSector.y = Mathf.RoundToInt(pos.y);

        // on met à jour la position du visu
        // transform.position = new Vector3(preSector.x, preSector.y, 3f);

        // on met à jour l'end of simu
        end_of_simu = true;
    }

}