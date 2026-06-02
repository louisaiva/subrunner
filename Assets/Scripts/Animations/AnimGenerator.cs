using System.Collections.Generic;
using System;
using UnityEngine;

public class AnimGenerator : MonoBehaviour
{
    [Header("Anims to choose name")]
    [SerializeField] private List<SimpleSpriteAnim> anims;
    
    [Header("Anims to burst spritesheets")]
    [SerializeField] private List<BurstAnim> burst_anims;
    
    [SerializeField] private bool log_extraction = false;



    #if UNITY_EDITOR
    public List<Anim> GenerateAnims(string spritesheets_path)
    {
        List<Anim> generated_anims = new List<Anim>();

        // SSA
        for (int i = 0; i < anims.Count; i++)
        {
            SimpleSpriteAnim ssa = anims[i];
            Anim anim = extractSimpleSpriteAnim(ssa, spritesheets_path);
            generated_anims.Add(anim);
            if (log_extraction) { Debug.Log("(AnimBank - GenerateAnims) Extracted anim : " + anim.name + " from SimpleSpriteAnim : " + ssa.name); }
        }


        // Burst anims
        for (int i = 0; i < burst_anims.Count; i++)
        {
            BurstAnim ba = burst_anims[i];
            int sprite_count = extractBurstAnim(ba, spritesheets_path, ref generated_anims);
            if (log_extraction) { Debug.Log("(AnimBank - GenerateAnims) Extracted " + sprite_count + " sprites from BurstAnim : " + ba.name); }
        }


        return generated_anims;
    }

    public int extractBurstAnim(BurstAnim ba, string spritesheets_path, ref List<Anim> generated_anims)
    {
        if (ba.spritesheet == null)
        {
            if (log_extraction) { Debug.LogWarning("(AnimBank - ExtractBurstAnim) SpriteSheet null on Anim : " + ba.name); }
            return 0;
        }

        UnityEngine.Object[] data = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(ba.spritesheet));
        if (data == null || data.Length == 0)
        {
            if (log_extraction) { Debug.LogWarning("(AnimBank - ExtractBurstAnim) No data found on SpriteSheet : " + ba.name); }
            return 0;
        }

        // we load all sprites
        int sprite_count = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] is Sprite)
            {
                Sprite sprite = (Sprite)data[i];
                string sprite_path = UnityEditor.AssetDatabase.GetAssetPath(sprite).Replace(".png", "").Replace("Assets/Resources/" + spritesheets_path, "") + "." + sprite.name;
                string anim_name = ba.name + "_" + i + ".idle.D";
                generated_anims.Add(new Anim(anim_name, new string[1] { sprite_path }, new float[1] { 0.1f }));
                sprite_count++;
            }
        }

        return sprite_count;
    }

    public Anim extractSimpleSpriteAnim(SimpleSpriteAnim ssa, string spritesheets_path)
    {
        string anim_name = ssa.name;
        if (!anim_name.Contains("."))
        {
            anim_name += ".idle.D";
        }

        // save the sprite path
        Sprite sprite = ssa.sprite;
        if (sprite == null)
        {
            if (log_extraction) { Debug.LogWarning("(AnimBank - ExtractSpriteCurve) Sprite null on Anim : " + anim_name); }
            return null;
        }

        string[] spritePaths = new string[1] { UnityEditor.AssetDatabase.GetAssetPath(sprite).Replace(".png", "").Replace("Assets/Resources/" + spritesheets_path, "") };
        spritePaths[0] += "." + sprite.name;
        return new Anim(anim_name, spritePaths, new float[1] { 0.1f });
    }
    #endif
}

[Serializable] public class SimpleSpriteAnim
{
    public string name; // skin.capacity.orientation OR skin
    public Sprite sprite;
}

[Serializable] public class BurstAnim
{
    public string name; // skin
    public Texture2D spritesheet;
}