using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public sealed class UnityValueTypeContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        if (member == null) { return null; }
        if (ShouldSkipMember(member, out FieldInfo backingField)) { return null; }
        JsonProperty prop;
        if (backingField != null)
        {
            // Debug.Log($"Creating property for backing field: {backingField.Name} of type {backingField.FieldType}");
            prop = base.CreateProperty(backingField, memberSerialization);

            // set the prop readable and writable based on the backing field's accessibility
            prop.Readable = true;
            prop.Writable = true;
        }
        else { prop = base.CreateProperty(member, memberSerialization); }

        Debug.Log($"Created JsonProperty: Name={prop.PropertyName}, Ignored={prop.Ignored}, Readable={prop.Readable}, Writable={prop.Writable}");

        return prop;
    }
    private static bool ShouldSkipMember(MemberInfo member, out FieldInfo backingField)
    {
        backingField = null;
        if (member == null) { return true; }

        if (member is PropertyInfo propertyInfo)
        {
            // Debug.Log($"Checking property: {propertyInfo.Name} of type {propertyInfo.PropertyType}");

            // we only want properties that have a SerializeField attribute !
            if (Attribute.IsDefined(propertyInfo, typeof(IncludeInDataAttribute), inherit: false))
            {
                // Debug.Log($"Property {propertyInfo.Name} has IncludeInData attribute, will be serialized.");
                return false;
            }

            // Check if the backing field has the attribute (for [field: SerializeField])
            string backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
            backingField = propertyInfo.DeclaringType?.GetField(
                backingFieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (backingField != null && Attribute.IsDefined(backingField, typeof(SerializeField), inherit: false))
            {
                // Debug.Log($"Backing field {backingField.Name} for property {propertyInfo.Name} has SerializeField attribute, will be serialized.");
                return false;
            }
            // Debug.Log($"Property {propertyInfo.Name} does not have IncludeInData attribute and its backing field does not have SerializeField attribute, will be skipped.");
            return true;
        }


        if (member is not FieldInfo field)
        {
            // Debug.Log($"Member {member.Name} ({member.MemberType}) is not a property or field, will be skipped.");
            return true;
        }

        // for fields, we don't want RuntimeOnly, NonSerialized, JsonIgnore, or not Serializable attributes
        // all the rest is good !
        // Debug.Log($"Checking field: {field.Name} of type {field.FieldType}");

        // checks attributes for Unity serialization
        // if (!Attribute.IsDefined(field, typeof(SerializableAttribute), inherit: false)) { return true; }
        if (Attribute.IsDefined(field, typeof(RuntimeOnlyAttribute), inherit: false))
        {
            // Debug.Log($"Field {field.Name} has RuntimeOnly attribute, will be skipped.");
            return true;
        }
        if (Attribute.IsDefined(field, typeof(NonSerializedAttribute), inherit: false))
        {
            // Debug.Log($"Field {field.Name} has NonSerialized attribute, will be skipped.");
            return true;
        }
        if (Attribute.IsDefined(field, typeof(JsonIgnoreAttribute), inherit: false))
        {
            // Debug.Log($"Field {field.Name} has JsonIgnore attribute, will be skipped.");
            return true;
        }

        return false;
    }

    /* private static bool ShouldSerializeType(Type type)
    {
        if (type == null) { return false; }

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)) { return true; }

        if (typeof(MonoBehaviour).IsAssignableFrom(type)) { return false; }

        Type nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            return ShouldSerializeType(nullableType);
        }

        if (type.IsArray)
        {
            return ShouldSerializeType(type.GetElementType());
        }

        return false;
    } */
}