using System.Collections.Generic;
using UnityEngine;

public class SeeThroughHandler : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_PlayerScreenPosition");
    public static int SizeID = Shader.PropertyToID("_EllipseSize");
    public static int Y_PlayerID = Shader.PropertyToID("_Y_Player");
    public static int WorldPosID = Shader.PropertyToID("_PlayerWorldPosition");
    // public static int Y_HitID = Shader.PropertyToID("_Y_Hit");

    [Header("Parameters")]
    public float current_size_percentage = 0f; // current size % of the circle (0 to 1)
    public float ceiling_desired_size = 1f; // max size of the circle
    public float wall_desired_size = 1f; // max size of the circle

    public float size_up_speed = 1f; // speed at which the circle grows
    public Vector2 offset_ellipse_center = new Vector2(0f, 0f);


    [Header("Materials")]
    public Material see_through_material_lit;
    public Material see_through_material_unlit;
    public Material y_see_through_material_lit;
    public Material y_see_through_material_unlit;
    // public Material laser_material_lit;

    [Header("Components")]
    private Camera cam;
    public Collider2D trigger;
    public LayerMask walls_and_ceiling_mask;
    public ContactFilter2D overlap_filter = new ContactFilter2D();

    [Header("Wall hit")]
    public Wall wall_hit = null;


    [Header("Debug")]
    public bool debug = false;
    public bool debug_Y_Comparison = false;

    void Start()
    {
        cam = Camera.main;

        // we get the height of the current sprite
        SpriteRenderer sprite_renderer = GetComponent<SpriteRenderer>();
        float sprite_height = sprite_renderer.bounds.size.y;
        offset_ellipse_center.y = sprite_height / 2f;

        // we get the player body
        if (trigger == null)
        {
            trigger = transform.Find("head").GetComponent<Collider2D>();
        }

        // we set the contact filter
        overlap_filter.SetLayerMask(walls_and_ceiling_mask);
        overlap_filter.useLayerMask = true;
        overlap_filter.useTriggers = true;
    }

    void Update()
    {
        // we get the center of the ellipse
        Vector3 see_through_center = offset_ellipse_center;
        see_through_center.x += transform.position.x;
        see_through_center.y += transform.position.y;


        calculate_size_percentage();


        // we send infos to the shaders

        // we set the size of the ellipse
        y_see_through_material_lit.SetFloat(SizeID, current_size_percentage * wall_desired_size);
        y_see_through_material_unlit.SetFloat(SizeID, current_size_percentage * ceiling_desired_size);

        see_through_material_lit.SetFloat(SizeID, current_size_percentage*wall_desired_size);
        see_through_material_unlit.SetFloat(SizeID, current_size_percentage * ceiling_desired_size);

        // we send the player screen position to the shader
        Vector3 player_screen_pos = cam.WorldToViewportPoint(see_through_center);
        y_see_through_material_lit.SetVector(PosID, player_screen_pos);
        y_see_through_material_unlit.SetVector(PosID, player_screen_pos);

        see_through_material_lit.SetVector(PosID, player_screen_pos);
        see_through_material_unlit.SetVector(PosID, player_screen_pos);

        // we send the world Y position of the player to the shader
        y_see_through_material_lit.SetFloat(Y_PlayerID, transform.position.y);
        y_see_through_material_unlit.SetVector(WorldPosID, transform.position);
    }

    private void calculate_size_percentage()
    {

        // we check if our body overlaps with the walls
        List<Collider2D> hits = new List<Collider2D>();
        Physics2D.OverlapCollider(trigger, overlap_filter, hits);

        // we check if the raycast hit something
        if (hits.Count == 0)
        {
            if (debug) { Debug.Log("(SeeThroughHandler) hit : nothing"); }
            current_size_percentage = Mathf.Lerp(current_size_percentage, 0f, size_up_speed * Time.deltaTime);

            // we unsee through the wall if we were seeing through it
            if (wall_hit != null)
            {
                wall_hit.UnSeeThrough();
                wall_hit = null;
            }
            return;
        }

        // we make the circle grow
        current_size_percentage = Mathf.Lerp(current_size_percentage, 1f, size_up_speed * Time.deltaTime);

        // we check if the hit object is a wall
        Wall wall = hits[0].transform.parent.GetComponent<Wall>();
        if (wall != null && wall_hit != wall)
        {
            if (debug) { Debug.Log("(SeeThroughHandler) hit a wall : " + wall.name); }

            // we unsee through the previous wall
            if (wall_hit != null)
            {
                wall_hit.UnSeeThrough();
            }

            // we see through the new wall
            wall_hit = wall;
            wall.SeeThrough();
        }
        else if (wall == wall_hit && debug) { Debug.Log("(SeeThroughHandler) hit the same wall : " + wall.name); }
        else if (debug) { Debug.Log("(SeeThroughHandler) hit something : " + hits[0].name); }

        // we log the position of the hit
        // Vector3 hit_pos = hits[0].transform.position;
        // if (debug_Y_Comparison) { Debug.Log("(SeeThroughHandler) hit : " + hit_pos.y + " player : " + transform.position.y); }
    }
}