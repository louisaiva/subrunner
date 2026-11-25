using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttributesAnalyzer : MonoBehaviour
{

    public int data_types = 0;
    public int broken_fields = 0;
    public int subclass_fields = 0;
    public int total_fields = 0;

    // ENTRY POINT
    public void AnalyzeAttributes()
    {
        // init variables
        string debug = "";
        data_types = 0;
        broken_fields = 0;
        total_fields = 0;
        subclass_fields = 0;

        // get all DataHolder types
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        Type[] types = currentAssembly.GetTypes();
        foreach (var type in types)
        {
            if (!is_data_holder(type)) { continue; }
            data_types++;
            debug += "\n]]-_-[[> ";

            string type_debug = "";
            int type_fields = analyze_type(type, ref type_debug, out int broken_count);
            debug += (type.Name + (broken_count > 0 ? $" : {broken_count} / {type_fields} broken fields" : $" : {type_fields} fields")).AddColor(broken_count > 0 ? Color.red : Color.green); ;
            debug += type_debug;
            total_fields += type_fields;
        }

        string broken = (broken_fields > 0 ? $"{broken_fields} / {total_fields} broken fields" : $"{total_fields} fields").AddColor(broken_fields > 0 ? Color.red : Color.green);
        string end_debug = $"(AttributesAnalyzer) Analyze found {data_types} HolderData types, {broken}   ({subclass_fields} subclass fields)\n" + debug;
        // Debug.Log(end_debug);

        // output_debug(end_debug);
        output_debug_type_by_type(end_debug, "]]-_-[[");
    }


    // MAIN METHODS
    private Type get_generic_type_of(Type type)
    {
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(List<>))
        {
            return type.GetGenericArguments()[0];
        }
        return type;
    }
    private bool is_data_holder(Type type)
    {
        type = get_generic_type_of(type);
        if (!typeof(DataHolder).IsAssignableFrom(type)) { return false; }
        if (!type.IsPublic) { return false; }
        if (type.IsInterface) { return false; }
        return true;
    }
    private int analyze_type(Type type, ref string debug, out int broken_in_type)
    {
        broken_in_type = 0;
        // get all fields + properties
        BindingFlags flags = BindingFlags.Public |  BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        List<FieldInfo> fields = new List<FieldInfo>();
        foreach (FieldInfo field in type.GetFields(flags)) { fields.Add(field); }
        foreach (PropertyInfo prop in type.GetProperties(flags))
        {
            string backingFieldName = $"<{prop.Name}>k__BackingField";
            FieldInfo backingField = type.GetField(backingFieldName, flags);
            if (backingField == null) { continue; }
            fields.Add(backingField);
        }

        // get all events to filter them
        List<EventInfo> events = type.GetEvents(flags).ToList();

        // analyze all fields, and add to debug which have attributes
        int fields_count = 0;
        foreach (FieldInfo field in fields)
        {
            if (events.Any(e => e.Name == field.Name)) { continue; } // filter events

            if (!analyze_field(field, ref debug)) { broken_in_type++; }
            fields_count++;
        }
        return fields_count;
    }
    /// <summary>
    /// returns true if the field is correct, false otherwise
    /// </summary>
    /// <param name="field"></param>
    /// <param name="debug"></param>
    /// <returns></returns>
    private bool analyze_field(FieldInfo field, ref string debug)
    {
        bool has_at_least_one_attribute = false;
        int attributes_count = 0;
        bool is_rto = Attribute.IsDefined(field, typeof(RuntimeOnlyAttribute));
        bool is_instance = Attribute.IsDefined(field, typeof(InstanceDataAttribute));
        bool is_kind = Attribute.IsDefined(field, typeof(KindDataAttribute));
                
        string field_debug = field.Name + " : ";
        if (is_rto) { attributes_count++; has_at_least_one_attribute = true; field_debug += " RuntimeOnly "; }
        if (is_instance) { attributes_count++; has_at_least_one_attribute = true; field_debug += " InstanceData "; }
        if (is_kind) { attributes_count++; has_at_least_one_attribute = true; field_debug += " KindData "; }

        if (!has_at_least_one_attribute)
        {
            debug += "\n" + (" [x] " + field_debug + " <--- HAS NO DATA ATTRIBUTE").Red();
            broken_fields++;
            return false;
        }
        if (attributes_count > 1 && is_rto)
        {
            debug += "\n" + (" [x] " + field_debug + " <--- RUNTIME ONLY MEMBERS CAN'T BE INSTANCE OR KIND DATA").Red();
            broken_fields++;
            return false;
        }
        if (attributes_count > 1 && !is_data_holder(field.FieldType)) // instancedata & kinddata & !DataHolder
        {
            debug += "\n" + (" [>] " + field_debug + " <--- SUBCLASS FIELDS NEED TO BE DATA HOLDER. Apply DataHolder interface on type : ").Red() + get_generic_type_of(field.FieldType).Name;
            broken_fields++;
            return false;
        }
        if (attributes_count > 1 || is_data_holder(field.FieldType)) // instancedata & kinddata & DataHolder | instancedata & DataHolder | kinddata & DataHolder
        {
            debug += "\n" + ("  >  " + field_debug).Yellow();
            subclass_fields++;
            return true;
        }

        debug += "\n" + "     " + field_debug;
        return true;
    }


    // OUTPUT METHOD
    /* private void output_debug(string end_debug)
    {
        int j; //variable that holds the slide content length
        int max_lines = 200; //desired maximum amount of lines per slide
        int max_characters = 10000; //desired maximum amount of characters
        char[] input = end_debug.ToCharArray(); //convert input string into an array of characters
        string slide_content; //variable that will hold the output of each single slide

        int counter = 0;
        int max_iterations = 100000;

        while (input.Length > 0 && counter < max_iterations)
        {
            counter++;

            // reset slide content and length
            slide_content = string.Empty;
            j = 0;

            // loop through the input string and get a 'j' amount of characters
            while ((slide_content.Split('\n').Length < max_lines) && (j < max_characters) && counter < max_iterations)
            {
                counter++;

                j = j + 1;
                slide_content = new string(input.Take(j).ToArray());
            }

            while (slide_content.Last() != '\n' && counter < max_iterations)
            {
                counter++;

                j = j + 1;
                slide_content = new string(input.Take(j).ToArray());
            }

            // Output slide content
            Debug.Log(slide_content);
            // Debug.Log("=================== END OF THE SLIDE =====================");

            // Remove the previous slide content from the input string
            input = input.Skip(j).ToArray();
        }
        // Debug.Log(new string(input.ToArray()));
    } */
    private void output_debug_type_by_type(string end_debug, string splitter)
    {
        List<string> debugs = end_debug.Split(splitter).ToList();
        foreach (string deb in debugs)
        {
            Debug.Log(deb);
        }
    }



    // EDITOR
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