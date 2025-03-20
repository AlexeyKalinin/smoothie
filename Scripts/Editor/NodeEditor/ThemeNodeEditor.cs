using UnityEditor;
using UnityEngine;

namespace Smoothie.Editor.NodeEditor
{
    // Вместо использования атрибута [CustomEditor(typeof(Theme))], 
    // мы создаем редактор, который будет использоваться только 
    // через метод Editor.CreateEditor в InspectorView
    public class ThemeNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _themeNameProperty;
        
        private void OnEnable()
        {
            _themeNameProperty = serializedObject.FindProperty("themeName");
        }
        
        public override void OnInspectorGUI()
        {
            // Это специальный редактор только для отображения в графе узлов
            // проверяем, что мы находимся в контексте нодового редактора
            bool isInNodeContext = EditorWindow.focusedWindow is SmoothieGraphEditorWindow;
            
            // Если мы в окне графа, показываем упрощенный интерфейс
            if (isInNodeContext)
            {
                serializedObject.Update();
                
                Theme theme = (Theme)target;
                
                EditorGUI.BeginChangeCheck();
                
                // Theme name field
                string newName = EditorGUILayout.TextField("Theme Name", _themeNameProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    _themeNameProperty.stringValue = newName;
                    serializedObject.ApplyModifiedProperties();
                    
                    // Also update the asset name to match
                    if (theme.name != newName)
                    {
                        theme.name = newName;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    }
                }
                
                // Отображаем некоторую базовую информацию о теме
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Parent Definition", theme.ParentDefinition != null ? 
                    theme.ParentDefinition.name : "None");
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Parameter Values", EditorStyles.boldLabel);
                
                // Создаем кнопку для открытия темы в стандартном инспекторе
                EditorGUILayout.Space();
                if (GUILayout.Button("Edit Theme Parameters"))
                {
                    // Выбираем объект в проекте, чтобы открыть его в стандартном инспекторе
                    Selection.activeObject = theme;
                    EditorGUIUtility.PingObject(theme);
                }
            }
            else
            {
                // Если мы не в контексте нодового редактора,
                // используем стандартный инспектор
                base.OnInspectorGUI();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}