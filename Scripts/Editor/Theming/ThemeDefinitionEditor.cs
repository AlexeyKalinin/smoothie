using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Smoothie
{
    [CustomEditor(typeof(ThemeDefinition))]
    public class ThemeDefinitionEditor : Editor
    {
        private bool _parametersFoldout = true;
        private bool _themesFoldout = true;
        private Dictionary<int, bool> _actionsFoldouts = new Dictionary<int, bool>();
        
        private ThemeDefinition _themeDefinition;
        private SerializedProperty _parametersProperty;
        
        private void OnEnable()
        {
            _themeDefinition = (ThemeDefinition)target;
            _parametersProperty = serializedObject.FindProperty("parameters");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Theme Definition", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Parameters section
            _parametersFoldout = EditorGUILayout.Foldout(_parametersFoldout, "Parameters", true, EditorStyles.foldoutHeader);
            if (_parametersFoldout)
            {
                EditorGUILayout.Space(5);
                DrawParametersList();
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Add Parameter", GUILayout.Height(30)))
                {
                    AddParameter();
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Themes section
            _themesFoldout = EditorGUILayout.Foldout(_themesFoldout, "Themes", true, EditorStyles.foldoutHeader);
            if (_themesFoldout)
            {
                EditorGUILayout.Space(5);
                DrawThemesList();
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Create New Theme", GUILayout.Height(32)))
                {
                    CreateNewTheme();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawDivider(Rect rect)
        {
            // Верхняя тень (темная линия)
            Rect shadowRect = new Rect(rect.x, rect.y, rect.width, 1);
            EditorGUI.DrawRect(shadowRect, new Color(0.1f, 0.1f, 0.1f, 0.3f));
    
            // Нижняя подсветка (светлая линия)
            Rect highlightRect = new Rect(rect.x, rect.y + 1, rect.width, 1);
            EditorGUI.DrawRect(highlightRect, new Color(1f, 1f, 1f, 0.1f));
        }
        
        private void DrawParametersList()
        {
            for (int i = 0; i < _parametersProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var paramElement = _parametersProperty.GetArrayElementAtIndex(i);
                var nameProperty = paramElement.FindPropertyRelative("Name");
                var typeProperty = paramElement.FindPropertyRelative("Type");
                var actionsProperty = paramElement.FindPropertyRelative("Actions");
                
                EditorGUILayout.BeginHorizontal();
                
                ThemeParameterType currentType = (ThemeParameterType)typeProperty.enumValueIndex;
                
                // Вместо обычного EditorGUILayout.PropertyField для Divider и Space
if (currentType == ThemeParameterType.Space || currentType == ThemeParameterType.Divider)
{
    EditorGUILayout.BeginHorizontal();
    
    // Отображаем тип (Space или Divider)
    EditorGUILayout.PropertyField(typeProperty, GUIContent.none);
    
    // Кнопки управления
    if (GUILayout.Button("↑", GUILayout.Width(25)))
    {
        _themeDefinition.MoveParameterUp(i);
        serializedObject.Update();
    }
    
    if (GUILayout.Button("↓", GUILayout.Width(25)))
    {
        _themeDefinition.MoveParameterDown(i);
        serializedObject.Update();
    }
    
    if (GUILayout.Button("×", GUILayout.Width(25)))
    {
        _themeDefinition.RemoveParameter(i);
        serializedObject.Update();
        break;  // Exit the loop since we've removed an item
    }
    
    EditorGUILayout.EndHorizontal();
    
    // Если это разделитель, рисуем линию
    if (currentType == ThemeParameterType.Divider)
    {
        EditorGUILayout.Space(3);
        Rect rect = EditorGUILayout.GetControlRect(false, 2);
        DrawDivider(rect);
        EditorGUILayout.Space(3);
    }
    else if (currentType == ThemeParameterType.Space)
    {
        EditorGUILayout.Space(10); // Дополнительное пространство для Space
    }
}
else
{
    // Обычная обработка для остальных типов параметров
    EditorGUILayout.BeginHorizontal();
    
    EditorGUILayout.PropertyField(nameProperty, GUIContent.none, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f - 60));
    EditorGUILayout.PropertyField(typeProperty, GUIContent.none, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f - 60));
    
    // Кнопки управления
    if (GUILayout.Button("↑", GUILayout.Width(25)))
    {
        _themeDefinition.MoveParameterUp(i);
        serializedObject.Update();
    }
    
    if (GUILayout.Button("↓", GUILayout.Width(25)))
    {
        _themeDefinition.MoveParameterDown(i);
        serializedObject.Update();
    }
    
    if (GUILayout.Button("×", GUILayout.Width(25)))
    {
        _themeDefinition.RemoveParameter(i);
        serializedObject.Update();
        break;  // Exit the loop since we've removed an item
    }
    
    EditorGUILayout.EndHorizontal();
}
                
                // Add control buttons
                if (GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    _themeDefinition.MoveParameterUp(i);
                    serializedObject.Update();
                }
                
                if (GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    _themeDefinition.MoveParameterDown(i);
                    serializedObject.Update();
                }
                
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    _themeDefinition.RemoveParameter(i);
                    serializedObject.Update();
                    break;  // Exit the loop since we've removed an item
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Draw actions for non-Title parameters
                if (currentType != ThemeParameterType.Title && 
                    currentType != ThemeParameterType.Space && 
                    currentType != ThemeParameterType.Divider)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    if (!_actionsFoldouts.ContainsKey(i))
                    {
                        _actionsFoldouts[i] = actionsProperty.arraySize > 0;
                    }
                    
                    GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
                    if (actionsProperty.arraySize == 0)
                    {
                        foldoutStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                    }
                    else
                    {
                        foldoutStyle.normal.textColor = new Color(0.3f, 0.5f, 0.85f);
                    }
                    
                    _actionsFoldouts[i] = EditorGUILayout.Foldout(_actionsFoldouts[i], 
                        $"Actions ({actionsProperty.arraySize})", true, foldoutStyle);
                    
                    if (GUILayout.Button("Add Action", GUILayout.Width(100)))
                    {
                        AddAction(i);
                        _actionsFoldouts[i] = true;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (_actionsFoldouts[i] && actionsProperty.arraySize > 0)
                    {
                        for (int actionIdx = 0; actionIdx < actionsProperty.arraySize; actionIdx++)
                        {
                            DrawActionProperty(actionsProperty.GetArrayElementAtIndex(actionIdx), i, actionIdx, currentType);
                        }
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }
        
        private void DrawActionProperty(SerializedProperty actionProperty, int parameterIndex, int actionIndex, ThemeParameterType paramType)
        {
            EditorGUI.indentLevel++;
            
            var actionTypeProperty = actionProperty.FindPropertyRelative("ActionType");
            var targetObjectProperty = actionProperty.FindPropertyRelative("TargetObject");
            var propertyNameProperty = actionProperty.FindPropertyRelative("PropertyName");
            
            var currentActionType = (ThemeActionType)actionTypeProperty.enumValueIndex;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            string[] options = GetActionOptionsForType((int)paramType);
            int[] optionValues = GetActionValuesForType((int)paramType);
            
            int currentIndex = System.Array.IndexOf(optionValues, (int)currentActionType);
            if (currentIndex < 0) currentIndex = 0;
            
            int newIndex = EditorGUILayout.Popup("Action", currentIndex, options);
            if (newIndex != currentIndex)
            {
                actionTypeProperty.enumValueIndex = optionValues[newIndex];
                currentActionType = (ThemeActionType)optionValues[newIndex];
            }
            
            // Add remove button
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                var actionsProperty = _parametersProperty.GetArrayElementAtIndex(parameterIndex).FindPropertyRelative("Actions");
                actionsProperty.DeleteArrayElementAtIndex(actionIndex);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Handle target object
            switch (currentActionType)
            {
                case ThemeActionType.ChangeMaterialColor:
                case ThemeActionType.ChangeMaterialProperty:
                    EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Material"));
                    break;
                case ThemeActionType.ChangeLightColor:
                case ThemeActionType.ChangeLightIntensity:
                    EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Light"));
                    break;
                case ThemeActionType.ChangePosition:
                case ThemeActionType.ChangeRotation:
                case ThemeActionType.ChangeScale:
                    EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Transform"));
                    break;
                default:
                    EditorGUILayout.PropertyField(targetObjectProperty, new GUIContent("Target"));
                    break;
            }
            
            // Property name for material properties
            if (currentActionType == ThemeActionType.ChangeMaterialProperty)
            {
                EditorGUILayout.PropertyField(propertyNameProperty, new GUIContent("Property Name"));
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawThemesList()
        {
            var themes = GetThemesForDefinition();
            
            if (themes.Length == 0)
            {
                EditorGUILayout.HelpBox("No themes created for this definition yet.", MessageType.Info);
                return;
            }
            
            // Sort themes by name
            System.Array.Sort(themes, (a, b) => 
                string.Compare(a.ThemeName, b.ThemeName, System.StringComparison.OrdinalIgnoreCase));
            
            // Get UI assets for icons
            SmoothieUIAssets uiAssets = SmoothieUIAssets.Instance;
            
            foreach (var theme in themes)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // Theme name
                EditorGUILayout.LabelField(theme.ThemeName, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                
                // Open button
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    Selection.activeObject = theme;
                }
                
                // Duplicate button
                GUIContent duplicateContent;
                Color originalColor = GUI.color;
                
                if (uiAssets != null && uiAssets.duplicateIcon != null)
                {
                    GUI.color = uiAssets.duplicateIconTint;
                    duplicateContent = new GUIContent(uiAssets.duplicateIcon);
                }
                else
                {
                    duplicateContent = new GUIContent("D");
                }
                
                if (GUILayout.Button(duplicateContent, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    DuplicateTheme(theme);
                }
                
                // Delete button
                if (uiAssets != null && uiAssets.deleteIcon != null)
                {
                    GUI.color = uiAssets.deleteIconTint;
                    var deleteContent = new GUIContent(uiAssets.deleteIcon);
                    
                    if (GUILayout.Button(deleteContent, GUILayout.Width(30), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Theme",
                            $"Are you sure you want to delete the theme '{theme.ThemeName}'?",
                            "Delete", "Cancel"))
                        {
                            DeleteTheme(theme);
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("×", GUILayout.Width(30), GUILayout.Height(20)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Theme",
                            $"Are you sure you want to delete the theme '{theme.ThemeName}'?",
                            "Delete", "Cancel"))
                        {
                            DeleteTheme(theme);
                        }
                    }
                }
                
                GUI.color = originalColor;
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private Theme[] GetThemesForDefinition()
        {
            string assetPath = AssetDatabase.GetAssetPath(_themeDefinition);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var themes = new List<Theme>();
            
            foreach (var asset in assets)
            {
                if (asset is Theme theme && theme.ParentDefinition == _themeDefinition)
                {
                    themes.Add(theme);
                }
            }
            
            return themes.ToArray();
        }
        
        private void AddParameter()
        {
            int index = _parametersProperty.arraySize;
            _parametersProperty.arraySize++;
            
            var element = _parametersProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Name").stringValue = "New Parameter";
            element.FindPropertyRelative("Type").enumValueIndex = 0;
            var actionsProperty = element.FindPropertyRelative("Actions");
            actionsProperty.ClearArray();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void AddAction(int parameterIndex)
        {
            if (parameterIndex < 0 || parameterIndex >= _parametersProperty.arraySize)
                return;
                
            var paramElement = _parametersProperty.GetArrayElementAtIndex(parameterIndex);
            var actionsProperty = paramElement.FindPropertyRelative("Actions");
            
            // Get parameter type
            int paramTypeIndex = paramElement.FindPropertyRelative("Type").enumValueIndex;
            ThemeParameterType paramType = (ThemeParameterType)paramTypeIndex;
            
            int newIndex = actionsProperty.arraySize;
            actionsProperty.arraySize++;
            
            var actionProperty = actionsProperty.GetArrayElementAtIndex(newIndex);
            
            // Set default ActionType based on parameter type
            switch (paramType)
            {
                case ThemeParameterType.Color:
                    actionProperty.FindPropertyRelative("ActionType").enumValueIndex = (int)ThemeActionType.ChangeMaterialColor;
                    break;
                case ThemeParameterType.Float:
                    actionProperty.FindPropertyRelative("ActionType").enumValueIndex = (int)ThemeActionType.ChangeLightIntensity;
                    break;
                case ThemeParameterType.Vector3:
                    actionProperty.FindPropertyRelative("ActionType").enumValueIndex = (int)ThemeActionType.ChangePosition;
                    break;
                default:
                    actionProperty.FindPropertyRelative("ActionType").enumValueIndex = (int)ThemeActionType.None;
                    break;
            }
            
            actionProperty.FindPropertyRelative("TargetObject").objectReferenceValue = null;
            actionProperty.FindPropertyRelative("PropertyName").stringValue = "";
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateNewTheme()
        {
            var newTheme = CreateInstance<Theme>();
            newTheme.ThemeName = "New Theme";
            newTheme.Initialize(_themeDefinition);
            
            AssetDatabase.AddObjectToAsset(newTheme, _themeDefinition);
            
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = newTheme;
        }
        
        private void DuplicateTheme(Theme sourceTheme)
        {
            Theme newTheme = Instantiate(sourceTheme);
            newTheme.ThemeName = sourceTheme.ThemeName + " Copy";
            newTheme.name = newTheme.ThemeName;
            
            AssetDatabase.AddObjectToAsset(newTheme, _themeDefinition);
            
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = newTheme;
        }
        
        private void DeleteTheme(Theme theme)
        {
            Object.DestroyImmediate(theme, true);
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
        }
        
        // Helper methods for action types
        private string[] GetActionOptionsForType(int paramTypeIndex)
        {
            var paramType = (ThemeParameterType)paramTypeIndex;
            switch (paramType)
            {
                case ThemeParameterType.Color:
                    return new[] { "Change Material Color", "Change Light Color" };
                case ThemeParameterType.Float:
                    return new[] { "Change Light Intensity", "Change Material Property" };
                case ThemeParameterType.Vector3:
                    return new[] { "Change Position", "Change Rotation", "Change Scale" };
                default:
                    return new[] { "None" };
            }
        }
        
        private int[] GetActionValuesForType(int paramTypeIndex)
        {
            var paramType = (ThemeParameterType)paramTypeIndex;
            switch (paramType)
            {
                case ThemeParameterType.Color:
                    return new[] { (int)ThemeActionType.ChangeMaterialColor, (int)ThemeActionType.ChangeLightColor };
                case ThemeParameterType.Float:
                    return new[] { (int)ThemeActionType.ChangeLightIntensity, (int)ThemeActionType.ChangeMaterialProperty };
                case ThemeParameterType.Vector3:
                    return new[] { (int)ThemeActionType.ChangePosition, (int)ThemeActionType.ChangeRotation, (int)ThemeActionType.ChangeScale };
                default:
                    return new[] { (int)ThemeActionType.None };
            }
        }
    }
}