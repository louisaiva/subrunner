using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Orderer : Crafter
{


    ///
    //
    /// CRAFTER
    //
    ///
    public override string EmptyInventoryDesc => "ingredients will be shown here";
    public override string ItemRule => "food;!food:apple;!food:meat|ingredient";
    public override string UI_PoolName => "orderer";


    // MAIN METHODS
    public bool Order()
    {
        // we try to make a pasta meal from current inventory
        PastaMeal current_meal = new PastaMeal(Inventory);
        if (!current_meal.valid)
        {
            if (log) { Debug.LogWarning($"(Orderer) {ID} wanted to order but miss some required ingredients !!\n"+current_meal.GetDetails()); }
            return false;
        }
        if (SiblingPrinter == null) { Debug.LogError($"(Orderer) {ID} wanted to order but no sibling printer was found ://"); return false; }

        if (!SiblingPrinter.Print(current_meal)) { return false; } // if printer is printing, we can't print rn
        hide_ui_pool();
        return true;
    }


    // DESCRIPTION
    public string GetCurrentMealDescription()
    {
        // we try to make a pasta meal from current inventory
        PastaMeal current_meal = new PastaMeal(Inventory);
        return current_meal.GetDetails();
    }


    // SIBLING Printer
    private Printer3D printer = null;
    public Printer3D SiblingPrinter
    {
        get
        {
            if (printer != null) { return printer; }

            // we do a circle cast to find the closest TV
            printer = find_sibling_printer();
            return printer;
        }
    }
    private Printer3D find_sibling_printer()
    {
        float radius = 5f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Interactives"));
        List<Printer3D> printers = new List<Printer3D>();
        foreach (Collider2D collider in colliders)
        {
            if (collider.GetComponentInParent<Printer3D>(includeInactive: true) is not Printer3D printer) { continue; }
            printers.Add(printer);
        }
        if (printers.Count == 0) { return null; }

        // return closest one
        Printer3D closest_pr = null;
        float closest_distance = float.MaxValue;
        foreach (Printer3D pr in printers)
        {
            float distance = Vector2.Distance(transform.position, pr.transform.position);
            if (distance > closest_distance) { continue; }
            closest_distance = distance;
            closest_pr = pr;
        }
        return closest_pr;
    }

    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);
        printer = null;
    }
}

public class PastaMeal
{
    public bool valid = false;

    // required ingredients
    public Pasta pasta = null;
    public Item plate = null;
    public Item fork = null;
    public Food sauce = null;

    // optional ingredients
    public List<Item> options = new List<Item>();
    private static List<string> options_ref = new List<string>() {
        "food:lentils",
        "food:beans",
        "food:onion",
    };

    public PastaMeal(Inventory inventory)
    {
        valid = true;

        // pastas
        pasta = inventory.GetItem<Pasta>();
        if (pasta == null) { valid = false; }

        // plate
        List<Item> plates = inventory.GetItemsByRule("ingredient:plate_empty");
        if (plates.Count == 0) { valid = false; }
        else { plate = plates[0]; }

        // fork
        List<Item> forks = inventory.GetItemsByRule("ingredient:fork");
        if (forks.Count == 0) { valid = false; }
        else { fork = forks[0]; }

        // sauce
        List<Food> sauces = inventory.GetItemsByRule("food:tomato_sauce").ConvertAll(i => i as Food);
        if (sauces.Count == 0) { valid = false; }
        else { sauce = sauces[0]; }

        // options
        foreach (string reference in options_ref)
        {
            List<Item> opts = inventory.GetItemsByRule(reference);
            if (opts.Count == 0) { continue; }
            options.Add(opts[0]);
        }
    }

    public string GetDetails()
    {
        string details = "";
        // details += "PASTA MEAL : " + (valid ? "VALID".Green() : "INVALID".Red()) + "\n\n";
        details += "dry pastas - " + (pasta == null ? "MISSING".Red() : "  READY".Green()) + " \n";
        details += "empty plate - " + (plate == null ? "MISSING".Red() : "  READY".Green()) + " \n";
        details += "fork - " + (fork == null ? "MISSING".Red() : "  READY".Green()) + " \n";
        details += "tomato sauce - " + (sauce == null ? "MISSING".Red() : "  READY".Green()) + " \n";
        details += "\n";

        if (options.Count == 0) { details += "NO OPTIONS ".Grey(); return details; }

        details += $"{options.Count} OPTIONS :   \n\n";
        foreach (Item item in options)
        {
            details += item.SuffixReference + "\n";
        }

        return details;
    }

    public List<Item> GetItems()
    {
        List<Item> all_items = new List<Item>();
        if (pasta != null) { all_items.Add(pasta); }
        if (plate != null) { all_items.Add(plate); }
        if (fork != null) { all_items.Add(fork); }
        if (sauce != null) { all_items.Add(sauce); }
        all_items.AddRange(options);
        return all_items;
    }
}