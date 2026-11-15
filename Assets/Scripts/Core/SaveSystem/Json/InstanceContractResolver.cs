using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public sealed class InstanceContractResolver : DefaultContractResolver
{
    private static bool log = false;

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

        if (log) { Debug.Log($"(InstanceCR) Created JsonProperty: Name={prop.PropertyName}, Ignored={prop.Ignored}, Readable={prop.Readable}, Writable={prop.Writable}"); }

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
            if (!isDataClassRelative(propertyInfo.DeclaringType)) { return true; } // we don't want Vector2.normalized

            // Check if the backing field has the attribute (for [field: SerializeField])
            string backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
            backingField = propertyInfo.DeclaringType?.GetField(
                backingFieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // if the field has no attributes, we skip it by default
            if (backingField == null) { return true; }
            // List<Attribute> attrs = Attribute.GetCustomAttributes(backingField, inherit: false).ToList();
            // if (attrs.Count == 0) { return true; }
            // if (log) { Debug.Log($"(InstanceCR) Backing field {backingField.Name} for property {propertyInfo.Name} has {attrs.Count} attributes, we check if we should skip it"); }
            return ShouldSkipMember(backingField, out _); 
        }

        if (member is not FieldInfo field)
        {
            if (log) { Debug.Log($"(InstanceCR) Member {member.Name} ({member.MemberType}) is not a property or field, will be skipped."); }
            return true;
        }

        if (!isDataClassRelative(field.DeclaringType)) { return false; } // we authorize all fields --> allow for storing Vector2.x & Vector2.y

        // handling field case
        if (!Attribute.IsDefined(field, typeof(InstanceSpecificAttribute), inherit: false))
        {
            if (log) { Debug.Log($"(InstanceCR) Field {field.Name} has no Instance Specific attribute, will be skipped."); }
            return true;
        }
        return false;
    }
    private static bool isDataClassRelative(Type type)
    {
        if (typeof(CapableData).IsAssignableFrom(type)) { return true; }
        if (typeof(CapacityData).IsAssignableFrom(type)) { return true; }
        // inserts others here, like FeetData ?
        return false;
    }
}