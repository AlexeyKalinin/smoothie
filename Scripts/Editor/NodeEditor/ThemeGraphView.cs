using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie.Editor.NodeEditor
{
    public class ThemeGraphView : GraphView
    {
        private SmoothieGraphEditorWindow _editorWindow;
        private ThemeDefinition _themeDefinition;
        private ThemeNodePositionData _positionData;
        
        public ThemeGraphView(SmoothieGraphEditorWindow window)
        {
            _editorWindow = window;
            
            // Set up styles and functionality
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());
            
            // Set the view's size and position
            style.width = new StyleLength(StyleKeyword.Auto);
            style.height = new StyleLength(StyleKeyword.Auto);
            style.flexGrow = 1;
            
            // Add grid background
            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
            
            // Register for context menu
            this.RegisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuPopulateEvent);
            
            // Register mouse up for selection tracking
            RegisterCallback<MouseUpEvent>(_ => {
                schedule.Execute(() => UpdateSelection());
            });
            
            // Hook into the graphViewChanged event
            graphViewChanged += OnGraphViewChanged;
        }
        
        public void PopulateWithThemes(ThemeDefinition definition)
        {
            _themeDefinition = definition;
            graphElements.ForEach(element => RemoveElement(element));
            
            if (definition == null)
                return;
            
            // Load position data
            _positionData = ThemeNodePositionData.GetOrCreateForDefinition(definition);
                
            // Add nodes for each theme
            var themes = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(definition))
                .OfType<Theme>()
                .ToList();
                
            if (themes.Count == 0)
            {
                // No themes yet, log info
                Debug.Log("No themes found in definition.");
                return;
            }
                
            // Create nodes for themes
            foreach (var theme in themes)
            {
                CreateThemeNodeView(theme);
            }
        }
        private ThemeNodeView CreateThemeNodeView(Theme theme)
        {
            ThemeNodeView nodeView = new ThemeNodeView(theme);
            
            // Position the node
            Vector2 position = Vector2.zero;
            
            // Try to get stored position
            if (_positionData != null)
            {
                Vector2? storedPosition = _positionData.GetPositionForTheme(theme);
                if (storedPosition.HasValue)
                {
                    position = storedPosition.Value;
                }
                else
                {
                    // Generate position if none exists
                    position = GenerateNewPosition();
                    _positionData.SetPositionForTheme(theme, position);
                    EditorUtility.SetDirty(_positionData);
                }
            }
            else
            {
                position = GenerateNewPosition();
            }
            
            nodeView.SetPosition(new Rect(position, new Vector2(300, 200)));
            
            AddElement(nodeView);
            return nodeView;
        }
        
        private Vector2 GenerateNewPosition()
        {
            // Generate a new position that avoids overlapping with existing nodes
            float baseX = 100;
            float baseY = 100;
            
            // Get all existing positions
            var existingNodes = nodes.ToList().OfType<ThemeNodeView>().ToList();
            
            if (existingNodes.Count == 0)
                return new Vector2(baseX, baseY);
                
            // Find the rightmost node
            float maxX = existingNodes.Max(n => n.GetPosition().xMax);
            
            // Place new node to the right with some spacing
            return new Vector2(maxX + 50, baseY);
        }
        
        private void OnContextualMenuPopulateEvent(ContextualMenuPopulateEvent evt)
        {
            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            
            if (_themeDefinition != null)
            {
                evt.menu.AppendAction("Add New Theme", _ => CreateNewTheme(nodePosition));
            }
        }
        
        private void CreateNewTheme(Vector2 position)
        {
            if (_themeDefinition == null)
                return;
                
            // Create new theme
            Theme newTheme = ScriptableObject.CreateInstance<Theme>();
            newTheme.ThemeName = "New Theme";
            newTheme.Initialize(_themeDefinition);
            newTheme.name = newTheme.ThemeName;
            
            // Store position
            if (_positionData != null)
            {
                _positionData.SetPositionForTheme(newTheme, position);
                EditorUtility.SetDirty(_positionData);
            }
            
            // Add to asset
            AssetDatabase.AddObjectToAsset(newTheme, _themeDefinition);
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
            
            // Create node view
            ThemeNodeView nodeView = CreateThemeNodeView(newTheme);
            
            // Select the node
            ClearSelection();
            AddToSelection(nodeView);
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            // Handle element removal
            if (changes.elementsToRemove != null)
            {
                foreach (var element in changes.elementsToRemove)
                {
                    if (element is ThemeNodeView themeNodeView)
                    {
                        if (EditorUtility.DisplayDialog("Delete Theme", 
                            $"Are you sure you want to delete the theme '{themeNodeView.theme.ThemeName}'?", 
                            "Delete", "Cancel"))
                        {
                            // Remove the asset
                            Object.DestroyImmediate(themeNodeView.theme, true);
                            EditorUtility.SetDirty(_themeDefinition);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            // Cancel deletion
                            changes.elementsToRemove.Remove(element);
                        }
                    }
                }
            }
            
            return changes;
        }
        
        private void UpdateSelection()
        {
            if (selection.Count == 1)
            {
                if (selection[0] is ThemeNodeView themeNodeView)
                {
                    _editorWindow.UpdateSelection(themeNodeView);
                }
            }
            else
            {
                _editorWindow.UpdateSelection(null);
            }
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return new List<Port>(); // No connections needed for theme nodes
        }
    }
}