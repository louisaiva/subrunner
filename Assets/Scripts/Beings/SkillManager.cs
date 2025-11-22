using UnityEngine;

/// <summary>
/// 
/// gère les skills du perso
/// 
/// pour chaque skill on a 4 choses :
/// -un level(l)
/// - une base(b)
/// - un modifier(m)
/// - une stat de base constante (K)
/// 
/// soit x la stat actuelle du perso,
/// 
///             x = K + l * (b + l * m * K)
/// </summary>

public class SkillManager : MonoBehaviour
{

    [Header("stat:max_life")]
    [SerializeField] private int max_life_level = 0;
    [SerializeField] private float max_life_K = 100f;
    [SerializeField] private float max_life_base = 15f;
    [SerializeField] private float max_life_modifier = 0.1f;


    [Header("stat:regen_life")]
    [SerializeField] private int regen_life_level = 0;
    [SerializeField] private float regen_life_K = 0f;
    [SerializeField] private float regen_life_base = 0.15f;
    [SerializeField] private float regen_life_modifier = 0.1f;

    [Header("stat:damage")]
    [SerializeField] private int damage_level = 0;
    [SerializeField] private float damage_K = 10f;
    [SerializeField] private float damage_base = 3f;
    [SerializeField] private float damage_modifier = 0.1f;

    [Header("logs")]
    public bool log = false;


    /* // max bits (en bits)
    // exception : on fait par octet (lol)
    // x = K*(l+1)
    public int max_bits_level = 0;
    private float max_bits_K = 8f;

    // regen bits
    public int regen_bits_level = 0;
    private float regen_bits_base = 0.03f;
    private float regen_bits_modifier = 0.2f;
    private float regen_bits_K = 0.1f;

    // portee hack
    public int portee_hack_level = 0;
    private float portee_hack_base = 0.1f;
    private float portee_hack_modifier = 0.1f;
    private float portee_hack_K = 2f; */


    // START
    public void Start()
    {
        // on met à jour les valeurs du perso
        Perso.Instance.max_life = (int)calculateX("stat:max_life");
        Perso.Instance.regen_life = calculateX("stat:regen_life");
        // perso.GetCapacity<AttackCapacity>().damage = calculateX("damage");
        // perso.max_bits = (int) calculateX("max_bits");
        // perso.regen_bits = calculateX("regen_bits");
        // perso.setHackinRange(calculateX("portee_hack"));
    }

    // UPGRADING
    public void UpgradeSkill(string reference)
    {
        if (Perso.Instance == null) { Debug.LogWarning("(SkillManager) Perso instance is null, can't upgrade skill " + reference); return; }
        
        if (reference == "stat:max_life")
        {
            max_life_level++;
            Perso.Instance.max_life = (int)calculateX(reference);
            if (log) { Debug.Log("(SkillManager) skill " + reference + " upgraded to level " + max_life_level + " (new value: " + Perso.Instance.max_life + ")"); }
            return;
        }
        if (reference == "stat:regen_life")
        {
            regen_life_level++;
            Perso.Instance.regen_life = calculateX(reference);
            if (log) { Debug.Log("(SkillManager) skill " + reference + " upgraded to level " + regen_life_level + " (new value: " + Perso.Instance.regen_life + ")");}
            return;
        }
        if (reference == "stat:damage")
        {
            damage_level++;
            if (log) { Debug.Log("(SkillManager) skill " + reference + " upgraded to level " + damage_level + " (new value: " + calculateX(reference) + ")");}
            return;
        }
        if (log) { Debug.Log("(SkillManager) skill " + reference + " not found");}
    }
    private float calculateX(string skill, int deltaLevel=0)
    {

        float x = 0f;

        /* // cas spécial de max_bits
        if (skill == "max_bits")
        {
            return max_bits_K * (max_bits_level + 1);
        } */

        float l = 0f;
        float b = 0f;
        float m = 0f;
        float K = 0f;

        // on regarde quel skill on augmente
        switch (skill)
        {
            case "stat:max_life":
                l = max_life_level;
                b = max_life_base;
                m = max_life_modifier;
                K = max_life_K;
                break;
            case "stat:regen_life":
                l = regen_life_level;
                b = regen_life_base;
                m = regen_life_modifier;
                K = regen_life_K;
                break;
            case "stat:damage":
                l = damage_level;
                b = damage_base;
                m = damage_modifier;
                K = damage_K;
                break;
            /* case "stat:regen_bits":
                l = regen_bits_level;
                b = regen_bits_base;
                m = regen_bits_modifier;
                K = regen_bits_K;
                break;
            case "stat:portee_hack":
                l = portee_hack_level;
                b = portee_hack_base;
                m = portee_hack_modifier;
                K = portee_hack_K;
                break; */
            default:
                return 0f;
        }

        l += deltaLevel;

        // on calcule x
        x = K + l * (b + l * m * (K == 0f ? 1f : K));

        return x;

    }
    
    // GETTERS
    public float GetSkillLevel(string skill)
    {
        switch (skill)
        {
            case "stat:max_life":
                return max_life_level;
            case "stat:regen_life":
                return regen_life_level;
            case "stat:damage":
                return damage_level;
            /* case "stat:regen_bits":
                return regen_bits_level;
            case "stat:portee_hack":
                return portee_hack_level; */
            default:
                return 0f;
        }
    }
    public float GetSkillValue(string skill)
    {
        return (float) calculateX(skill);
    }
    public float GetNextLevelSkillValue(string skill)
    {
        // on regarde quel skill on augmente
        return (float) calculateX(skill,1);
    }
}