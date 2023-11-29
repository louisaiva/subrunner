using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {


    // items
    public int max_items = 9;
    public bool scalable = false;

    // prefabs
    public string prefabs_path = "prefabs/items/";

    // perso
    public GameObject perso;

    // ui
    public Canvas canvas;
    // public GameObject inv_bg;
    public bool is_showed = false;
    public Vector2 inv_offset = new Vector2(0.5f, 0.5f);
    List<GameObject> empty_slots = new List<GameObject>();
    GameObject empty_slot_prefab;

    // unity functions
    void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le prefab
        empty_slot_prefab = Resources.Load("prefabs/ui/empty_slot") as GameObject;

        // on récupère le canvas
        canvas = GetComponent<Canvas>();

        // on récupère le bg
        // inv_bg = transform.Find("bg").gameObject;

        // on met à jour la taille du canvas
        updateSize();

        // on met à jour l'offset de l'inventaire en fonction de l'offset du canvas
        updateOffset();

    }

    void Update()
    {
        // on met à jour la taille du canvas si c'est scalable
        if (scalable) { updateSize(); }

        // on met à jour l'affichage
        updateShow();

        // on met à jour les positions des items
        updateUI();

        // on check les events
        // Events();
    }

    // UI
    void updateUI()
    {
        // on récupère les items
        List<Item> items = getItems();

        int w = (int) GetComponent<RectTransform>().sizeDelta.x;

        // on met à jour les positions des items
        for (int i = 0; i < items.Count; i++)
        {
            // on récupère l'item
            Item item = items[i];

            // on récupère la position de l'item
            Vector2 pos = new Vector2(0, 0);
            pos.x = inv_offset.x + (i % w);
            pos.y = inv_offset.y + (i / w);

            // on met à jour la position de l'item
            item.transform.localPosition = pos;
        }

        // si on est pas scalable, on ajoute/supprime des items vides pour remplir l'inventaire
        if (!scalable && items.Count + empty_slots.Count < max_items)
        {
            int nb_empty_slots_a_creer = max_items - items.Count - empty_slots.Count;

            // on ajoute des empty slots
            for (int i = 0; i < nb_empty_slots_a_creer; i++)
            {
                // on crée des empty slots
                GameObject empty_slot = Instantiate(empty_slot_prefab, transform.position, Quaternion.identity) as GameObject;
                empty_slot.transform.SetParent(transform);

                // on règle la scale à 1
                empty_slot.transform.localScale = new Vector3(1, 1, 1);

                // on ajoute l'empty slot à la liste
                empty_slots.Add(empty_slot);
            }
        }
        else if (!scalable && items.Count + empty_slots.Count > max_items)
        {
            // on supprime les empty slots en trop
            int nb_empty_slots_a_supprimer = items.Count + empty_slots.Count - max_items;

            // on supprime les empty slots
            for (int i = 0; i < nb_empty_slots_a_supprimer; i++)
            {
                // on récupère le dernier empty slot
                GameObject empty_slot = empty_slots[empty_slots.Count - 1];

                // on le supprime
                Destroy(empty_slot);

                // on le supprime de la liste
                empty_slots.Remove(empty_slot);
            }
        }

        // on met à jour les positions des empty slots
        for (int i = 0; i < empty_slots.Count; i++)
        {
            // on récupère l'empty slot
            GameObject empty_slot = empty_slots[i];

            int pos_dans_linv = items.Count + i;

            // on récupère la position de l'empty slot
            Vector2 pos = new Vector2(0, 0);
            pos.x = inv_offset.x + (pos_dans_linv % w);
            pos.y = inv_offset.y + (pos_dans_linv / w);

            // on met à jour la position de l'empty slot
            empty_slot.transform.localPosition = pos;
        }
    }

    void updateShow()
    {
        // on met à jour l'affichage du canvas
        // canvas.SetActive(is_showed);
        canvas.enabled = is_showed;

        // on régule l'affichage des items
        foreach (Transform child in transform)
        {
            // on regarde si c'est un item
            if (child.GetComponent<Item>() == null) { continue; }

            // on récupère l'item
            Item item = child.GetComponent<Item>();

            // on met à jour l'affichage
            item.changeShow(is_showed);
        }

        // on met à jour les empty slots
        foreach (GameObject empty_slot in empty_slots)
        {
            // on met à jour l'affichage
            empty_slot.SetActive(is_showed);
        }
    }

    void updateOffset()
    {
        // on met à jour l'offset de l'inventaire en fonction de l'offset du canvas
        if (GetComponent<RectTransform>().pivot.x == 0)
        {
            inv_offset.x = 0.5f;
        }
        else if (GetComponent<RectTransform>().pivot.x == 1)
        {
            inv_offset.x = -GetComponent<RectTransform>().sizeDelta.x + 0.5f;
        }
        else if (GetComponent<RectTransform>().pivot.x == 0.5f)
        {
            inv_offset.x = -GetComponent<RectTransform>().sizeDelta.x / 2 + 0.5f;
        }

        // on met a jour l'offset du box collider
        
    }

    /* void updateSize()
    {

        int size = max_items;
        if (scalable) { size = getItems().Count; }

        // on veut pouvoir afficher "size" items de manière carrée
        // on veut que l'inventaire soit un rectangle pouvant contenir EXACTEMENT "size" items
        // on veut que l'inventaire soit le plus petit possible

        // on vérifie qu'on a pas déjà la bonne taille
        if (GetComponent<RectTransform>().sizeDelta.x * GetComponent<RectTransform>().sizeDelta.y == size) { return; }


        // on calcule la taille de l'inventaire
        int width = 0;
        int height = 0;

        // on cherche le plus grand diviseur de "max_items"
        // qui donne la plus petite diagonale
        int min_diag = 5000;
        int max_div = 1;
        for (int i = 1; i <= size / 2; i++)
        {
            int div = size / i;

            if (size % i == 0)
            {
                // on calcule la diagonale
                int w = div;
                int h = size / div;
                int diag = (int)Mathf.Sqrt(w * w + h * h);
                if (diag < min_diag)
                {
                    min_diag = diag;
                    max_div = div;
                }
            }
        }

        // on calcule la taille de l'inventaire
        if (max_div > 1)
        {
            width = max_div;
            height = size / max_div;
        }
        else
        {
            width = size;
            height = 1;
        }

        // on met à jour la taille de l'inventaire
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

        // on met à jour la taille du box collider
        GetComponent<BoxCollider2D>().size = new Vector2(width, height);

        // on met à jour l'offset du box collider
        float x_offset = (GetComponent<RectTransform>().pivot.x - 0.5f) * width * -1;
        GetComponent<BoxCollider2D>().offset = new Vector2(x_offset, height / 2f);


        // on met à jour l'offset de l'inventaire en fonction de l'offset du canvas
        updateOffset();
    } */

    void updateSize()
    {

        int size = max_items;
        if (scalable) { size = getItems().Count; }

        // nouvelle version on veut afficher "size" items avec l'inventaire le plus petit possible
        // pour ça on veut trouver le nb le plus petit "n" tel que "n*n" >= "size"

        // on vérifie qu'on a pas déjà la bonne taille
        if (GetComponent<RectTransform>().sizeDelta.x * GetComponent<RectTransform>().sizeDelta.y >= size) { return; }


        // on calcule la taille de l'inventaire
        int width = 0;
        int height = 0;

        if (size == 3)
        {
            // cas particulier pour size == 3
            width = 3;
            height = 1;
        }
        else
        {
            // on cherche le plus petit carré >= size
            for (int i = 1; i <= size; i++)
            {
                if (i * i >= size)
                {
                    width = i;
                    height = i;
                    break;
                }
            }
        }

        // on met à jour la taille de l'inventaire
        GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

        // on met à jour la taille du box collider
        GetComponent<BoxCollider2D>().size = new Vector2(width, height);

        // on met à jour l'offset du box collider
        float x_offset = (GetComponent<RectTransform>().pivot.x - 0.5f) * width * -1;
        GetComponent<BoxCollider2D>().offset = new Vector2(x_offset, height / 2f);


        // on met à jour l'offset de l'inventaire en fonction de l'offset du canvas
        updateOffset();
    }

    public void rollShow()
    {
        // on met à jour l'affichage
        is_showed = !is_showed;
        updateShow();
    }

    public void setShow(bool is_showed)
    {
        // on met à jour l'affichage
        this.is_showed = is_showed;
        updateShow();
    }

    // functions
    public void dropItem(Item item)
    {
        // on récupère le gameobject de l'item
        GameObject item_go = item.gameObject;

        // on regarde si l'item est dans l'inventaire
        // if (!item.transform.parent == this.transform) { return; }
        // print("on essaye de drop " + item.item_name + " de " + item.transform.parent.name);

        // on vérifie si notre inventaire est un inventaire de perso
        if (transform.parent == perso.transform)
        {
            // on drop l'item via le perso
            perso.GetComponent<Perso>().drop(item);
        }
        else
        {
            // si on est un coffre, on drop l'item dans le perso
            perso.GetComponent<Perso>().grab(item);
        }
    }

    public void randomize()
    {
        // on randomize les items de l'inventaire
        // si on en a déjà, on les conserve
        List<Item> items = getItems();
        
        // le nombre d'items après la randomization
        int nb_items = Random.Range(items.Count, max_items + 1);

        // on récupère les prefabs
        Object[] prefabs = Resources.LoadAll(prefabs_path);
        // print(prefabs + " " + prefabs.Length);

        // on ajoute des items random
        for (int i = items.Count; i < nb_items; i++)
        {
            // on choisit un prefab random
            int nb = Random.Range(0, prefabs.Length);

            // on crée un item random
            GameObject item_go = Instantiate(prefabs[nb], transform.position, Quaternion.identity) as GameObject;

            // on ajoute l'item
            addItem(item_go.GetComponent<Item>());
        }
    }

    public bool addItem(Item item)
    {
        // on ajoute un item à l'inventaire
        // on vérifie qu'on est pas déjà plein
        if (!scalable && getItems().Count >= max_items) { return false; }

        // on ajoute l'item
        item.transform.SetParent(transform);

        // on règle la scale à 1
        item.transform.localScale = new Vector3(1, 1, 1);

        return true;
    }

    // getters
    public List<Item> getItems()
    {
        // on récupère les items
        List<Item> items = new List<Item>();
        foreach (Transform child in transform)
        {
            // on regarde si c'est un item
            if (child.GetComponent<Item>() == null) { continue; }

            // on ajoute l'item
            items.Add(child.GetComponent<Item>());
        }
        return items;
    }

    public Item getItem(string item_name)
    {
        // on cherche l'item
        foreach (Transform child in transform)
        {
            // on regarde si c'est un item
            if (child.GetComponent<Item>() == null) { continue; }

            // on regarde si c'est le bon item
            if (child.gameObject.GetComponent<Item>().item_name == item_name)
            {
                // on retourne l'item
                return child.gameObject.GetComponent<Item>();
            }
        }
        return null;
    }

    public List<Hack> getHacks()
    {
        // on récupère les hacks
        List<Hack> hacks = getItems().OfType<Hack>().ToList();
        return hacks;
    }
}