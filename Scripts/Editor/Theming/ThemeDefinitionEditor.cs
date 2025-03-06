using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Smoothie
{
    /// <summary>
    /// Custom editor for ThemeDefinition ScriptableObjects
    /// </summary>
    [CustomEditor(typeof(ThemeDefinition))]
    public class ThemeDefinitionEditor : UnityEditor.Editor
    {
        private ReorderableList _parametersList;
        private ThemeDefinition _themeDefinition;
        private SerializedProperty _parametersProperty;
        private Dictionary<int, ReorderableList> _actionsLists = new Dictionary<int, ReorderableList>();
        
        // Foldout states
        private bool _parametersFoldout = true;
        private bool _themesFoldout = true;
        private Dictionary<int, bool> _actionsFoldouts = new Dictionary<int, bool>();

        private void OnEnable()
        {
            _themeDefinition = (ThemeDefinition)target;
            _parametersProperty = serializedObject.FindProperty("parameters");
            
            // Set up reorderable list for parameters
            _parametersList = new ReorderableList(serializedObject, _parametersProperty, true, true, true, true);
            
            _parametersList.drawHeaderCallback = rect => 
            {
                EditorGUI.LabelField(rect, "Theme Parameters");
            };
            
            _parametersList.elementHeightCallback = index =>
            {
                var element = _parametersProperty.GetArrayElementAtIndex(index);
                var typeProperty = element.FindPropertyRelative("Type");
                var actionsProperty = element.FindPropertyRelative("Actions");
                
                float baseHeight = EditorGUIUtility.singleLineHeight + 4; // Base height for name and type
                ThemeParameterType currentType = (ThemeParameterType)typeProperty.enumValueIndex;
                // Для Title, Space и Divider экшены не нужны – возвращаем базовую высоту
                if (currentType == ThemeParameterType.Title || currentType == ThemeParameterType.Space || currentType == ThemeParameterType.Divider)
                    return baseHeight;
                
                // Добавляем высоту для раскрывающегося списка экшенов
                baseHeight += EditorGUIUtility.singleLineHeight + 4;
                
                // Если экшены раскрыты, добавляем высоту для каждого из них
                bool actionsFoldout = _actionsFoldouts.ContainsKey(index) && _actionsFoldouts[index];
                if (actionsFoldout && actionsProperty.arraySize > 0)
                {
                    if (!_actionsLists.ContainsKey(index))
                    {
                        CreateActionsList(index);
                    }
                    
                    var actionsList = _actionsLists[index];
                    baseHeight += actionsList.GetHeight();
                }
                
                // Добавляем высоту для кнопки "Add Action"
                if (actionsFoldout)
                {
                    baseHeight += EditorGUIUtility.singleLineHeight + 8;
                }
                
                return baseHeight;
            };
            
            _parametersList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _parametersProperty.GetArrayElementAtIndex(index);
                var nameProperty = element.FindPropertyRelative("Name");
                var typeProperty = element.FindPropertyRelative("Type");
                var actionsProperty = element.FindPropertyRelative("Actions");
                
                float lineHeight = EditorGUIUtility.singleLineHeight;
                rect.y += 2;
                
                ThemeParameterType currentType = (ThemeParameterType)typeProperty.enumValueIndex;
                // Для параметров Space или Divider отрисовываем только поле выбора типа
                if (currentType == ThemeParameterType.Space || currentType == ThemeParameterType.Divider)
                {
                    Rect typeRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                    EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
                    return;
                }
                
                // Отрисовка имени и типа для остальных параметров
                Rect nameRect = new Rect(rect.x, rect.y, rect.width * 0.5f - 5, lineHeight);
                EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
                
                Rect typeRectNormal = new Rect(nameRect.xMax + 10, rect.y, rect.width * 0.5f - 5, lineHeight);
                EditorGUI.PropertyField(typeRectNormal, typeProperty, GUIContent.none);
                
                rect.y += lineHeight + 4;
                
                // Для Title параметров экшены не нужны
                if (currentType == ThemeParameterType.Title)
                    return;
                
                // Отрисовка метки экшенов и кнопки "Add Action" для остальных параметров
                Rect actionsLabelRect = new Rect(rect.x, rect.y, rect.width * 0.5f, lineHeight);
                Rect addButtonRect = new Rect(rect.x + rect.width * 0.5f + 5, rect.y, rect.width * 0.5f - 5, lineHeight);
                
                if (!_actionsFoldouts.ContainsKey(index))
                {
                    _actionsFoldouts[index] = actionsProperty.arraySize > 0;
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
                
                _actionsFoldouts[index] = EditorGUI.Foldout(actionsLabelRect, _actionsFoldouts[index], 
                    $"Actions ({actionsProperty.arraySize})", true, foldoutStyle);
                
                if (GUI.Button(addButtonRect, "Add Action"))
                {
                    AddAction(index);
                    _actionsFoldouts[index] = true; // Авто-раскрытие при добавлении
                }
                
                if (_actionsFoldouts[index])
                {
                    rect.y += lineHeight + 4;
                    
                    if (!_actionsLists.ContainsKey(index))
                    {
                        CreateActionsList(index);
                    }
                    
                    if (actionsProperty.arraySize > 0)
                    {
                        Rect actionsRect = new Rect(rect.x, rect.y, rect.width, _actionsLists[index].GetHeight());
                        _actionsLists[index].DoList(actionsRect);
                    }
                    else
                    {
                        Rect noActionsRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                        EditorGUI.LabelField(noActionsRect, "No actions defined for this parameter.", 
                            new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic });
                    }
                }
            };
            
            _parametersList.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
                
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                // По умолчанию имя пустое для новых параметров (для Space и Divider так же можно оставить пустым)
                element.FindPropertyRelative("Name").stringValue = "";
                element.FindPropertyRelative("Type").enumValueIndex = 0; // Title по умолчанию
                var actionsProperty = element.FindPropertyRelative("Actions");
                actionsProperty.ClearArray();
            };
            
            _parametersList.onRemoveCallback = list =>
            {
                int indexToRemove = list.index;
                
                if (_actionsLists.ContainsKey(indexToRemove))
                {
                    _actionsLists.Remove(indexToRemove);
                }
                if (_actionsFoldouts.ContainsKey(indexToRemove))
                {
                    _actionsFoldouts.Remove(indexToRemove);
                }
                
                list.serializedProperty.DeleteArrayElementAtIndex(indexToRemove);
                list.index = Mathf.Clamp(indexToRemove - 1, 0, list.serializedProperty.arraySize - 1);
                serializedObject.ApplyModifiedProperties();
                RebuildActionsLists();
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Theme Definition", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Раздел для параметров
            _parametersFoldout = EditorGUILayout.Foldout(_parametersFoldout, "Parameters", true, EditorStyles.foldoutHeader);
            if (_parametersFoldout)
            {
                EditorGUILayout.Space(5);
                _parametersList.DoLayoutList();
            }
            
            EditorGUILayout.Space(10);
            
            // Раздел для тем
            _themesFoldout = EditorGUILayout.Foldout(_themesFoldout, "Themes", true, EditorStyles.foldoutHeader);
            if (_themesFoldout)
            {
                EditorGUILayout.Space(5);
                DrawThemesList();
                
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Create New Theme"))
                {
                    CreateNewTheme();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateActionsList(int parameterIndex)
        {
            var paramElement = _parametersProperty.GetArrayElementAtIndex(parameterIndex);
            var actionsProperty = paramElement.FindPropertyRelative("Actions");
            var paramType = (ThemeParameterType)paramElement.FindPropertyRelative("Type").enumValueIndex;
            
            var actionsList = new ReorderableList(serializedObject, actionsProperty, true, true, false, true);
            
            actionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Parameter Actions");
            };
            
            actionsList.elementHeightCallback = index =>
            {
                var actionProperty = actionsProperty.GetArrayElementAtIndex(index);
                var actionType = (ThemeActionType)actionProperty.FindPropertyRelative("ActionType").enumValueIndex;
                
                float height = EditorGUIUtility.singleLineHeight * 2 + 4;
                if (actionType == ThemeActionType.ChangeMaterialProperty)
                {
                    height += EditorGUIUtility.singleLineHeight + 2;
                }
                
                return height;
            };
            
            actionsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var actionProperty = actionsProperty.GetArrayElementAtIndex(index);
                var actionTypeProperty = actionProperty.FindPropertyRelative("ActionType");
                var targetObjectProperty = actionProperty.FindPropertyRelative("TargetObject");
                var propertyNameProperty = actionProperty.FindPropertyRelative("PropertyName");
                
                rect.y += 2;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                
                EditorGUI.BeginChangeCheck();
                Rect actionTypeRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                
                string[] options = GetActionOptionsForType(paramType);
                int[] optionValues = GetActionValuesForType(paramType);
                
                int currentIndex = System.Array.IndexOf(optionValues, actionTypeProperty.enumValueIndex);
                if (currentIndex < 0) currentIndex = 0;
                
                int newIndex = EditorGUI.Popup(actionTypeRect, "Action", currentIndex, options);
                if (EditorGUI.EndChangeCheck())
                {
                    actionTypeProperty.enumValueIndex = optionValues[newIndex];
                }
                
                rect.y += lineHeight + 2;
                
                Rect targetRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                EditorGUI.BeginChangeCheck();
                Object newTarget = null;
                
                var actionType = (ThemeActionType)actionTypeProperty.enumValueIndex;
                switch (actionType)
                {
                    case ThemeActionType.ChangeMaterialColor:
                    case ThemeActionType.ChangeMaterialProperty:
                        newTarget = EditorGUI.ObjectField(targetRect, "Material", targetObjectProperty.objectReferenceValue, typeof(Material), false);
                        break;
                    case ThemeActionType.ChangeLightColor:
                    case ThemeActionType.ChangeLightIntensity:
                        newTarget = EditorGUI.ObjectField(targetRect, "Light", targetObjectProperty.objectReferenceValue, typeof(Light), true);
                        break;
                    case ThemeActionType.ChangePosition:
                    case ThemeActionType.ChangeRotation:
                    case ThemeActionType.ChangeScale:
                        newTarget = EditorGUI.ObjectField(targetRect, "Transform", targetObjectProperty.objectReferenceValue, typeof(Transform), true);
                        break;
                    default:
                        newTarget = EditorGUI.ObjectField(targetRect, "Target", targetObjectProperty.objectReferenceValue, typeof(Object), true);
                        break;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    targetObjectProperty.objectReferenceValue = newTarget;
                }
                
                rect.y += lineHeight + 2;
                if (actionType == ThemeActionType.ChangeMaterialProperty)
                {
                    Rect propNameRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
                    EditorGUI.PropertyField(propNameRect, propertyNameProperty, new GUIContent("Property Name"));
                }
            };
            
            _actionsLists[parameterIndex] = actionsList;
        }
        
        private string[] GetActionOptionsForType(ThemeParameterType paramType)
        {
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
        
        private int[] GetActionValuesForType(ThemeParameterType paramType)
        {
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
        
        private void AddAction(int parameterIndex)
        {
            var paramElement = _parametersProperty.GetArrayElementAtIndex(parameterIndex);
            var actionsProperty = paramElement.FindPropertyRelative("Actions");
            var paramType = (ThemeParameterType)paramElement.FindPropertyRelative("Type").enumValueIndex;
            
            int newIndex = actionsProperty.arraySize;
            actionsProperty.arraySize++;
            
            var actionProperty = actionsProperty.GetArrayElementAtIndex(newIndex);
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
        
        private void RebuildActionsLists()
        {
            _actionsLists.Clear();
        }
        
        private void DrawThemesList()
        {
            var themes = GetThemesForDefinition();
            
            if (themes.Length == 0)
            {
                EditorGUILayout.HelpBox("No themes created for this definition yet.", MessageType.Info);
                return;
            }
            
            foreach (var theme in themes)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                GUILayout.Label(theme.ThemeName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    Selection.activeObject = theme;
                }
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Theme", 
                        $"Are you sure you want to delete the theme '{theme.ThemeName}'?", 
                        "Delete", "Cancel"))
                    {
                        DeleteTheme(theme);
                    }
                }
                
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
        
        private void DeleteTheme(Theme theme)
        {
            Object.DestroyImmediate(theme, true);
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
        }
    }
}
