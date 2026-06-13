using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arc.Attribute
{

//  Usage Example
//  public enum BlockSides
//  {
//      Left = 1,
//      Right = 2,
//      Front = 4,
//      Back = 8,
//      Up = 16,
//      Down = 32
//  }

//  public class ExampleScript : MonoBehaviour
//  {
//      [EnumRestrict((int)BlockSides.Left, (int)BlockSides.Right, (int)BlockSides.Up)]
//      public BlockSides selectedSide;
//  }

    public class EnumFilterAttribute : PropertyAttribute
    {
        public int[] allowedValues;

        public EnumFilterAttribute(params int[] values)
        {
            allowedValues = values;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFilterAttribute))]
    public class EnumFilterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumFilterAttribute restrictAttribute = (EnumFilterAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                // Get the enum type
                System.Type enumType = fieldInfo.FieldType;
                string[] enumNames = property.enumNames;
                int[] enumValues = (int[])System.Enum.GetValues(enumType);

                // Filter allowed values
                var allowedNames = new System.Collections.Generic.List<string>();
                var allowedValues = new System.Collections.Generic.List<int>();

                for (int i = 0; i < enumValues.Length; i++)
                {
                    if (System.Array.IndexOf(restrictAttribute.allowedValues, enumValues[i]) >= 0)
                    {
                        allowedNames.Add(enumNames[i]);
                        allowedValues.Add(enumValues[i]);
                    }
                }

                // Show filtered dropdown
                int selectedIndex = allowedValues.IndexOf(property.intValue);
                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, allowedNames.ToArray());

                if (selectedIndex >= 0)
                {
                    property.intValue = allowedValues[selectedIndex];
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use EnumRestrict with an Enum!");
            }
        }
    }
#endif
}
