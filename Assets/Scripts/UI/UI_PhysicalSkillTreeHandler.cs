using UnityEngine;
using TMPro;

public class UI_PhysicalSkillTreeHandler : MonoBehaviour {
        
        // perso
        private SkillTree tree;

        // uis
        private GameObject max_vie_lvl_label;
        private GameObject regen_vie_lvl_label;
        private GameObject degats_lvl_label;

        private GameObject points_to_spend_label;

        private GameObject perso_level_label;

        // unity functions
        void Start()
        {
            // on récupère le skill tree
            tree = transform.parent.GetComponent<SkillTree>();

            // on récupère les labels
            max_vie_lvl_label = transform.Find("max_vie/lvl_label").gameObject;
            regen_vie_lvl_label = transform.Find("regen_vie/lvl_label").gameObject;
            degats_lvl_label = transform.Find("degats/lvl_label").gameObject;

            points_to_spend_label = transform.Find("level/tuto/points_to_spend").gameObject;

            perso_level_label = transform.Find("level").gameObject;
        }

        void Update()
        {
            // on met à jour les labels
            max_vie_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.max_vie_level.ToString();
            regen_vie_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.regen_vie_level.ToString();
            degats_lvl_label.GetComponent<TextMeshProUGUI>().text = "level " + tree.degats_level.ToString();

            points_to_spend_label.GetComponent<TextMeshProUGUI>().text = tree.physical_points.ToString();

            perso_level_label.GetComponent<TextMeshProUGUI>().text = "LEVEL " + tree.transform.parent.GetComponent<Perso>().level.ToString();
        }       

}