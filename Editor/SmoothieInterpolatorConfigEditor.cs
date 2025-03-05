using UnityEngine;
using UnityEditor;

namespace Smoothie
{
    using Config = FloatInterpolator.Config;

    [CustomPropertyDrawer(typeof(Config))]
    class SmoothieInterpolatorConfigEditor : PropertyDrawer
    {
        static GUIContent _textSpeed = new GUIContent("Speed");
        static GUIContent _textElasticity = new GUIContent("Elasticity");

        bool CheckShouldExpand(SerializedProperty property)
        {
            var type = property.FindPropertyRelative("_interpolationType");
            return type.hasMultipleDifferentValues ||
                type.enumValueIndex != (int)Config.InterpolationType.Direct;
        }

        bool CheckShouldExpandSpring(SerializedProperty property)
        {
            var type = property.FindPropertyRelative("_interpolationType");
            return type.hasMultipleDifferentValues ||
                type.enumValueIndex == (int)Config.InterpolationType.Spring;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineSpace = EditorGUIUtility.standardVerticalSpacing;
            if (CheckShouldExpandSpring(property))
            {
                return lineHeight * 3 + lineSpace; // Add an extra line for elasticity
            }
            else if (CheckShouldExpand(property))
            {
                return lineHeight * 2 + lineSpace; // Standard height for other interpolation types
            }
            else
            {
                return lineHeight; // Standard height when no interpolation type is selected
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            const int indentWidth = 16;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var lineSpace = EditorGUIUtility.standardVerticalSpacing;

            // these controls have single line height
            position.height = lineHeight;

            // interpolation type
            var type = property.FindPropertyRelative("_interpolationType");
            EditorGUI.PropertyField(position, type, label);

            if (CheckShouldExpand(property))
            {
                // indent the line
                position.width -= indentWidth;
                position.x += indentWidth;
                EditorGUIUtility.labelWidth -= indentWidth;

                // go to the next line
                position.y += lineHeight + lineSpace;

                // interpolation speed
                var speed = property.FindPropertyRelative("_interpolationSpeed");
                EditorGUI.PropertyField(position, speed, _textSpeed);

                position.y += lineHeight + lineSpace;

                if (CheckShouldExpandSpring(property))
                {
                    // interpolation elasticity
                    var elasticity = property.FindPropertyRelative("_interpolationElasticity");
                    EditorGUI.PropertyField(position, elasticity, _textElasticity);
                    position.y += lineHeight + lineSpace;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
