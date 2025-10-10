using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;


/*
 * By Moreno Lovato
 * RuneHeads srl
 * for the Project Journey to the Void
 * 2025
 */
public static class ProjectStats
{

    [MenuItem("Tools/Project Stats")]
    public static void AnalyzeProject()
    {
        string[] allFiles = Directory.GetFiles(UnityEngine.Application.dataPath, "*.*", SearchOption.AllDirectories);

        // Count lines and classes only in .cs files
        string[] csFiles = allFiles.Where(f => f.EndsWith(".cs")).ToArray();
        int totalLines = 0;
        int totalClasses = 0;

        Regex classRegex = new Regex(@"\bclass\s+\w+");

        // store a dictionary with file name & line count to store 10 files with highest line count (danger zone > 500 lines)
        Dictionary<string, int> fileLineCounts = new Dictionary<string, int>();
        // fileLineCounts.Clear();


        foreach (string file in csFiles)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            add_file_if_necessary(file, lines.Length);

            // count non-empty and non-comment lines
            totalLines += lines.Count(l =>
                !string.IsNullOrWhiteSpace(l) &&
                !l.TrimStart().StartsWith("//")
            );

            // count classes
            totalClasses += lines.Count(l => classRegex.IsMatch(l));
        }

        // Convert lines to book pages (assuming ~35 lines per page)
        int approxPages = Mathf.CeilToInt(totalLines / 35f);

        // File type definitions
        Dictionary<string, string[]> fileTypes = new Dictionary<string, string[]>()
        {
            { "C# Scripts", new[] { ".cs" } },
            { "Shaders (classic)", new[] { ".shader", ".hlsl", ".cginc" } },
            { "Shader Graphs", new[] { ".shadergraph", ".subgraph" } },
            { "Prefabs", new[] { ".prefab" } },
            { "Images", new[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" } },
            { "Materials", new[] { ".mat" } },
            { "Scenes", new[] { ".unity" } },
            { "Animations", new[] { ".anim" } },
            { "Model 3D", new[] { ".fbx" } },
        };

        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (var kvp in fileTypes)
        {
            int count = allFiles.Count(f => kvp.Value.Contains(Path.GetExtension(f).ToLower()));
            counts[kvp.Key] = count;
        }

        // Print results
        string result = $"📊 Project Statistics:\n";
        result += $"📦 C# Files: {csFiles.Length}\n";
        result += $"   ➤ Classes: {totalClasses}\n";
        result += $"   ➤ Lines of Code: {totalLines}\n";
        result += $"   ➤ Equivalent Book Length: ~{approxPages} pages\n\n";

        // prints 10 highest lines count c# files
        result += "📝 Top 10 Largest C# Files (> 500 lines means DANGER ZONE):\n";
        foreach (var kvp in fileLineCounts.OrderByDescending(kvp => kvp.Value))
        {
            string fileName = Path.GetFileName(kvp.Key);
            int lineCount = kvp.Value;
            result += $"   ➤ {fileName}: {lineCount} lines\n";
        }


        /* foreach (var kvp in counts)
        {
            result += $"{kvp.Key}: {kvp.Value}\n";
        } */

        UnityEngine.Debug.Log(result);



        void add_file_if_necessary(string file, int lineCount)
        {
            // checks if we are in the Assets\Scripts folder otherwise return
            if (!file.Contains(Path.Combine("Assets", "Scripts"))) { return; }

            // if we have less than 10 files, we add it
            if (fileLineCounts.Count < 10)
            {
                fileLineCounts[file] = lineCount;
                return;
            }

            // otherwise we check if the new file has more lines than the minimum
            string minFile = fileLineCounts.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
            int minLines = fileLineCounts[minFile];

            if (lineCount > minLines)
            {
                fileLineCounts.Remove(minFile);
                fileLineCounts[file] = lineCount;
            }
        }
    }

}