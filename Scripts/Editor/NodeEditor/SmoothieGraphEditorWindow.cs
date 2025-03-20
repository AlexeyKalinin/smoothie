using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Smoothie.Editor.NodeEditor
{
    public class SmoothieGraphEditorWindow : EditorWindow
    {
        public enum EditorMode { Containers, Themes }
        
        private EditorMode _currentMode = EditorMode.Containers;
        private SmoothieGraph _graph;
        private ThemeDefinition _themeDefinition;
        
        private SmoothieGraphView _containerGraphView;
        private ThemeGraphView _themeGraphView;
        private VisualElement _activeGraphView;
        
        private InspectorView _inspectorView;
        private VisualElement _resizer;
        private bool _isDragging;
        private float _inspectorWidth = 250;
        private VisualElement _contentContainer;
        
        [MenuItem("Window/Smoothie/Graph Editor")]
        public static void OpenWindow()
        {
            SmoothieGraphEditorWindow window = GetWindow<SmoothieGraphEditorWindow>();
            window.titleContent = new GUIContent("Smoothie Graph");
            window.minSize = new Vector2(800, 600);
        }
        
        public void CreateGUI()
        {
            // Import USS (styles)
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Smoothie/Scripts/Editor/NodeEditor/Styles.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            
            // Create root container
            var rootContainer = new VisualElement();
            rootContainer.style.flexGrow = 1;
            rootContainer.style.flexDirection = FlexDirection.Column;
            rootVisualElement.Add(rootContainer);
            
            // Add toolbar
            var toolbar = new Toolbar();
            
            // File menu dropdown
            var fileButton = new ToolbarButton(() => ShowFileMenu()) { text = "File" };
            toolbar.Add(fileButton);
            
            // Add mode selector
            var modeSelector = new ToolbarMenu { text = "Mode" };
            modeSelector.menu.AppendAction("Containers", a => SwitchMode(EditorMode.Containers), 
                a => _currentMode == EditorMode.Containers ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            modeSelector.menu.AppendAction("Themes", a => SwitchMode(EditorMode.Themes), 
                a => _currentMode == EditorMode.Themes ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            toolbar.Add(modeSelector);
            
            // Add spacer to push toggle to right side
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);
            
            // Add toggle for inspector visibility
            var inspectorToggle = new ToolbarToggle { value = true, text = "Inspector" };
            inspectorToggle.RegisterValueChangedCallback(evt => {
                ToggleInspector(evt.newValue);
            });
            toolbar.Add(inspectorToggle);
            
            rootContainer.Add(toolbar);
            
            // Create main content area
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.flexDirection = FlexDirection.Row;
            rootContainer.Add(_contentContainer);
            
            // Create graph views
            _containerGraphView = new SmoothieGraphView(this);
            _containerGraphView.style.flexGrow = 1;
            
            _themeGraphView = new ThemeGraphView(this);
            _themeGraphView.style.flexGrow = 1;
            
            // Set initial view
            SetActiveGraphView(_currentMode);
            
            // Create resizer
            _resizer = new VisualElement();
            _resizer.name = "resizer";
            _resizer.style.width = 5;
            _resizer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            // Removed cursor styling
            _resizer.RegisterCallback<MouseDownEvent>(OnResizerMouseDown);
            rootVisualElement.RegisterCallback<MouseMoveEvent>(OnResizerMouseMove);
            rootVisualElement.RegisterCallback<MouseUpEvent>(OnResizerMouseUp);
            _contentContainer.Add(_resizer);
            
            // Create inspector view
            _inspectorView = new InspectorView();
            _inspectorView.style.width = _inspectorWidth;
            _inspectorView.style.minWidth = 200;
            _contentContainer.Add(_inspectorView);
            
            // Open graph when selected
            EditorApplication.delayCall += () => 
            {
                if (Selection.activeObject is SmoothieGraph selectedGraph)
                {
                    OpenGraph(selectedGraph);
                }
                else if (Selection.activeObject is ThemeDefinition selectedDefinition)
                {
                    OpenThemeDefinition(selectedDefinition);
                }
            };
        }
        
        private void SwitchMode(EditorMode mode)
        {
            if (_currentMode == mode)
                return;
                
            _currentMode = mode;
            SetActiveGraphView(mode);
            
            // Update window title
            UpdateWindowTitle();
        }
        
        private void SetActiveGraphView(EditorMode mode)
        {
            // Remove current view
            if (_activeGraphView != null)
                _contentContainer.Remove(_activeGraphView);
                
            // Set new active view
            _activeGraphView = mode == EditorMode.Containers ? (VisualElement)_containerGraphView : _themeGraphView;
            
            // Add to container
            _contentContainer.Insert(0, _activeGraphView);
        }
        
        private void ToggleInspector(bool visible)
        {
            if (_inspectorView != null)
            {
                if (visible)
                {
                    _inspectorView.style.width = _inspectorWidth;
                    _inspectorView.style.display = DisplayStyle.Flex;
                    _resizer.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _inspectorWidth = _inspectorView.style.width.value.value;
                    _inspectorView.style.display = DisplayStyle.None;
                    _resizer.style.display = DisplayStyle.None;
                }
            }
        }
        
        private void OnResizerMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                _isDragging = true;
                evt.StopPropagation();
            }
        }
        
        private void OnResizerMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging)
            {
                Vector2 mousePos = evt.mousePosition;
                float windowWidth = rootVisualElement.worldBound.width;
                
                // Adjust inspector width based on mouse position
                float newWidth = windowWidth - mousePos.x;
                
                // Clamp to reasonable values
                newWidth = Mathf.Clamp(newWidth, 200, windowWidth * 0.7f);
                
                _inspectorView.style.width = newWidth;
                evt.StopPropagation();
            }
        }
        
        private void OnResizerMouseUp(MouseUpEvent evt)
        {
            if (_isDragging && evt.button == 0)
            {
                _isDragging = false;
                _inspectorWidth = _inspectorView.style.width.value.value;
                evt.StopPropagation();
            }
        }
        
        private void ShowFileMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            if (_currentMode == EditorMode.Containers)
            {
                menu.AddItem(new GUIContent("New Graph"), false, CreateNewGraph);
                menu.AddItem(new GUIContent("Open Graph"), false, OpenGraphDialog);
                menu.AddItem(new GUIContent("Save Graph"), false, SaveGraph);
            }
            else // Themes mode
            {
                menu.AddItem(new GUIContent("Open Theme Definition"), false, OpenThemeDefinitionDialog);
                menu.AddItem(new GUIContent("Save Theme Definition"), false, SaveThemeDefinition);
            }
            
            menu.ShowAsContext();
        }
        
        private void CreateNewGraph()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Smoothie Graph", "New Smoothie Graph", "asset", "Create a new Smoothie Graph");
            if (string.IsNullOrEmpty(path))
                return;
                
            SmoothieGraph newGraph = CreateInstance<SmoothieGraph>();
            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.SaveAssets();
            OpenGraph(newGraph);
        }
        
        private void OpenGraphDialog()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Open Smoothie Graph", "Assets", new[] { "Smoothie Graph", "asset" });
            if (string.IsNullOrEmpty(path))
                return;
                
            // Convert from absolute path to relative path
            path = "Assets" + path.Substring(Application.dataPath.Length);
            SmoothieGraph loadedGraph = AssetDatabase.LoadAssetAtPath<SmoothieGraph>(path);
            if (loadedGraph != null)
                OpenGraph(loadedGraph);
        }
        
        private void OpenThemeDefinitionDialog()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Open Theme Definition", "Assets", new[] { "Theme Definition", "asset" });
            if (string.IsNullOrEmpty(path))
                return;
                
            // Convert from absolute path to relative path
            path = "Assets" + path.Substring(Application.dataPath.Length);
            ThemeDefinition loadedDefinition = AssetDatabase.LoadAssetAtPath<ThemeDefinition>(path);
            if (loadedDefinition != null)
                OpenThemeDefinition(loadedDefinition);
        }
        
        public void OpenGraph(SmoothieGraph smoothieGraph)
        {
            _graph = smoothieGraph;
            _containerGraphView.PopulateView(smoothieGraph);
            
            SwitchMode(EditorMode.Containers);
            UpdateWindowTitle();
        }
        
        public void OpenThemeDefinition(ThemeDefinition definition)
        {
            _themeDefinition = definition;
            _themeGraphView.PopulateWithThemes(definition);
            
            SwitchMode(EditorMode.Themes);
            UpdateWindowTitle();
        }
        
        private void UpdateWindowTitle()
        {
            string windowTitle = "Smoothie Graph";
            
            if (_currentMode == EditorMode.Containers && _graph != null)
            {
                windowTitle += $" - Container: {_graph.name}";
            }
            else if (_currentMode == EditorMode.Themes && _themeDefinition != null)
            {
                windowTitle += $" - Theme: {_themeDefinition.name}";
            }
            
            titleContent = new GUIContent(windowTitle);
        }
        
        private void SaveGraph()
        {
            if (_graph == null)
                return;
                
            // Save container positions and connections
            foreach (var nodeView in _containerGraphView.nodes.ToList())
            {
                if (nodeView is SmoothieNodeView smoothieNodeView)
                {
                    SmoothieContainer container = smoothieNodeView.container;
                    container.position = smoothieNodeView.GetPosition().position;
                }
            }
            
            // Save view state
            _graph.viewPosition = _containerGraphView.viewTransform.position;
            _graph.zoomScale = _containerGraphView.viewTransform.scale.x;
            
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
        }
        
        private void SaveThemeDefinition()
        {
            if (_themeDefinition == null)
                return;
                
            // Position data is saved automatically when nodes are moved
            EditorUtility.SetDirty(_themeDefinition);
            AssetDatabase.SaveAssets();
        }
        
        public void UpdateSelection(UnityEditor.Experimental.GraphView.Node nodeView)
        {
            if (nodeView is SmoothieNodeView containerNode)
            {
                _inspectorView.UpdateContainerSelection(containerNode);
            }
            else if (nodeView is ThemeNodeView themeNode)
            {
                _inspectorView.UpdateThemeSelection(themeNode);
            }
            else
            {
                _inspectorView.ClearSelection();
            }
        }
    }
    
    // Enhanced InspectorView to handle both container and theme nodes
    public class InspectorView : VisualElement
    {
        private UnityEditor.Editor _currentEditor;
        private IMGUIContainer _inspectorContainer;
        private VisualElement _contentContainer;
        
        private SmoothieNodeView _currentContainerNode;
        private ThemeNodeView _currentThemeNode;
        
        public InspectorView()
        {
            // Create header
            var header = new VisualElement();
            header.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            header.style.paddingTop = 5;
            header.style.paddingBottom = 5;
            
            var label = new Label("Inspector");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = 14;
            label.style.paddingLeft = 5;
            header.Add(label);
            
            Add(header);
            
            // Create content container
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            Add(_contentContainer);
            
            // Add IMGUI container for editor
            _inspectorContainer = new IMGUIContainer(() => {
                if (_currentEditor != null)
                {
                    EditorGUI.BeginChangeCheck();
                    
                    EditorGUIUtility.labelWidth = 120; // Make labels fit better
                    _currentEditor.OnInspectorGUI();
                    
                    // If any changes were made, update the node view
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (_currentContainerNode != null)
                        {
                            EditorUtility.SetDirty(_currentContainerNode.container);
                            _currentContainerNode.UpdateViewFromModel();
                        }
                        else if (_currentThemeNode != null)
                        {
                            EditorUtility.SetDirty(_currentThemeNode.theme);
                            _currentThemeNode.UpdateViewFromModel();
                        }
                    }
                }
            });
            _contentContainer.Add(_inspectorContainer);
            
            // Style the inspector
            style.flexGrow = 0;
            style.flexShrink = 0;
        }
        
        public void UpdateContainerSelection(SmoothieNodeView nodeView)
        {
            ClearSelection();
            
            _currentContainerNode = nodeView;
            
            if (nodeView != null)
            {
                _currentEditor = UnityEditor.Editor.CreateEditor(nodeView.container);
                _contentContainer.Add(_inspectorContainer);
            }
        }
        
        public void UpdateThemeSelection(ThemeNodeView nodeView)
        {
            ClearSelection();
    
            _currentThemeNode = nodeView;
    
            if (nodeView != null)
            {
                // Create instance of our custom editor for Theme, fully qualifying the types
                _currentEditor = UnityEditor.Editor.CreateEditor(nodeView.theme, typeof(ThemeNodeEditor));
                _contentContainer.Add(_inspectorContainer);
        
                // Add a title label
                var titleLabel = new Label($"Editing Theme: {nodeView.theme.ThemeName}");
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 14;
                titleLabel.style.marginBottom = 10;
                _contentContainer.Insert(0, titleLabel);
            }
        }
        
        public void ClearSelection()
        {
            _currentContainerNode = null;
            _currentThemeNode = null;
            
            if (_currentEditor != null)
            {
                Object.DestroyImmediate(_currentEditor);
                _currentEditor = null;
            }
            
            _contentContainer.Clear();
            _contentContainer.Add(new Label("No selection"));
        }
    }
}