using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arc.Attribute
{

    //  Usage Example
    //public class Example : MonoBehaviour
    //{
    //    public UISubTypes UISubTypes;

    //    [EnumStringSelector(nameof(UISubTypes), typeof(Type1), typeof(Type3))]
    //    public string Options;
    //}

    //public enum UISubTypes { Type1, Type3 }
    //public enum Type1 { AA, BB, CC }
    //public enum Type3 { ZZ, YY, XX }

    public class EnumStringSelectorAttribute : PropertyAttribute
    {
        public string FieldName { get; }
        public Type[] EnumTypes { get; }

        public EnumStringSelectorAttribute(string fieldName, params Type[] enumTypes)
        {
            FieldName = fieldName;
            EnumTypes = enumTypes;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumStringSelectorAttribute))]
    public class EnumStringSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (EnumStringSelectorAttribute)attribute;
            var so = property.serializedObject;
            var triggerProp = so.FindProperty(attr.FieldName);

            if (triggerProp == null || triggerProp.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position, label.text,
                    attr.FieldName + " not found or not an enum");
                return;
            }

            string selectedTriggerName = triggerProp.enumNames[triggerProp.enumValueIndex];

            Type matched = attr.EnumTypes
                .FirstOrDefault(t => t.IsEnum && t.Name == selectedTriggerName);

            if (matched == null)
            {
                EditorGUI.LabelField(position, label.text,
                    "No enum type matched " + selectedTriggerName);
                return;
            }

            var names = Enum.GetNames(matched);
            int curIndex = Array.IndexOf(names, property.stringValue);
            if (curIndex < 0) curIndex = 0;

            EditorGUI.BeginProperty(position, label, property);
            int newIndex = EditorGUI.Popup(position, label.text, curIndex, names);
            property.stringValue = names[newIndex];
            EditorGUI.EndProperty();
        }
    }
#endif


}
