using UnityEngine;

/// <summary>
/// this class handles multiple writers FOR UI_SKILLS
/// </summary>
public class UI_SkillDescriptor : UI_Descriptor
{
    [Header("Skill status")]
    [SerializeField] private UI_Writer current_writer;
    [SerializeField] private UI_Writer next_writer;
    [SerializeField] private UI_Writer level_writer;

    // DESCRIPTION
    /* public override void Describe(Descriptable descriptable)
    {
        base.Describe(descriptable);

        // we get the skill manager
        if (Perso.Instance == null) { updateSkill(); return; }
        SkillManager skill_manager = Perso.Instance.skillManager;
        if (skill_manager == null) { updateSkill(); return; }

        // we get the skill
        UI_Skill ui_skill = descriptable as UI_Skill;
        if (ui_skill == null) { updateSkill(); return; }

        // we check if we have to updates upgrades
        updateSkill(ui_skill);
    }

    // UPGRADES MANAGEMENT
    protected void updateSkill(UI_Skill ui_skill = null)
    {
        // checks if skill is null it means we reset all
        if (ui_skill == null)
        {
            current_writer.Write("no data '-'");
            next_writer.Write("no data '-'");
            level_writer.Write("no data '-'");
            return;
        }

        // we get the skill manager
        SkillManager skill_manager = Perso.Instance.skillManager;

        // we update the writers

        // current
        string data = "current : " + skill_manager.GetSkillValue(ui_skill.Reference).ToString() + " " + ui_skill.unit;
        current_writer.Write(data);
        
        // next
        data = "next : " + skill_manager.GetNextLevelSkillValue(ui_skill.Reference).ToString() + " " + ui_skill.unit;
        next_writer.Write(data);

        // level
        data = "level " + skill_manager.GetSkillLevel(ui_skill.Reference).ToString();
        level_writer.Write(data);
    } */
}