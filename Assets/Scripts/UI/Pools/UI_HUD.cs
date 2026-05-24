using UnityEngine;

public class UI_HUD : UI_Pool
{
    public UI_Notifier Notifier;

    private GameObject _life_bar;
    public GameObject LifeBar
    {
        get
        {
            if (_life_bar == null) { _life_bar = transform.Find("life_bar").gameObject; }
            return _life_bar;
        }
    }
    private GameObject _items_bar;
    public GameObject ItemsBar
    {
        get
        {
            if (_items_bar == null) { _items_bar = transform.Find("shortcuts_if").gameObject; }
            return _items_bar;
        }
    }
}