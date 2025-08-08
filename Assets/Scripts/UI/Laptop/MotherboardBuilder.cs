#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class is used as a helper to assemble the motherboard of the right size.
/// </summary>
[ExecuteAlways] public class MotherboardBuilder : MonoBehaviour
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
    [SerializeField] private bool show_label = true;

    [Header("MB building")]
    [SerializeField] private bool update_scale = true;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject empty_slot_prefab;

    [Header("MB perimeter")]
    [SerializeField] private RectTransform UL_corner;
    [SerializeField] private RectTransform UR_corner, DL_corner, DR_corner;
    [SerializeField] private RectTransform U_side, D_side, L_side, R_side;
    [SerializeField] private RectTransform label_UL;
    [SerializeField] private RectTransform slots_parent;

    [Header("Components")]
    [SerializeField] private Canvas canvas;

    [Header("Logs")]
    [SerializeField] private bool debug = false;

    // HELPERS
    private float px_size = 8.33f; // 12 px = 100 units  100/12 = 8.33

    // ON ENABLE
    private void OnEnable()
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
        UL_corner.anchoredPosition = new Vector2(-width / 2, height / 2);
        UR_corner.anchoredPosition = new Vector2(width / 2, height / 2);
        DL_corner.anchoredPosition = new Vector2(-width / 2, -height / 2);
        DR_corner.anchoredPosition = new Vector2(width / 2, -height / 2);

        // we place the label
        if (show_label)
        {
            label_UL.gameObject.SetActive(true);
        }
        else
        {
            label_UL.gameObject.SetActive(false);
        }
        label_UL.anchoredPosition = new Vector2(-width / 2, height / 2);
        label_UL.sizeDelta = new Vector2(56 * px_size, 12 * px_size);

        // we place the sides
        if (show_label) { U_side.anchoredPosition = new Vector2(-width / 2 + 56 * px_size, height / 2); }
        else { U_side.anchoredPosition = new Vector2(-width / 2, height / 2); }
        D_side.anchoredPosition = new Vector2(0, -height / 2);
        L_side.anchoredPosition = new Vector2(-width / 2, 0);
        R_side.anchoredPosition = new Vector2(width / 2, 0);

        // we size the sides
        if (show_label) { U_side.sizeDelta = new Vector2(width - 56 * px_size, 12 * px_size); }
        else { U_side.sizeDelta = new Vector2(width, 12 * px_size); }
        D_side.sizeDelta = new Vector2(width, 12 * px_size);
        L_side.sizeDelta = new Vector2(12 * px_size, height);
        R_side.sizeDelta = new Vector2(12 * px_size, height);

        // we size the slots_parent
        slots_parent.sizeDelta = new Vector2(width, height);

        // todo : cache those getcomponent -> motherboard is disabled so no awake, maybe assign it to awaker manager ?

        // we set the slots_parent column count
        slots_parent.GetComponent<GridLayoutGroup>().constraintCount = columns;

        // we check how many children we have
        int module_slots = columns * rows;
        UI_ModulePool module_pool = slots_parent.GetComponent<UI_ModulePool>();

        // we apply the slots nb to the module_pool max_slots (bcz it is non scalable)
        module_pool.MaxSlots = module_slots;
        if (module_pool.Count > module_slots && Application.isPlaying)
        {
            module_pool.DropOverheadSlots();
        }
        else if (module_pool.Count < module_slots && Application.isPlaying)
        {
            module_pool.CreateEmptySlots(module_slots - module_pool.Count);
        }

        // we resize the entire motherboard to fit perfectly in the canvas
        adjustMBScale();
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


    // ADJUSTING GLOBAL SIZE
    private void adjustMBScale()
    {
        // 1 - we reset the scale of the motherboard to 1
        transform.localScale = new Vector3(1, 1, 1);

        // we calculate the pixel size between the 4 corners of the motherboard
        Rect MB_px = calculate_visual_bounds();

        // we calculate the pixel size of our parent (the size we can use)
        Rect desired_px = RectTransformUtility.PixelAdjustRect(GetComponent<RectTransform>(), canvas);

        // 3 - we adapt the size of the motherboard to fit the available size
        float scaleX = desired_px.width / MB_px.width;
        float scaleY = desired_px.height / MB_px.height;
        float ratio = Mathf.Min(scaleX, scaleY); // preserve aspect ratio

        // we log
        if (debug) {Debug.Log("(MotherboardBuilder) ratio : " + ratio + " | desired_px : " + desired_px.width + "x" + desired_px.height +
            " | MB_px : " + MB_px.width + "x" + MB_px.height);}

        // check if the ratio is valid
        if (ratio <= 0) { return; }
        else if (ratio >= 100000) {return;}

        // we apply the ratio to the scale
        Vector3 newScale = new Vector3(ratio, ratio, ratio);
        transform.localScale = newScale;
    }
    private Rect calculate_visual_bounds()
    {
        // from chat gpt !!

        // we get the world corners
        Vector3[] ul = new Vector3[4];
        Vector3[] dr = new Vector3[4];

        UL_corner.GetWorldCorners(ul);
        DR_corner.GetWorldCorners(dr);

        float xMin = ul[0].x;
        float xMax = dr[2].x;
        float yMin = dr[0].y;
        float yMax = ul[1].y;       

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    // UPDATE
    private Vector2 lastSize;
    void Update()
    {
        if (!update_scale) { return; }

        Vector2 currentSize = GetComponent<RectTransform>().rect.size;
        if (currentSize != lastSize)
        {
            lastSize = currentSize;
            adjustMBScale();
        }
    }

#if UNITY_EDITOR
    private void OnRectTransformDimensionsChange()
    {
        if (!Application.isPlaying)
        {
            // In editor mode!
            if (debug) {Debug.Log("(MotherboardBuilder) OnRectTransformDimensionsChange");}
            adjustMBScale();
        }
    }
#endif
}

