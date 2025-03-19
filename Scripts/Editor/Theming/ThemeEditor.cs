using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Linq;

namespace Smoothie
{
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : UnityEditor.Editor
    {
        // UI Elements
        private VisualElement _root;
        private Button _backButton;
        private Button _duplicateButton;
        private Button _deleteButton;
        private Button _editThemeButton;
        private TextField _themeNameField;
        private Toggle _editParametersToggle;
        private Label _parameterCountLabel;
        private VisualElement _parametersContainer;
        
        // Data references
        private Theme _theme;
        private SerializedProperty _nameProperty;
        private SerializedProperty _parameterValuesProperty;
        
        // Editor state
        private bool _editMode = false;
        private static Theme currentlyEditingTheme = null;
        private bool IsEditingThisTheme => currentlyEditingTheme == _theme;

        private void OnEnable()
        {
            _theme = (Theme)target;
            _nameProperty = serializedObject.FindProperty("themeName");
            _parameterValuesProperty = serializedObject.FindProperty("parameterValues");
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Load UXML template
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Smoothie/Scripts/Editor/Theming/UXML/ThemeEditor.uxml");
            
            if (visualTree == null)
            {
                Debug.LogError("Failed to load ThemeEditor.uxml. Please make sure the file exists at the specified path.");
                return new Label("Error loading UI template.");
            }
            
            _root = visualTree.Instantiate();
            
            // Load USS styles
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Smoothie/Scripts/Editor/Theming/USS/SmoothieEditor.uss");
                
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }
            
            // Get UI components
            _backButton = _root.Q<Button>("backButton");
            _editThemeButton = _root.Q<Button>("editThemeButton");
            _duplicateButton = _root.Q<Button>("duplicateButton");
            _deleteButton = _root.Q<Button>("deleteButton");
            _themeNameField = _root.Q<TextField>("themeName");
            _editParametersToggle = _root.Q<Toggle>("editParametersToggle");
            _parameterCountLabel = _root.Q<Label>("parameterCount");
            _parametersContainer = _root.Q<VisualElement>("parametersContainer");
            
            // Initialize values
            if (_themeNameField != null)
                _themeNameField.value = _theme.ThemeName;
                
            if (_editParametersToggle != null)
                _editParametersToggle.value = _editMode;
                
            if (_editThemeButton != null)
            {
                UpdateEditThemeButtonStyle();
                _editThemeButton.text = "Edit Theme";
            }
            
            // Set up icons
            SetupIcons();
            
            // Set up event handlers
            SetupEventHandlers();
            
            // Update parameter count display
            UpdateParameterCountDisplay();
            
            // Add IMGUI container for parameters
            if (_parametersContainer != null)
            {
                var imguiContainer = new IMGUIContainer(DrawParametersGUI);
                _parametersContainer.Add(imguiContainer);
            }
            
            return _root;
        }
        
        private void UpdateEditThemeButtonStyle()
        {
            if (_editThemeButton == null) return;
            
            if (IsEditingThisTheme)
            {
                _editThemeButton.AddToClassList("edit-theme-button-active");
            }
            else
            {
                _editThemeButton.RemoveFromClassList("edit-theme-button-active");
            }
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
        private void SetupIcons()
        {
            SmoothieUIAssets uiAssets = SmoothieUIAssets.Instance;
            if (uiAssets != null)
            {
                // Back button
                if (_backButton != null && uiAssets.backIcon != null)
                {
                    var backImage = new Image();
                    backImage.image = uiAssets.backIcon;
                    backImage.tintColor = uiAssets.backIconTint;
                    backImage.AddToClassList("button-icon");
                    _backButton.Add(backImage);
                }
                else if (_backButton != null)
                {
                    _backButton.text = "Back";
                }
                
                // Duplicate button
                if (_duplicateButton != null && uiAssets.duplicateIcon != null)
                {
                    var duplicateImage = new Image();
                    duplicateImage.image = uiAssets.duplicateIcon;
                    duplicateImage.tintColor = uiAssets.duplicateIconTint;
                    duplicateImage.AddToClassList("button-icon");
                    _duplicateButton.Add(duplicateImage);
                }
                else if (_duplicateButton != null)
                {
                    _duplicateButton.text = "Duplicate";
                }
                
                // Delete button
                if (_deleteButton != null && uiAssets.deleteIcon != null)
                {
                    var deleteImage = new Image();
                    deleteImage.image = uiAssets.deleteIcon;
                    deleteImage.tintColor = uiAssets.deleteIconTint;
                    deleteImage.AddToClassList("button-icon");
                    _deleteButton.Add(deleteImage);
                }
                else if (_deleteButton != null)
                {
                    _deleteButton.text = "Delete";
                }
            }
            else
            {
                if (_backButton != null) _backButton.text = "Back";
                if (_duplicateButton != null) _duplicateButton.text = "Duplicate";
                if (_deleteButton != null) _deleteButton.text = "Delete";
            }
        }
        
        private void SetupEventHandlers()
        {
            if (_backButton != null)
            {
                _backButton.clicked += () =>
                {
                    if (_theme.ParentDefinition != null)
                    {
                        var path = AssetDatabase.GetAssetPath(_theme.ParentDefinition);
                        var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                        Selection.activeObject = mainAsset;
                    }
                };
            }
            
            if (_duplicateButton != null)
            {
                _duplicateButton.clicked += DuplicateTheme;
            }
            
            if (_deleteButton != null)
            {
                _deleteButton.clicked += () =>
                {
                    if (EditorUtility.DisplayDialog("Delete Theme",
                        $"Are you sure you want to delete the theme '{_theme.ThemeName}'?",
                        "Delete", "Cancel"))
                    {
                        DeleteTheme();
                        var path = AssetDatabase.GetAssetPath(_theme.ParentDefinition);
                        var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                        Selection.activeObject = mainAsset;
                    }
                };
            }
            
            if (_editThemeButton != null)
            {
                _editThemeButton.clicked += () =>
                {
                    bool newState = !IsEditingThisTheme;
                    currentlyEditingTheme = newState ? _theme : null;
                    UpdateEditThemeButtonStyle();
                };
            }
            
            if (_themeNameField != null)
            {
                _themeNameField.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue != _theme.ThemeName)
                    {
                        _theme.ThemeName = evt.newValue;
                        _theme.name = evt.newValue;
                        EditorUtility.SetDirty(_theme);
                        AssetDatabase.SaveAssets();
                    }
                });
            }
            
            if (_editParametersToggle != null)
            {
                _editParametersToggle.RegisterValueChangedCallback(evt =>
                {
                    _editMode = evt.newValue;
                    UpdateParameterCountDisplay();
                    if (_parametersContainer != null)
                    {
                        var imgui = _parametersContainer.Q<IMGUIContainer>();
                        if (imgui != null)
                        {
                            imgui.MarkDirtyRepaint();
                        }
                    }
                });
            }
        }
        
        private void UpdateParameterCountDisplay()
        {
            if (_parameterCountLabel == null) return;
            
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
            
            _parameterCountLabel.text = countText;
        }
        
        private void DrawParametersGUI()
        {
            serializedObject.Update();
            
            // Check if parent definition exists
            if (_theme.ParentDefinition == null)
            {
                EditorGUILayout.HelpBox("This theme has no parent definition.", MessageType.Warning);
                if (GUILayout.Button("Back to Theme Definition"))
                {
                    var assetPath = AssetDatabase.GetAssetPath(_theme);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    Selection.activeObject = mainAsset;
                }
                return;
            }
            
            // Synchronize with definition
            _theme.SynchronizeWithDefinition();
            
            // Group the parameters
            EditorGUILayout.BeginVertical();
            
            // Build parameter list - only include valid parameters
            bool lastWasTitle = false;
            string currentSection = "";
            
            foreach (var defParam in _theme.ParentDefinition.Parameters)
            {
                // Find the parameter value in the theme
                SerializedProperty paramValue = null;
                for (int i = 0; i < _parameterValuesProperty.arraySize; i++)
                {
                    var valueProp = _parameterValuesProperty.GetArrayElementAtIndex(i);
                    var nameProperty = valueProp.FindPropertyRelative("ParameterName");
                    if (nameProperty != null && nameProperty.stringValue == defParam.Name)
                    {
                        paramValue = valueProp;
                        break;
                    }
                }
                
                // Skip invalid parameters
                if (paramValue == null)
                    continue;
                
                var enabledProperty = paramValue.FindPropertyRelative("Enabled");
                
                if (_editMode)
                {
                    // Edit mode - show all parameters with visibility toggles
                    if (defParam.Type == ThemeParameterType.Title)
                    {
                        if (!lastWasTitle)
                            EditorGUILayout.Space(5);
                            
                        EditorGUILayout.BeginHorizontal();
                        
                        bool newEnabled = EditorGUILayout.Toggle(enabledProperty.boolValue, GUILayout.Width(20));
                        if (newEnabled != enabledProperty.boolValue)
                        {
                            enabledProperty.boolValue = newEnabled;
                            serializedObject.ApplyModifiedProperties();
                            UpdateParameterCountDisplay();
                        }
                        
                        // Add GUI.enabled to visually show disabled state
                        bool oldEnabled = GUI.enabled;
                        if (!enabledProperty.boolValue)
                            GUI.enabled = false;
                            
                        EditorGUILayout.LabelField(defParam.Name, EditorStyles.boldLabel);
                        
                        // Restore GUI.enabled
                        GUI.enabled = oldEnabled;
                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(3);
                        lastWasTitle = true;
                        currentSection = defParam.Name;
                    }
                    else if (defParam.Type == ThemeParameterType.Divider)
                    {
                        if (_editMode)
                        {
                            EditorGUILayout.BeginHorizontal();
        
                            bool newEnabled = EditorGUILayout.Toggle(enabledProperty.boolValue, GUILayout.Width(20));
                            if (newEnabled != enabledProperty.boolValue)
                            {
                                enabledProperty.boolValue = newEnabled;
                                serializedObject.ApplyModifiedProperties();
                                UpdateParameterCountDisplay();
                            }
        
                            bool oldEnabled = GUI.enabled;
                            if (!enabledProperty.boolValue)
                                GUI.enabled = false;
            
                            // Рисуем улучшенный разделитель
                            Rect rect = EditorGUILayout.GetControlRect(false, 2);
                            DrawDivider(rect);
        
                            GUI.enabled = oldEnabled;
        
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(5);
                            lastWasTitle = false;
                        }
                        else if (enabledProperty.boolValue)
                        {
                            EditorGUILayout.Space(3);
                            Rect rect = EditorGUILayout.GetControlRect(false, 2);
                            DrawDivider(rect);
                            EditorGUILayout.Space(5);
                            lastWasTitle = false;
                        }
                    }
                    else
                    {
                        lastWasTitle = false;
                        EditorGUILayout.BeginHorizontal();
                        
                        bool newEnabled = EditorGUILayout.Toggle(enabledProperty.boolValue, GUILayout.Width(20));
                        if (newEnabled != enabledProperty.boolValue)
                        {
                            enabledProperty.boolValue = newEnabled;
                            serializedObject.ApplyModifiedProperties();
                            UpdateParameterCountDisplay();
                        }
                        
                        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
                        DrawParameterField(defParam, paramValue);
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(2);
                    }
                }
                else
                {
                    // Normal mode - only show enabled parameters
                    if (enabledProperty.boolValue)
                    {
                        if (defParam.Type == ThemeParameterType.Title)
                        {
                            if (!lastWasTitle)
                                EditorGUILayout.Space(5);
                                
                            EditorGUILayout.LabelField(defParam.Name, EditorStyles.boldLabel);
                            EditorGUILayout.Space(3);
                            lastWasTitle = true;
                            currentSection = defParam.Name;
                        }
                        else if (defParam.Type == ThemeParameterType.Divider)
                        {
                            Rect rect = EditorGUILayout.GetControlRect(false, 2);
                            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                            EditorGUILayout.Space(5);
                            lastWasTitle = false;
                        }
                        else
                        {
                            lastWasTitle = false;
                            DrawParameterField(defParam, paramValue);
                            EditorGUILayout.Space(2);
                        }
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawParameterField(ThemeParameter defParam, SerializedProperty paramValue)
        {
            switch (defParam.Type)
            {
                case ThemeParameterType.Color:
                    var colorProperty = paramValue.FindPropertyRelative("ColorValue");
                    colorProperty.colorValue = EditorGUILayout.ColorField(defParam.Name, colorProperty.colorValue);
                    break;
                    
                case ThemeParameterType.Float:
                    var floatProperty = paramValue.FindPropertyRelative("FloatValue");
                    floatProperty.floatValue = EditorGUILayout.FloatField(defParam.Name, floatProperty.floatValue);
                    break;
                    
                case ThemeParameterType.Vector3:
                    var vectorProperty = paramValue.FindPropertyRelative("VectorValue");
                    vectorProperty.vector3Value = EditorGUILayout.Vector3Field(defParam.Name, vectorProperty.vector3Value);
                    break;
            }
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
                Debug.LogWarning("Cannot duplicate theme without a parent definition.");
                return;
            }
            Object.DestroyImmediate(_theme, true);
            EditorUtility.SetDirty(_theme.ParentDefinition);
            AssetDatabase.SaveAssets();
        }
    }
}