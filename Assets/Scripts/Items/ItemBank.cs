using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemBank : MonoBehaviour
{ 

    public Dictionary<string, Sprite> item_sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, string> item_prefabs = new Dictionary<string, string>();

    // constructor
    public ItemBank()
    {

        // on ajoute les prefabs des items
        addItem("dirty_water", "prefabs/items/dirty_water");
        addItem("clean_water", "prefabs/items/clean_water");
        addItem("orange_juice", "prefabs/items/orange_juice");
        addItem("usb_key", "prefabs/items/usb_key");
        addItem("gv_glasses", "prefabs/items/legendary/gv_glasses");
        addItem("speed_glasses", "prefabs/items/legendary/speed_glasses");
        addItem("3d_glasses", "prefabs/items/legendary/3d_glasses");
        addItem("dynamite", "prefabs/items/dynamite");
        addItem("recorder", "prefabs/items/legendary/recorder");
        addItem("helium_shoes", "prefabs/items/legendary/helium_shoes");
        addItem("carbon_shoes", "prefabs/items/legendary/carbon_shoes");
        addItem("orangina", "prefabs/items/orangina");
        addItem("nood_os", "prefabs/items/legendary/nood_os");
        addItem("gyroscope", "prefabs/items/legendary/gyroscope");
        addItem("door_hack", "prefabs/items/door_hack");
        addItem("computer_hack", "prefabs/items/computer_hack");
        addItem("subway_hack", "prefabs/items/subway_hack");
        addItem("firewall_hack", "prefabs/items/firewall_hack");
        addItem("zombo_damage", "prefabs/items/zombo_damage");
        addItem("zombo_explosion", "prefabs/items/zombo_explosion");
        addItem("zombo_electrochoc", "prefabs/items/zombo_electrochoc");
        // addItem("zombo_control", "prefabs/items/zombo_control");
        addItem("slow_damage", "prefabs/items/slow_damage");
        addItem("light_hack", "prefabs/items/light_hack");
        addItem("tv_hack", "prefabs/items/tv_hack");
        addItem("katana", "prefabs/items/legendary/katana");
        addItem("cd", "prefabs/items/cd");
        addItem("badge", "prefabs/items/badge");

    }

    private void addItem(string item_name, string prefab_name)
    {
        if (System.IO.File.Exists("Assets/Resources/" + prefab_name + ".prefab"))
        {
            item_prefabs.Add(item_name, prefab_name);
        }
        else
        {
            // Debug.LogWarning("(ItemBank) cannot add item " + item_name);
        }
        // item_prefabs.Add(item_name, prefab_name);
    }

    public void init(Sprite[] sprites)
    {
        // on ajoute les sprites des items
        item_sprites.Add("dirty_water", sprites[0]);
        item_sprites.Add("clean_water", sprites[1]);
        item_sprites.Add("orange_juice", sprites[2]);
        item_sprites.Add("usb_key", sprites[3]);
        item_sprites.Add("gv_glasses", sprites[4]);
        item_sprites.Add("speed_glasses", sprites[5]);
        item_sprites.Add("3d_glasses", sprites[6]);
        item_sprites.Add("dynamite", sprites[7]);
        item_sprites.Add("recorder", sprites[8]);
        item_sprites.Add("helium_shoes", sprites[9]);
        item_sprites.Add("carbon_shoes", sprites[10]);
        item_sprites.Add("orangina", sprites[11]);
        item_sprites.Add("NOOD_OS", sprites[12]);
        item_sprites.Add("gyroscope", sprites[13]);
        item_sprites.Add("door_hack", sprites[14]);
        item_sprites.Add("computer_hack", sprites[15]);
        item_sprites.Add("subway_hack", sprites[16]);
        item_sprites.Add("firewall_hack", sprites[17]);
        item_sprites.Add("zombo_damage", sprites[18]);
        item_sprites.Add("zombo_explosion", sprites[19]);
        item_sprites.Add("zombo_electrochoc", sprites[20]);
        item_sprites.Add("zombo_control", sprites[21]);
        item_sprites.Add("slow_damage", sprites[21]);
        item_sprites.Add("light_hack", sprites[22]);
        item_sprites.Add("tv_hack", sprites[23]);
        item_sprites.Add("katana", sprites[24]);
        item_sprites.Add("cd", sprites[25]);
        item_sprites.Add("badge", sprites[26]);
    }


    // GETTERS
    public Sprite getSprite(string item_name)
    {
        if (!item_sprites.ContainsKey(item_name))
        {
            Debug.LogWarning("(ItemBank) cannot find sprite " + item_name);
            return null;
        }

        return item_sprites[item_name];
    }

    public Item getRandomItem(string cat="all",bool use_leg=false)
    {
        List<string> prefabs = new List<string>();

        // on les trie par catégorie
        if (cat != "all")
        {
            // on récupère les prefabs de la catégorie
            prefabs = item_prefabs.Where(x => cat.Contains(getCategory(x.Key))).Select(x => x.Value).ToList();
        }
        else
        {
            prefabs = item_prefabs.Select(x => x.Value).ToList();
        }

        // on enlève les items légendaires si on ne veut pas les utiliser
        if (!use_leg)
        {
            prefabs = prefabs.Where(x => !x.Contains("legendary")).ToList();
        }

        // on choisit un prefab random
        int nb = Random.Range(0, prefabs.Count);
        GameObject item = Resources.Load(prefabs[nb]) as GameObject;

        if (item == null || item.GetComponent<Item>() == null)
        {
            Debug.LogWarning("(ItemBank) cannot find item component in " + prefabs[nb]);
            return null;
        }

        return item.GetComponent<Item>();
    }

    private string getCategory(string item_name)
    {
        if (!item_prefabs.ContainsKey(item_name))
        {
            Debug.LogWarning("(ItemBank) cannot find category " + item_name);
            return null;
        }
        
        // on instancie l'item
        GameObject item = Resources.Load(item_prefabs[item_name]) as GameObject;
        return item.GetComponent<Item>().category;
    }

    public Item createItem(string item_name)
    {
        if (!item_prefabs.ContainsKey(item_name))
        {
            Debug.LogWarning("(ItemBank) cannot find prefab " + item_name);
            return null;
        }

        // on instancie l'item
        // GameObject item = Resources.Load(item_prefabs[item_name]) as GameObject;
        GameObject item = Instantiate(Resources.Load(item_prefabs[item_name])) as GameObject;
        return item.GetComponent<Item>();
    }
}