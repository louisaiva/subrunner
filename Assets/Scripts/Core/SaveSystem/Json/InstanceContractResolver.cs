using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public sealed class InstanceContractResolver : DefaultContractResolver
{
    private static bool log = true;

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        if (member == null) { return null; }
        if (ShouldSkipMember(member, out FieldInfo backingField)) { return null; }
        JsonProperty prop;
        if (backingField != null)
        {
            if (log) { Debug.Log($"(InstanceCR) Creating property for backing field: {backingField.Name} of type {backingField.FieldType}"); }
            prop = base.CreateProperty(backingField, memberSerialization);

            // set the prop readable and writable based on the backing field's accessibility
            prop.Readable = true;
            prop.Writable = true;
        }
        else { prop = base.CreateProperty(member, memberSerialization); }

        if (log) { Debug.Log($"(InstanceCR) Created JsonProperty: Name={prop.PropertyName}, Type={prop.PropertyType} Ignored={prop.Ignored}, Readable={prop.Readable}, Writable={prop.Writable}"); }

        return prop;
    }
    private static bool ShouldSkipMember(MemberInfo member, out FieldInfo backingField)
    {
        backingField = null;
        if (member == null) { return true; }
        // handling property case
        if (member is PropertyInfo propertyInfo)
        {
            if (log) { Debug.Log($"(InstanceCR) Checking property: {propertyInfo.Name} of type {propertyInfo.PropertyType}"); }
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
            if (log) { Debug.Log($"(InstanceCR) Member {member.Name} ({member.MemberType}) is not a property or field, will be skipped."); }
            return true;
        }

        if (!AttributesAnalyzer.IsDataClassRelative(field.DeclaringType)) { return false; } // we authorize all fields --> allow for storing Vector2.x & Vector2.y

        // handling field case
        if (!Attribute.IsDefined(field, typeof(InstanceDataAttribute), inherit: false))
        {
            if (log) { Debug.Log($"(InstanceCR) Field {field.Name} has no Instance Specific attribute, will be skipped."); }
            return true;
        }
        return false;
    }
}


/// <summary>
/// this attribute "RuntimeOnly" will never be serialized, and
/// will be ignored when duplicating data
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RuntimeOnlyAttribute : Attribute { }

/// <summary>
/// this attribute "InstanceData" mark fields that need to be
/// saved dynamically for all instances of a same kind.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class InstanceDataAttribute : Attribute { }


/// <summary>
/// this attribute "KindData" mark fields which values are the same
/// for all instances of a same kind. These are not saved dynamically,
/// since their values are feed on their pre saved static values.
/// To modify a value, the kind template needs to be saved staticly.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class KindDataAttribute : Attribute { }
