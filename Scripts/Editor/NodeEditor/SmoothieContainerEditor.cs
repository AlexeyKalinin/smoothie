// SmoothieContainerEditor.cs
using UnityEditor;
using UnityEngine;

namespace Smoothie.Editor
{
    [CustomEditor(typeof(SmoothieContainer))]
    public class SmoothieContainerEditor : UnityEditor.Editor
    {
        private SerializedProperty titleProperty;
        private SerializedProperty headerColorProperty;
        private SerializedProperty positionProperty;
        
        void OnEnable()
        {
            titleProperty = serializedObject.FindProperty("title");
            headerColorProperty = serializedObject.FindProperty("headerColor");
            positionProperty = serializedObject.FindProperty("position");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Title field with immediate update on Enter
            EditorGUI.BeginChangeCheck();
            string newTitle = EditorGUILayout.TextField("Title", titleProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                titleProperty.stringValue = newTitle;
                serializedObject.ApplyModifiedProperties();
                
                // Also update the asset name
                SmoothieContainer container = (SmoothieContainer)target;
                if (container.name != newTitle)
                {
                    container.name = newTitle;
                    EditorUtility.SetDirty(container);
                    AssetDatabase.SaveAssets();
                }
            }
            
            // Color field with immediate update
            EditorGUI.BeginChangeCheck();
            Color newColor = EditorGUILayout.ColorField("Header Color", headerColorProperty.colorValue);
            if (EditorGUI.EndChangeCheck())
            {
                headerColorProperty.colorValue = newColor;
                serializedObject.ApplyModifiedProperties();
            }
            
            // Position field (read-only in Inspector)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector2Field("Position", positionProperty.vector2Value);
            EditorGUI.EndDisabledGroup();
            
            // Display any connections
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}