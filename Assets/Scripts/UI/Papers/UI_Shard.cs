using System.Collections.Generic;
using UnityEngine;

public class UI_Shard : MonoBehaviour
{
    private string SHARD_ID = "no id";
    [SerializeField] private UI_Text shard_text;
    [SerializeField] private UI_Text full_version_text;


    private void Start()
    {
        if (SHARD_ID != "no id") { return; }
        if (shard_text == null) { Debug.LogWarning("(UI_Shard) UI_Text component for shard id is missing. please assign it in the inspector."); }
        if (full_version_text == null) { Debug.LogWarning("(UI_Shard) UI_Text component for full version is missing. please assign it in the inspector."); }

        // get the full version
        string full_version = AppManager.Instance.GetFullVersion(includePrototype: false);
        full_version_text.SetText(full_version);

        // get the version
        string version = Application.version;

        // generate the id
        SHARD_ID = generate_random_id_from_version(version);
        shard_text.SetText(SHARD_ID);
    }

    private string generate_random_id_from_version(string version)
    {
        string id = "";



        // we split the version by .
        List<string> parts = new List<string>(version.Split('.'));
        foreach (string part in parts)
        {
            if (part.Length < 1) { continue; }
            int nb = 0;
            string last_char = "";
            try { nb = int.Parse(part); }
            catch
            {
                last_char = part[part.Length - 1].ToString();

                // we remove the last char (h in 1.4.85h)
                string part_clean = part.Remove(part.Length - 1);
                try { nb = int.Parse(part_clean); }
                catch
                {
                    Debug.LogWarning("(UI_Shard) could not parse version part: " + part);
                    continue;
                }
            }

            // we generate x random character where x is nb
            for (int i = 0; i < nb; i++)
            {
                id += (char)Random.Range(97, 123);
            }

            // we add the last char if any
            id += last_char;

            // we add a separator
            id += ' ';
        }

        // we remove the last separator
        if (id.Length > 0) { id = id.Remove(id.Length - 1); }
        return id;
    }
}






        /* for (int i = 0; i < version.Length; i++)
        {
            char c = version[i];
            if (c == '.')
            {
                id += ' ';
            }
            else if (char.IsDigit(c))
            {
                id += (char)Random.Range(97, 123);
            }
            else
            {
                id += c;
            }
        }
        return id;
    }
} */