using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Smoothie
{
    /// <summary>
    /// Custom editor for Theme sub-assets
    /// </summary>
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : UnityEditor.Editor
    {
        private Theme _theme;
        private SerializedProperty _nameProperty;
        private SerializedProperty _parameterValuesProperty;
        
        // Edit mode toggle
        private bool _editMode = false;
        
        private void OnEnable()
        {
            _theme = (Theme)target;
            _nameProperty = serializedObject.FindProperty("themeName");
            _parameterValuesProperty = serializedObject.FindProperty("parameterValues");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            
            // Theme Settings: редактирование имени темы через DelayedTextField
            EditorGUILayout.LabelField("Theme Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            string newName = EditorGUILayout.DelayedTextField("Theme Name", _theme.ThemeName);
            if(newName != _theme.ThemeName)
            {
                _theme.ThemeName = newName;
                // Переименовываем только сабассет, присваивая новое имя объекту
                _theme.name = newName;
                EditorUtility.SetDirty(_theme);
                AssetDatabase.SaveAssets();
            }
            
            // Синхронизация темы с её определением
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
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            // Подсчёт параметров
            int totalParams = _parameterValuesProperty.arraySize;
            int enabledParams = 0;
            for (int i = 0; i < _parameterValuesProperty.arraySize; i++)
            {
                var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                var enabledProp = valueProp.FindPropertyRelative("Enabled");
                if (enabledProp.boolValue)
                {
                    enabledParams++;
                }
            }
            int hiddenParams = totalParams - enabledParams;
            
            // Отображение переключателя Edit и счётчика параметров
            EditorGUILayout.BeginVertical();
                _editMode = EditorGUILayout.Toggle("Edit", _editMode, GUILayout.Width(100));
                EditorGUILayout.Space(0);
                
                string countText = $"Showing {enabledParams} of {totalParams} parameters";
                if (hiddenParams > 0)
                {
                    countText += $" ({hiddenParams} hidden)";
                }
                
                GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleLeft
                };
                EditorGUILayout.LabelField(countText, countStyle);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Отображение параметров в соответствии с определением темы
            foreach (var defParam in _theme.ParentDefinition.Parameters)
            {
                // Специальная обработка для Space и Divider
                if (defParam.Type == ThemeParameterType.Space || defParam.Type == ThemeParameterType.Divider)
                {
                    // Пытаемся найти соответствующее значение параметра в сабассете
                    SerializedProperty paramValue = null;
                    for (int i = 0; i < _parameterValuesProperty.arraySize; i++)
                    {
                        var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                        var nameProperty = valueProp.FindPropertyRelative("ParameterName");
                        // Для Space/Divider имя обычно пустое, поэтому сравнение по значению параметра
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
                        // В режиме редактирования показываем чекбокс и надпись "Space" или "Divider"
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(enabledProperty, GUIContent.none, GUILayout.Width(20));
                        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
                        EditorGUILayout.LabelField(defParam.Type == ThemeParameterType.Space ? "Space" : "Divider", EditorStyles.miniLabel);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        // В обычном режиме, если параметр отключён – не отображаем его
                        if (!enabledProperty.boolValue)
                            continue;
                        
                        if (defParam.Type == ThemeParameterType.Space)
                        {
                            EditorGUILayout.Space(10);
                        }
                        else if (defParam.Type == ThemeParameterType.Divider)
                        {
                            Color dividerColor = EditorGUIUtility.isProSkin 
                                ? new Color(0.15f, 0.15f, 0.15f) 
                                : new Color(0.7f, 0.7f, 0.7f);
                            Color highlightColor = EditorGUIUtility.isProSkin 
                                ? new Color(0.3f, 0.3f, 0.3f) 
                                : new Color(0.9f, 0.9f, 0.9f);
                            Rect dividerRect = EditorGUILayout.GetControlRect(false, 4);
                            EditorGUI.DrawRect(dividerRect, dividerColor);
                            Rect highlightRect = new Rect(dividerRect.x, dividerRect.yMax - 1, dividerRect.width, 1);
                            EditorGUI.DrawRect(highlightRect, highlightColor);
                            EditorGUILayout.Space(5);
                        }
                    }
                    
                    continue;
                }
                
                // Для остальных параметров: поиск соответствующего свойства в сабассете по имени
                SerializedProperty paramValueOther = null;
                for (int i = 0; i < _parameterValuesProperty.arraySize; i++)
                {
                    var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                    var nameProperty = valueProp.FindPropertyRelative("ParameterName");
                    if (nameProperty.stringValue == defParam.Name)
                    {
                        paramValueOther = valueProp;
                        break;
                    }
                }
                if (paramValueOther == null)
                    continue;
                
                var enabledPropertyOther = paramValueOther.FindPropertyRelative("Enabled");
                if (_editMode)
                {
                    if (defParam.Type == ThemeParameterType.Title)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(enabledPropertyOther, GUIContent.none, GUILayout.Width(20));
                        EditorGUI.BeginDisabledGroup(!enabledPropertyOther.boolValue);
                        EditorGUILayout.LabelField(defParam.Name, EditorStyles.boldLabel);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(5);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(enabledPropertyOther, GUIContent.none, GUILayout.Width(20));
                        DrawParameterValue(defParam, paramValueOther, !enabledPropertyOther.boolValue);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(2);
                    }
                }
                else
                {
                    if (enabledPropertyOther.boolValue)
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
                            DrawParameterValue(defParam, paramValueOther, false);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(2);
                        }
                    }
                }
            }
            
            EditorGUILayout.Space(10);
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
                default:
                    break;
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
