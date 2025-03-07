using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Smoothie
{
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : Editor
    {
        private Theme _theme;
        private SerializedProperty _nameProperty;
        private SerializedProperty _parameterValuesProperty;
        
        // Режим редактирования параметров (отдельно от режима редактирования темы)
        private bool _editMode = false;
        
        // Статическая ссылка на тему, которая сейчас находится в режиме редактирования (Edit Theme)
        private static Theme currentlyEditingTheme = null;
        private bool IsEditingThisTheme => currentlyEditingTheme == _theme;
        
        // Размеры кнопок
        private const float ButtonHeight = 24f;
        private const float IconButtonWidth = 50f;   // для кнопок с иконками (Back, Duplicate, Delete)
        private const float EditButtonWidth = 100f;    // для текстовой кнопки "Edit Theme"
        
        // Стили кнопок
        private GUIStyle iconButtonStyle;
        private GUIStyle textButtonStyle;
        private GUIStyle toggleButtonStyle;
        
        private void OnEnable()
        {
            _theme = (Theme)target;
            _nameProperty = serializedObject.FindProperty("themeName");
            _parameterValuesProperty = serializedObject.FindProperty("parameterValues");
            
            // Инициализация стилей
            iconButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(8, 8, 4, 4),
                alignment = TextAnchor.MiddleCenter
            };
            textButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(10, 10, 4, 4),
                alignment = TextAnchor.MiddleCenter
            };
            toggleButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(10, 10, 4, 4),
                alignment = TextAnchor.MiddleCenter
            };
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.hierarchyMode = false;
            EditorGUIUtility.wideMode = true;
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            
            // Оборачиваем всё в горизонтальный контейнер с равными отступами слева и справа
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(0); // левый отступ 5
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10); // верхний отступ 10
            
            // Получаем ассет с иконками
            SmoothieUIAssets uiAssets = SmoothieUIAssets.Instance;
            GUIContent backContent, duplicateContent, deleteContent;
            if (uiAssets != null && uiAssets.backIcon != null && uiAssets.duplicateIcon != null && uiAssets.deleteIcon != null)
            {
                backContent      = new GUIContent(uiAssets.backIcon,      "Back to Theme Definition");
                duplicateContent = new GUIContent(uiAssets.duplicateIcon, "Duplicate this theme");
                deleteContent    = new GUIContent(uiAssets.deleteIcon,    "Delete this theme");
            }
            else
            {
                backContent      = new GUIContent("Back");
                duplicateContent = new GUIContent("Duplicate");
                deleteContent    = new GUIContent("Delete");
            }
            
            // --- Верхняя строка с кнопками ---
            EditorGUILayout.BeginHorizontal();
            {
                Color oldColor = GUI.color;
                
                // Кнопка Back (иконка) слева
                if (uiAssets != null)
                    GUI.color = uiAssets.backIconTint;
                if (GUILayout.Button(backContent, iconButtonStyle, GUILayout.Width(IconButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    if (_theme.ParentDefinition != null)
                    {
                        var path = AssetDatabase.GetAssetPath(_theme.ParentDefinition);
                        var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                        Selection.activeObject = mainAsset;
                    }
                }
                
                // Растягиваем пространство между кнопками
                GUILayout.FlexibleSpace();
                
                // Toggle-кнопка "Edit Theme" – она показывает, что тема в режиме редактирования.
                bool isEditing = IsEditingThisTheme;
                bool newIsEditing = GUILayout.Toggle(isEditing, "Edit Theme", toggleButtonStyle,
                    GUILayout.Width(EditButtonWidth), GUILayout.Height(ButtonHeight));
                if (newIsEditing != isEditing)
                {
                    if (newIsEditing)
                    {
                        // Если другая тема уже редактируется, переключаемся на эту
                        currentlyEditingTheme = _theme;
                    }
                    else
                    {
                        currentlyEditingTheme = null;
                    }
                }
                
                // Кнопка Duplicate (иконка)
                if (uiAssets != null)
                    GUI.color = uiAssets.duplicateIconTint;
                if (GUILayout.Button(duplicateContent, iconButtonStyle, GUILayout.Width(IconButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    DuplicateTheme();
                }
                
                // Кнопка Delete (иконка)
                if (uiAssets != null)
                    GUI.color = uiAssets.deleteIconTint;
                if (GUILayout.Button(deleteContent, iconButtonStyle, GUILayout.Width(IconButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    if (EditorUtility.DisplayDialog("Delete Theme",
                        $"Are you sure you want to delete the theme '{_theme.ThemeName}'?",
                        "Delete", "Cancel"))
                    {
                        DeleteTheme();
                        // После удаления выбираем родительский ассет
                        var path = AssetDatabase.GetAssetPath(_theme.ParentDefinition);
                        var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                        Selection.activeObject = mainAsset;
                        // Завершаем отрисовку, так как объект удалён
                        EndLayout();
                        return;
                    }
                }
                
                GUI.color = oldColor;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space(5);
            
            // Поле редактирования имени темы
            string newName = EditorGUILayout.DelayedTextField("Theme Name", _theme.ThemeName);
            if (newName != _theme.ThemeName)
            {
                _theme.ThemeName = newName;
                _theme.name = newName;
                EditorUtility.SetDirty(_theme);
                AssetDatabase.SaveAssets();
            }
            
            // Переключатель "Edit Parameters" (оставляем без изменений)
            _editMode = EditorGUILayout.Toggle("Edit Parameters", _editMode);
            
            // Подсчет параметров с полупрозрачным текстом
            int totalParams = _parameterValuesProperty.arraySize;
            int enabledParams = 0;
            for (int i = 0; i < totalParams; i++)
            {
                var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                var enabledProp = valueProp.FindPropertyRelative("Enabled");
                if (enabledProp.boolValue)
                    enabledParams++;
            }
            int hiddenParams = totalParams - enabledParams;
            string countText = $"Showing {enabledParams} of {totalParams} parameters";
            if (hiddenParams > 0)
                countText += $" ({hiddenParams} hidden)";
            GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.4f) }
            };
            EditorGUILayout.LabelField(countText, countStyle);
            
            EditorGUILayout.Space(10);
            
            // Проверяем наличие родительского ThemeDefinition
            if (_theme.ParentDefinition != null)
            {
                _theme.SynchronizeWithDefinition();
                serializedObject.Update();
            }
            else
            {
                EditorGUILayout.HelpBox("This theme has no parent definition.", MessageType.Warning);
                if (GUILayout.Button("Back to Theme Definition"))
                {
                    var assetPath = AssetDatabase.GetAssetPath(_theme);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    Selection.activeObject = mainAsset;
                }
                EndLayout();
                return;
            }
            
            // Отрисовка параметров темы
            foreach (var defParam in _theme.ParentDefinition.Parameters)
            {
                SerializedProperty paramValue = null;
                for (int i = 0; i < _parameterValuesProperty.arraySize; i++)
                {
                    var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                    var nameProperty = valueProp.FindPropertyRelative("ParameterName");
                    if (nameProperty.stringValue == defParam.Name)
                    {
                        paramValue = valueProp;
                        break;
                    }
                }
                if (paramValue == null)
                    continue;
                
                var enabledProperty = paramValue.FindPropertyRelative("Enabled");
                if (_editMode)
                {
                    if (defParam.Type == ThemeParameterType.Title)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(enabledProperty, GUIContent.none, GUILayout.Width(20));
                        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
                        EditorGUILayout.LabelField(defParam.Name, EditorStyles.boldLabel);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(enabledProperty, GUIContent.none, GUILayout.Width(20));
                        DrawParameterValue(defParam, paramValue, !enabledProperty.boolValue);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(2);
                    }
                }
                else
                {
                    if (enabledProperty.boolValue)
                    {
                        if (defParam.Type == ThemeParameterType.Title)
                        {
                            EditorGUILayout.Space(10);
                            EditorGUILayout.LabelField(defParam.Name, EditorStyles.boldLabel);
                            continue;
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            DrawParameterValue(defParam, paramValue, false);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(2);
                        }
                    }
                }
            }
            
            EditorGUILayout.Space(10);
            EndLayout();
            EditorGUI.indentLevel = oldIndent;
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawParameterValue(ThemeParameter defParam, SerializedProperty paramValue, bool isDisabled)
        {
            EditorGUI.BeginDisabledGroup(isDisabled);
            switch (defParam.Type)
            {
                case ThemeParameterType.Color:
                    var colorProperty = paramValue.FindPropertyRelative("ColorValue");
                    EditorGUILayout.PropertyField(colorProperty, new GUIContent(defParam.Name));
                    break;
                case ThemeParameterType.Float:
                    var floatProperty = paramValue.FindPropertyRelative("FloatValue");
                    EditorGUILayout.PropertyField(floatProperty, new GUIContent(defParam.Name));
                    break;
                case ThemeParameterType.Vector3:
                    var vectorProperty = paramValue.FindPropertyRelative("VectorValue");
                    EditorGUILayout.PropertyField(vectorProperty, new GUIContent(defParam.Name));
                    break;
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void DuplicateTheme()
        {
            if (_theme.ParentDefinition == null)
            {
                Debug.LogWarning("Cannot duplicate theme without a parent definition.");
                return;
            }
            Theme newTheme = Instantiate(_theme);
            newTheme.ThemeName = _theme.ThemeName + " Copy";
            newTheme.name = newTheme.ThemeName;
            AssetDatabase.AddObjectToAsset(newTheme, _theme.ParentDefinition);
            EditorUtility.SetDirty(_theme.ParentDefinition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = newTheme;
        }
        
        private void DeleteTheme()
        {
            if (_theme.ParentDefinition == null)
            {
                Debug.LogWarning("Cannot delete theme without a parent definition.");
                return;
            }
            Object.DestroyImmediate(_theme, true);
            EditorUtility.SetDirty(_theme.ParentDefinition);
            AssetDatabase.SaveAssets();
            
            // После удаления выбираем родительский ассет
            var path = AssetDatabase.GetAssetPath(_theme.ParentDefinition);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            Selection.activeObject = mainAsset;
        }
        
        // Завершает горизонтальный и вертикальный блоки
        private void EndLayout()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }
    }
}
