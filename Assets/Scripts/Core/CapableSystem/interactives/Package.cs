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
        drop_magnitude = 0.1f,
        lock_magnitude = false,
        offset_drop = new Vector2(0f, 0.03f)
    };
    public DropParameters DropParameters { get => PackageDropParameters; }

    private bool is_turning_to_item = false;

    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        if (is_turning_to_item) { return; }
        AnimPlayer.AddToPile("idle_open");
        StartCoroutine(turn_into_leftover());
    }
    private IEnumerator turn_into_leftover()
    {
        is_turning_to_item = true;

        // disable the hover
        if (TryGetCapacity(out HoverCapacity hover)) { hover.gameObject.SetActive(false); }

        // we play anim
        AnimPlayer.Play("interact");
        while (AnimPlayer.IsPlaying("interact")) { yield return null; }
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
        base.LoadData(data);
    }
    public override void UnloadData()
    {
        // we cancel the coroutine
        cancel_coroutine();
        base.UnloadData();
    }
}