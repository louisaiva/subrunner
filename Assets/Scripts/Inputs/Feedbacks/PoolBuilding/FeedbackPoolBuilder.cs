using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// this class handles what we can call a Pool of InputFeedback.
/// it has 2 separated input types pool and 2 columns for each input type
/// then inside the columns there can be as many rows as specified
/// and inside each rows as many InputFeedback as specified.
/// 
/// the InputFeedback are got at Start from the FeedbackBank
/// and the plan are transmitted through a specific SO : FeedbackPoolSchematic
/// </summary>
public class FeedbackPoolBuilder : MonoBehaviour
{
    [Header("Layout transforms")]
    protected Transform kb_col_L;
    protected Transform kb_col_R;
    protected Transform gmpd_col_L;
    protected Transform gmpd_col_R;

    [Header("Pool Schematic")]
    [SerializeField] protected FeedbackPoolSchematic pool_schematic;

    [Header("Prefabs")]
    [SerializeField] protected GameObject left_row_prefab;
    [SerializeField] protected GameObject right_row_prefab;

    [Header("Logs")]
    [SerializeField] protected bool log = false;

    protected void Awake()
    {
        // get the transforms
        kb_col_L = transform.Find("keyboard_F/col_L");
        kb_col_R = transform.Find("keyboard_F/col_R");
        gmpd_col_L = transform.Find("gamepad_F/col_L");
        gmpd_col_R = transform.Find("gamepad_F/col_R");
    }

    // START
    protected virtual void Start()
    {
        // build the pool
        buildPool();
    }

    // BUILD POOL
    protected async void buildPool()
    {
        // we go through all 4 columns and for each column we build the rows
        if (log) { Debug.Log("(FeedbackPoolBuilder) Building Feedback Pool from schematic : " + pool_schematic.name); }

        // keyboard left column
        await buildColumn(pool_schematic.kb_L_plan, kb_col_L, is_left: true);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) [{pool_schematic.name}] Built keyboard left column : {pool_schematic.kb_L_plan.Count} rows"); }

        // keyboard right column
        await buildColumn(pool_schematic.kb_R_plan, kb_col_R, is_left: false);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) [{pool_schematic.name}] Built keyboard right column : {pool_schematic.kb_R_plan.Count} rows"); }
        // gamepad left column
        await buildColumn(pool_schematic.gmpd_L_plan, gmpd_col_L, is_left: true);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) [{pool_schematic.name}] Built gamepad left column : {pool_schematic.gmpd_L_plan.Count} rows"); }

        // gamepad right column
        await buildColumn(pool_schematic.gmpd_R_plan, gmpd_col_R, is_left: false);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) [{pool_schematic.name}] Built gamepad right column : {pool_schematic.gmpd_R_plan.Count} rows"); }
    }

    // BUILD COLUMN & ROWS
    protected async Awaitable buildColumn(List<FeedbackRowSchematic> column_plan, Transform column_transform, bool is_left)
    {
        // we build the column row per row
        for (int r = 0; r < column_plan.Count; r++)
        {
            FeedbackRowSchematic row_schem = column_plan[r];
            GameObject row_go = await create_row_go(row_schem, column_transform, is_left);
            row_go.name = $"row_{row_schem.label_text}";
        }
    }
    protected async Awaitable<GameObject> create_row_go(FeedbackRowSchematic row_schem, Transform parent_transform, bool is_left)
    {
        // we get the game object list
        List<GameObject> IFs = row_schem.prefabs;

        // we create the row GameObject
        GameObject row_go = Instantiate(is_left ? left_row_prefab : right_row_prefab, parent_transform);

        // wait a frame to be sure the row is instantiated
        await System.Threading.Tasks.Task.Yield();

        // we set the label
        TextMeshProUGUI label = row_go.transform.Find("text").GetComponent<TextMeshProUGUI>();
        label.text = row_schem.label_text;
        UI_Colorer label_colorer = label.GetComponent<UI_Colorer>();

        // we instantiate the IFs
        for (int i = 0; i < IFs.Count; i++)
        {
            GameObject IF_go = Instantiate(IFs[i], row_go.transform);
            IF_go.transform.localScale = Vector3.one;

            // apply the label colorer to the IF_go too
            Colorant colorant = IF_go.GetComponent<Colorant>();
            if (colorant != null)
            {
                colorant.Colorers.Add(label_colorer);
                colorant.SetColors(row_schem.base_color, row_schem.inputed_color);
            }

            // wait a frame to be sure the row is instantiated
            await System.Threading.Tasks.Task.Yield();

            // set its children index at Count-1 (count always > 0 bcz there is a text in empty row)
            // IF_go.transform.SetSiblingIndex(row_go.transform.childCount - 1);
        }

        return row_go;
    }

    // MANAGE ROWS AT RUNTIME
    public void EnableRows(string row_label, bool kb = true, bool gmpd = true)
    {
        List<Transform> found_rows = GetRows(row_label, kb, gmpd);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) Enabling rows with label {row_label} ({found_rows.Count} found)"); }
        for (int i = 0; i < found_rows.Count; i++)
        {
            found_rows[i].gameObject.SetActive(true);
        }
    }
    public void DisableRows(string row_label, bool kb = true, bool gmpd = true)
    {
        List<Transform> found_rows = GetRows(row_label, kb, gmpd);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) Disabling rows with label {row_label} ({found_rows.Count} found)"); }
        for (int i = 0; i < found_rows.Count; i++)
        {
            found_rows[i].gameObject.SetActive(false);
        }
    }
    public List<Transform> GetRows(string label, bool kb = true, bool gmpd = true)
    {
        List<Transform> found_rows = new List<Transform>();

        // get all rows
        List<Transform> all_rows = get_all_rows(kb, gmpd);

        // checks each row name
        string row_name = $"row_{label}";
        for (int i = 0; i < all_rows.Count; i++)
        {
            if (all_rows[i].name != row_name) { continue; }
            found_rows.Add(all_rows[i]);
        }

        return found_rows;
    }
    public List<Transform> get_all_rows(bool kb = true, bool gmpd = true)
    {
        List<Transform> all_rows = new List<Transform>();

        if (kb)
        {
            // kb_col_L
            for (int i = 0; i < kb_col_L.childCount; i++)
            {
                all_rows.Add(kb_col_L.GetChild(i));
            }

            // kb_col_R
            for (int i = 0; i < kb_col_R.childCount; i++)
            {
                all_rows.Add(kb_col_R.GetChild(i));
            }
        }

        if (gmpd)
        {
            // gmpd_col_L
            for (int i = 0; i < gmpd_col_L.childCount; i++)
            {
                all_rows.Add(gmpd_col_L.GetChild(i));
            }

            // gmpd_col_R
            for (int i = 0; i < gmpd_col_R.childCount; i++)
            {
                all_rows.Add(gmpd_col_R.GetChild(i));
            }
        }

        return all_rows;
    }

    // ROW LABEL SETTER
    public void SetTextOnRows(string row_label, string text, bool kb = true, bool gmpd = true)
    {
        List<Transform> found_rows = GetRows(row_label, kb, gmpd);
        if (log) { Debug.Log($"(FeedbackPoolBuilder) Setting text on rows with label {row_label} to {text} ({found_rows.Count} found)"); }
        for (int i = 0; i < found_rows.Count; i++)
        {
            TextMeshProUGUI label = found_rows[i].Find("text").GetComponent<TextMeshProUGUI>();
            label.text = text;
        }
    }
    public void ResetTextOnRows(string row_label, bool kb = true, bool gmpd = true)
    {
        List<Transform> found_rows = GetRows(row_label, kb, gmpd);
        for (int i = 0; i < found_rows.Count; i++)
        {
            TextMeshProUGUI label = found_rows[i].Find("text").GetComponent<TextMeshProUGUI>();
            label.text = row_label;
        }
    }
}