using TMPro;
using UnityEngine;

public class UI_WorldSlot : UI_EventButton, Descriptable
{
    private WorldData world_data;

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI name_text;

    public string Name => world_data != null ? world_data.id : "/!\\ no world data /!\\";

    public string Description => world_data != null ? "great world" : "";

    // INITIALIZATION & DESTRUCTION
    public void Initialize(WorldData world_data)
    {
        this.world_data = world_data;

        // register to the on click event
        OnClick += HandleOnClicked;

        // we set the icon sprite and color
        if (world_data == null) { return; }
        
        Sprite icon_sprite = WorldManager.Instance.GetIconSprite(world_data.icon_path, world_data.icon_name);
        if (icon_sprite != null) { btn_icon.sprite = icon_sprite; }
        btn_icon.color = world_data.color;
        this.baseColor = world_data.color;
        name_text.text = world_data.id;
    }
    private void OnDestroy()
    {
        // unregister from the on click event
        OnClick -= HandleOnClicked;
    }


    // CLICK HANDLER
    private void HandleOnClicked()
    {
        WorldManager.Instance.SelectWorld(world_data);
        SceneLoader.Instance.LoadGame();
    }

}