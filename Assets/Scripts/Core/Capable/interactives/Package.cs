using System.Collections;
using UnityEngine;

public class Package : Movable, Interactable, TurnableIntoItem
{
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Other;


    [Header("Turnable Into Item Settings")]
    public static ItemData PackageItemData = new ItemData()
    {
        reference = "leftover:package",
        max_qty = 16,
        item_description = "some rests of a package, maybe it can be useful ? i like the smell of cardboard boxes, it reminds me of my childhood",
    };
    public ItemData ItemDataInfo { get => PackageItemData; }
    public static DropParameters PackageDropParameters = new DropParameters()
    {
        random_direction = true,
        drop_magnitude = 0.01f,
        lock_magnitude = false,
        offset_drop = new Vector2(0f, 0.05f)
    };
    public DropParameters DropParameters { get => PackageDropParameters; }

    private bool is_turning_to_item = false;

    // ON INTERACT
    private float drop_animation_percentage = 0.4f; // the percentage of the animation duration at which we drop the items
    public void OnInteract(Capable interactor)
    {
        if (is_turning_to_item) { return; }
        (Visual as AnimPlayer)?.AddToPile("idle_open");
        StartCoroutine(turn_into_leftover());
    }
    private IEnumerator turn_into_leftover()
    {
        is_turning_to_item = true;

        // disable the hover & feet
        if (TryGetCapacity(out HoverCapacity hover)) { hover.gameObject.SetActive(false); }
        AddEffect(Effect.SemiGhost, timetolive:15f);

        // we play anim
        (Visual as AnimPlayer)?.Play("interact");
        float percentage = 0f;
        bool dropped_items = false;
        while ((Visual as AnimPlayer)?.IsShowing("interact") == true)
        {
            if (dropped_items) { yield return null; continue; }

            // we check the percentage
            percentage = (Visual as AnimPlayer)?.GetCurrentAnimPercentDone() ?? 0f;
            if (percentage >= drop_animation_percentage)
            {
                // we drop the items
                this.DropAllItems(parameters: DropParameters);
                dropped_items = true;
            }

            yield return null;
        }
        CapableEngine.Instance.TurnToItem(this, "package_leftover", "idle_open");
        is_turning_to_item = false;
    }
    private void cancel_coroutine()
    {
        StopAllCoroutines();
        is_turning_to_item = false;
    }

    // DATA LOADING
    public override void LoadData(CapableData data)
    {
        // we cancel the coroutine
        cancel_coroutine();

        // todo HERE WE SHOULD add some random items to the package if it's empty
        if (data.inventory != null && data.inventory.ItemsCount() == 0)
        {
            int random_eggs_to_add = Random.Range(1, 4);
            for (int i=0; i<random_eggs_to_add; i++)
            {
                ItemData egg = CapableEngine.Instance.DuplicateTemplate("spawn_egg") as ItemData;
                if (!data.inventory.AddItem(egg))
                {
                    Debug.LogError("(Package - LoadData) Failed to add egg to package inventory.");
                    // don't uncomment this, it will throw when we will rework the ItemPoolData add item with reference check, etc
                    // but if we comment this we will never see that it is broken, soooo leave it like this ehe :D
                    // means fewer future debug time !!!!!!!
                }
            }
        }


        base.LoadData(data);
    }
    public override void UnloadData()
    {
        // we cancel the coroutine
        cancel_coroutine();
        base.UnloadData();
    }
}