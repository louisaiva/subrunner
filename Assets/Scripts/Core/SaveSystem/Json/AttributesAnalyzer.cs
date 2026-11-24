using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttributesAnalyzer : MonoBehaviour
{

    public int data_types = 0;
    public int broken_members = 0;
    public int subclass_members = 0;
    public int total_members = 0;

    public void AnalyzeAttributes()
    {
        // init variables
        string debug = "";
        data_types = 0;
        broken_members = 0;
        total_members = 0;
        subclass_members = 0;

        // get all DataHolder types
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        Type[] types = currentAssembly.GetTypes();
        foreach (var type in types)
        {
            if (!is_data_holder(type)) { continue; }
            data_types++;
            debug += "\n" + type.Name + " :";
            total_members += analyze_type(type, ref debug);
        }

        string broken = $"{broken_members} / {total_members} members";
        if (broken_members > 0) { broken.Red(); }
        string end_debug = $"(AttributesAnalyzer) Analyze found {data_types} HolderData types, {broken}   ({subclass_members} subclass members)\n" + debug;
        // Debug.Log(end_debug);

        output_debug(end_debug);
    }


    private bool is_data_holder(Type type)
    {
        if (!typeof(DataHolder).IsAssignableFrom(type)) { return false; }
        if (!type.IsPublic) { return false; }
        if (type.IsInterface) { return false; }
        return true;
    }

    private int analyze_type(Type type, ref string debug)
    {
        // get all fields + properties
        List<MemberInfo> members = new List<MemberInfo>();
        foreach (FieldInfo field in type.GetFields()) { members.Add(field); }
        foreach (PropertyInfo prop in type.GetProperties()) { members.Add(prop); }

        // analyze all members, and add to debug which have attributes
        foreach (MemberInfo member in members)
        {
            analyze_member(member, ref debug);
        }
        return members.Count;
    }
    private void analyze_member(MemberInfo member, ref string debug)
    {
        bool has_at_least_one_attribute = false;
        int attributes_count = 0;
        bool is_read_only = Attribute.IsDefined(member, typeof(ReadOnlyAttribute));
        bool is_instance = Attribute.IsDefined(member, typeof(InstanceDataAttribute));
        bool is_kind = Attribute.IsDefined(member, typeof(KindDataAttribute));
                
        string member_debug = member.Name + " : ";
        if (is_read_only) { attributes_count++; has_at_least_one_attribute = true; member_debug += " ReadOnly "; }
        if (is_instance) { attributes_count++; has_at_least_one_attribute = true; member_debug += " InstanceData "; }
        if (is_kind) { attributes_count++; has_at_least_one_attribute = true; member_debug += " KindData "; }

        if (!has_at_least_one_attribute)
        {
            debug += "\n" + (" [x] " + member_debug + " <--- HAS NO DATA ATTRIBUTE").Red();
            broken_members++;
            return;
        }
        if (attributes_count > 1 && is_read_only)
        {
            debug += "\n" + (" [x] " + member_debug + " <--- READONLY MEMBERS CAN'T BE INSTANCE OR KIND DATA").Red();
            broken_members++;
            return;
        }
        if (attributes_count > 1) // instancedata & kinddata
        {
            debug += "\n" + (" [o] " + member_debug).Yellow();
            subclass_members++;
            return;
        }

        debug += "\n" + "     " + member_debug;
    }


    private void output_debug(string end_debug)
    {
        int j; //variable that holds the slide content length
        int max_lines = 200; //desired maximum amount of lines per slide
        // int max_characters = 10000; //desired maximum amount of characters
        char[] input = end_debug.ToCharArray(); //convert input string into an array of characters
        string slide_content; //variable that will hold the output of each single slide

        while (end_debug.Split('\n').Length > max_lines)
        {
            // reset slide content and length
            slide_content = string.Empty;
            j = 0;

            // loop through the input string and get a 'j' amount of characters
            while ((slide_content.Split('\n').Length < max_lines) /* && (j < max_characters) */)
            {
                j = j + 1;
                slide_content = new string(input.Take(j).ToArray());
            }

            // Output slide content
            Debug.Log(slide_content);
            // Debug.Log("=================== END OF THE SLIDE =====================");

            // Remove the previous slide content from the input string
            input = input.Skip(j).ToArray();
        }
        Debug.Log(new string(input.ToArray()));
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AttributesAnalyzer))]
    public class AttributesAnalyzerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AttributesAnalyzer analyzer = (AttributesAnalyzer)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Analyze Attributes"))
            {
                analyzer.AnalyzeAttributes();
            }
        }
    }
    #endif
}