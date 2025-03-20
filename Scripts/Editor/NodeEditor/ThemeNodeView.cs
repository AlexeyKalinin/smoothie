using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie.Editor.NodeEditor
{
    public class ThemeNodeView : Node
    {
        public Theme theme { get; private set; }
        private VisualElement _parametersContainer;
        private const float NODE_WIDTH = 300f; // Width of the node
        private const float NODE_MIN_HEIGHT = 200f; // Minimum height
        
        public ThemeNodeView(Theme themeData)
        {
            theme = themeData;
            viewDataKey = $"theme_{theme.GetInstanceID()}";
            
            // Set up node appearance
            SetupNodeAppearance();
            
            // Refresh expanded state
            RefreshExpandedState();
        }
        
        private void SetupNodeAppearance()
        {
            // Set fixed width for the node
            style.width = NODE_WIDTH;
            style.minHeight = NODE_MIN_HEIGHT;
            
            // Set node background and styling
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Add border
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Add rounded corners
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            
            // Add shadow
            style.unitySliceLeft = 5;
            style.unitySliceRight = 5;
            style.unitySliceTop = 5;
            style.unitySliceBottom = 5;
            
            // Set title
            title = theme.ThemeName;
            
            // Style the title
            var titleContainer = this.Q("title");
            if (titleContainer != null)
            {
                // Use color from theme if appropriate
                titleContainer.style.backgroundColor = GetHeaderColorForTheme(theme);
                titleContainer.style.height = 30;
                
                // Round the top corners of title to match the node
                titleContainer.style.borderTopLeftRadius = 5;
                titleContainer.style.borderTopRightRadius = 5;
                
                var titleLabel = this.Q("title-label") as Label;
                if (titleLabel != null)
                {
                    titleLabel.style.paddingLeft = 10;
                    titleLabel.style.paddingRight = 10;
                    titleLabel.style.paddingTop = 5;
                    titleLabel.style.paddingBottom = 5;
                    titleLabel.style.fontSize = 14;
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.color = Color.white;
                }
            }
            
            // Style for selected state
            RegisterCallback<AttachToPanelEvent>(_ => {
                this.OnSelectionChange(selection => {
                    if (selection)
                    {
                        // When selected, use a more prominent border
                        style.borderLeftWidth = 2;
                        style.borderRightWidth = 2;
                        style.borderTopWidth = 2;
                        style.borderBottomWidth = 2;
                        style.borderLeftColor = new Color(0.2f, 0.6f, 0.9f, 1f);
                        style.borderRightColor = new Color(0.2f, 0.6f, 0.9f, 1f);
                        style.borderTopColor = new Color(0.2f, 0.6f, 0.9f, 1f);
                        style.borderBottomColor = new Color(0.2f, 0.6f, 0.9f, 1f);
                    }
                    else
                    {
                        // Return to normal border when not selected
                        style.borderLeftWidth = 1;
                        style.borderRightWidth = 1;
                        style.borderTopWidth = 1;
                        style.borderBottomWidth = 1;
                        style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                        style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                        style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                        style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                    }
                });
            });
            
            // Add content container
            _parametersContainer = new VisualElement();
            _parametersContainer.style.paddingTop = 10;
            _parametersContainer.style.paddingBottom = 10;
            _parametersContainer.style.paddingLeft = 10;
            _parametersContainer.style.paddingRight = 10;
            
            // Add theme parameters display
            RefreshParametersDisplay();
            
            mainContainer.Add(_parametersContainer);
        }
        
        private void RefreshParametersDisplay()
        {
            _parametersContainer.Clear();
            
            if (theme?.ParentDefinition == null)
                return;
                
            // Display only enabled parameters, similar to what's shown in the theme editor
            foreach (var parameter in theme.ParentDefinition.Parameters)
            {
                var paramValue = theme.GetParameterValue(parameter.Name);
                if (paramValue == null || !theme.IsParameterEnabled(paramValue.ParameterName))
                    continue;
                    
                if (parameter.Type == ThemeParameterType.Title)
                {
                    var titleLabel = new Label(parameter.Name);
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.fontSize = 12;
                    titleLabel.style.marginTop = 8;
                    titleLabel.style.marginBottom = 4;
                    _parametersContainer.Add(titleLabel);
                }
                else if (parameter.Type == ThemeParameterType.Divider)
                {
                    var divider = new VisualElement();
                    divider.style.height = 1;
                    divider.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    divider.style.marginTop = 5;
                    divider.style.marginBottom = 5;
                    _parametersContainer.Add(divider);
                }
                else if (parameter.Type == ThemeParameterType.Space)
                {
                    var space = new VisualElement();
                    space.style.height = 10;
                    _parametersContainer.Add(space);
                }
                else
                {
                    // Create editable parameter field based on type
                    var parameterElement = CreateEditableParameterElement(parameter, paramValue);
                    if (parameterElement != null)
                        _parametersContainer.Add(parameterElement);
                }
            }
        }
        
        private VisualElement CreateEditableParameterElement(ThemeParameter parameter, ThemeParameterValue value)
        {
            // Create editable visualization of the parameter value
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;
            
            // Label section
            var label = new Label(parameter.Name);
            label.style.width = 80;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 11;
            container.Add(label);
            
            // Field section - takes remaining space
            var valueContainer = new VisualElement();
            valueContainer.style.flexGrow = 1;
            
            switch (parameter.Type)
            {
                case ThemeParameterType.Color:
                    // Create a color field for color params
                    var colorField = new ColorField();
                    colorField.style.minHeight = 22;
                    colorField.style.flexGrow = 1;
                    colorField.value = value.ColorValue;
                    colorField.RegisterValueChangedCallback(evt => {
                        value.ColorValue = evt.newValue;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    });
                    valueContainer.Add(colorField);
                    break;
                    
                case ThemeParameterType.Float:
                    // Create a float field for float params
                    var floatField = new FloatField();
                    floatField.style.minHeight = 18;
                    floatField.style.flexGrow = 1;
                    floatField.value = value.FloatValue;
                    floatField.RegisterValueChangedCallback(evt => {
                        value.FloatValue = evt.newValue;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    });
                    valueContainer.Add(floatField);
                    break;
                    
                case ThemeParameterType.Vector3:
                    // For vector3, create a simpler UI that can fit in the node width
                    var vectorContainer = new VisualElement();
                    vectorContainer.style.flexDirection = FlexDirection.Row;
                    vectorContainer.style.flexGrow = 1;
                    
                    // X component
                    var xField = new FloatField("X");
                    xField.style.flexGrow = 1;
                    xField.style.marginRight = 2;
                    xField.style.minHeight = 18;
                    xField.value = value.VectorValue.x;
                    xField.RegisterValueChangedCallback(evt => {
                        Vector3 newVec = value.VectorValue;
                        newVec.x = evt.newValue;
                        value.VectorValue = newVec;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    });
                    
                    // Y component
                    var yField = new FloatField("Y");
                    yField.style.flexGrow = 1;
                    yField.style.marginRight = 2;
                    yField.style.minHeight = 18;
                    yField.value = value.VectorValue.y;
                    yField.RegisterValueChangedCallback(evt => {
                        Vector3 newVec = value.VectorValue;
                        newVec.y = evt.newValue;
                        value.VectorValue = newVec;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    });
                    
                    // Z component
                    var zField = new FloatField("Z");
                    zField.style.flexGrow = 1;
                    zField.style.minHeight = 18;
                    zField.value = value.VectorValue.z;
                    zField.RegisterValueChangedCallback(evt => {
                        Vector3 newVec = value.VectorValue;
                        newVec.z = evt.newValue;
                        value.VectorValue = newVec;
                        EditorUtility.SetDirty(theme);
                        AssetDatabase.SaveAssets();
                    });
                    
                    // Add fields to container
                    vectorContainer.Add(xField);
                    vectorContainer.Add(yField);
                    vectorContainer.Add(zField);
                    valueContainer.Add(vectorContainer);
                    break;
            }
            
            container.Add(valueContainer);
            return container;
        }
        
        private Color GetHeaderColorForTheme(Theme theme)
        {
            // Try to find a characteristic color from the theme
            var colorParam = theme.ParentDefinition.Parameters
                .FirstOrDefault(p => p.Type == ThemeParameterType.Color);
                
            if (colorParam != null)
            {
                var paramValue = theme.GetParameterValue(colorParam.Name);
                if (paramValue != null && theme.IsParameterEnabled(paramValue.ParameterName))
                {
                    return paramValue.ColorValue;
                }
            }
            
            // Default color based on theme name
            // Generate a stable color from theme name hash
            int hash = theme.ThemeName.GetHashCode();
            float hue = (hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.6f, 0.8f);
        }
        
        public void UpdateViewFromModel()
        {
            // Update title
            title = theme.ThemeName;
            
            // Update header color
            var titleContainer = this.Q("title");
            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor = GetHeaderColorForTheme(theme);
            }
            
            // Refresh parameters
            RefreshParametersDisplay();
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            
            // Store the position for persistence
            if (theme != null)
            {
                ThemeNodePositionData positionData = ThemeNodePositionData.GetOrCreateForDefinition(theme.ParentDefinition);
                if (positionData != null)
                {
                    positionData.SetPositionForTheme(theme, newPos.position);
                    EditorUtility.SetDirty(positionData);
                }
            }
        }
        
        // Extension method for selection handling
        private void OnSelectionChange(System.Action<bool> callback)
        {
            this.RegisterCallback<MouseDownEvent>(evt => {
                if (evt.button == 0)
                {
                    schedule.Execute(() => {
                        // Execute callback in the next frame when selection is updated
                        callback(selected);
                    });
                }
            });
            
            this.RegisterCallback<FocusInEvent>(_ => {
                callback(selected);
            });
            
            this.RegisterCallback<FocusOutEvent>(_ => {
                callback(selected);
            });
        }
    }
}