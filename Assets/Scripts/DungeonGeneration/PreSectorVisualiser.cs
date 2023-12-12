using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class PreSectorVisualiser : MonoBehaviour
{
    [Header("pre-sectors")]
    private PreSector preSector;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private int w;
    [SerializeField] private int h;

    [Header("colors")]
    [SerializeField] private Color base_color;
    [SerializeField] private Color done_color;


    void Start()
    {
        // on applique la couleur de base
        GetComponent<SpriteRenderer>().color = base_color;

        // on applique les dimensions du pre-sector
        transform.localScale = new Vector3(preSector.w, preSector.h, 0.1f);

        // on applique la position du pre-sector
        transform.position = new Vector3(preSector.x, preSector.y, 3f);

        // on met un sprite
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("sprites/utils/white");

        // on met un material
        GetComponent<SpriteRenderer>().material = Resources.Load<Material>("materials/sprite_unlit_default");

        // on change le renderlayer à up pour que ça soit au dessus de tout
        GetComponent<SpriteRenderer>().sortingLayerName = "up";

    }

    public void init(PreSector preSector, Color base_color, Color done_color)
    {
        this.preSector = preSector;
        this.base_color = base_color;
        this.done_color = done_color;
    }

    void Update()
    {
        // on met à jour la position du pre-sector
        transform.position = new Vector3(preSector.cx(), preSector.cy(), 3f);
        x = preSector.x;
        y = preSector.y;
        w = preSector.w;
        h = preSector.h;
    }

}