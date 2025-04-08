using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class is used as a helper to assemble the motherboard of the right size.
/// </summary>
public class MotherboardBuilder : MonoBehaviour
{
    [Header("Motherboard parameters")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 4;
    public Vector2Int Size {get { return new Vector2Int(columns, rows); }
        set
        {
            // we check if the value is valid
            if (value.x < 1 || value.y < 1) { return; }

            BuildMotherboard(value.x, value.y);

            // we put the values
            columns = value.x;
            rows = value.y;
        }
    }


    [Header("MB building")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject empty_slot_prefab;

    [Header("MB perimeter")]
    [SerializeField] private RectTransform UL_corner;
    [SerializeField] private RectTransform UR_corner, DL_corner, DR_corner;
    [SerializeField] private RectTransform U_side, D_side, L_side, R_side;
    [SerializeField] private RectTransform label_UL;
    [SerializeField] private RectTransform inside;


    // HELPERS
    private float px_size = 8.33f; // 12 px = 100 units  100/12 = 8.33

    // AWAKE
    private void Awake()
    {
        // we build the motherboard
        BuildMotherboard(columns, rows);
    }
    
    // BUILD MOTHERBOARD
    public void BuildMotherboard(int columns, int rows)
    {
        // we calculate the width and height of the motherboard
        calculate_width_and_height(new Vector2Int(columns, rows));

        // we place the corners
        UL_corner.anchoredPosition = new Vector2(-width/2, height/2);
        UR_corner.anchoredPosition = new Vector2(width/2, height/2);
        DL_corner.anchoredPosition = new Vector2(-width/2, -height/2);
        DR_corner.anchoredPosition = new Vector2(width/2, -height/2);

        // we place the label
        label_UL.anchoredPosition = new Vector2(-width/2, height/2);
        label_UL.sizeDelta = new Vector2(56*px_size, 12*px_size);

        // we place the sides
        U_side.anchoredPosition = new Vector2(-width/2 + 56*px_size, height/2);
        D_side.anchoredPosition = new Vector2(0, -height/2);
        L_side.anchoredPosition = new Vector2(-width/2, 0);
        R_side.anchoredPosition = new Vector2(width/2, 0);

        // we size the sides
        U_side.sizeDelta = new Vector2(width - 56 * px_size, 12*px_size);
        D_side.sizeDelta = new Vector2(width, 12*px_size);
        L_side.sizeDelta = new Vector2(12*px_size, height);
        R_side.sizeDelta = new Vector2(12*px_size, height);

        // we size the inside
        inside.sizeDelta = new Vector2(width, height);

        // we set the inside column count
        inside.GetComponent<GridLayoutGroup>().constraintCount = columns;

        // we remove the old empty slots
        for (int i = inside.childCount; i > 0; --i)
        {
            DestroyImmediate(inside.GetChild(0).gameObject);
        }

        // and we put enough empty slots in the inside
        // to fill the whole motherboard
        int empty_slots = columns * rows;
        for (int i = 0; i < empty_slots; i++)
        {
            GameObject empty_slot = Instantiate(empty_slot_prefab, inside);
            empty_slot.name = "empty_module_slot_" + i;
        }
    }
    private void calculate_width_and_height(Vector2Int size)
    {
        // we calculate the width and height of the motherboard
        width = size.x * 56 // the width of all the slots
            + (size.x - 1) * 2; // the space between the slots
        height = size.y * 20 // the height of all the slots
            + (size.y - 1) * 1; // the space between the slots

        // then we multiply by the size of 1 px in the canvas
        // 100 = 12px so 1px = 100/12 = 8.33
        width = (int)(width * px_size);
        height = (int)(height * px_size);
    }


}