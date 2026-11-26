using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public sealed class KindContractResolver : DefaultContractResolver
{
    private static bool log = true;

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        if (member == null) { return null; }
        if (ShouldSkipMember(member, out FieldInfo backingField)) { return null; }
        JsonProperty prop;
        if (backingField != null)
        {
            if (log) { Debug.Log($"(KindCR) Creating property for backing field: {backingField.Name} of type {backingField.FieldType}"); }
            prop = base.CreateProperty(backingField, memberSerialization);

            // set the prop readable and writable based on the backing field's accessibility
            prop.Readable = true;
            prop.Writable = true;
        }
        else { prop = base.CreateProperty(member, memberSerialization); }

        if (log) { Debug.Log($"(KindCR) Created JsonProperty: Name={prop.PropertyName}, Type={prop.PropertyType} Ignored={prop.Ignored}, Readable={prop.Readable}, Writable={prop.Writable}"); }

        return prop;
    }
    private static bool ShouldSkipMember(MemberInfo member, out FieldInfo backingField)
    {
        backingField = null;
        if (member == null) { return true; }
        // handling property case
        if (member is PropertyInfo propertyInfo)
        {
            if (log) { Debug.Log($"(KindCR) Checking property: {propertyInfo.Name} of type {propertyInfo.PropertyType}"); }
            if (!AttributesAnalyzer.IsDataClassRelative(propertyInfo.DeclaringType)) { return true; } // we don't want Vector2.normalized

            // get the backing field
            string backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
            backingField = propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField == null) { return true; }

            // check if skip the backing field
            return ShouldSkipMember(backingField, out _);
        }

        if (member is not FieldInfo field)
        {
            if (log) { Debug.Log($"(KindCR) Member {member.Name} ({member.MemberType}) is not a property or field, will be skipped."); }
            return true;
        }

        if (!AttributesAnalyzer.IsDataClassRelative(field.DeclaringType)) { return false; } // we authorize all fields --> allow for storing Vector2.x & Vector2.y

        // handling field case
        if (!Attribute.IsDefined(field, typeof(KindDataAttribute), inherit: false))
        {
            if (log) { Debug.Log($"(KindCR) Field {field.Name} has no [KindData] attribute, will be skipped."); }
            return true;
        }
        return false;
    }
}
