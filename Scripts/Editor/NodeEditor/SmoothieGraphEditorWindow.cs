// SmoothieGraphEditorWindow.cs - Fix for StyleCursor error
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Smoothie.Editor
{
    public class SmoothieGraphEditorWindow : EditorWindow
    {
        private SmoothieGraph graph;
        private SmoothieGraphView graphView;
        private InspectorView inspectorView;
        private VisualElement resizer;
        private bool isDragging;
        private float inspectorWidth = 250;
        private VisualElement contentContainer;
        
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
            contentContainer = new VisualElement();
            contentContainer.style.flexGrow = 1;
            contentContainer.style.flexDirection = FlexDirection.Row;
            rootContainer.Add(contentContainer);
            
            // Create graph view
            graphView = new SmoothieGraphView(this);
            graphView.style.flexGrow = 1;
            contentContainer.Add(graphView);
            
            // Create resizer
            resizer = new VisualElement();
            resizer.name = "resizer";
            resizer.style.width = 5;
            resizer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            // Removed cursor styling
            resizer.RegisterCallback<MouseDownEvent>(OnResizerMouseDown);
            rootVisualElement.RegisterCallback<MouseMoveEvent>(OnResizerMouseMove);
            rootVisualElement.RegisterCallback<MouseUpEvent>(OnResizerMouseUp);
            contentContainer.Add(resizer);
            
            // Create inspector view
            inspectorView = new InspectorView();
            inspectorView.style.width = inspectorWidth;
            inspectorView.style.minWidth = 200;
            contentContainer.Add(inspectorView);
            
            // Open graph when selected
            EditorApplication.delayCall += () => 
            {
                OpenGraph(Selection.activeObject as SmoothieGraph);
            };
        }
        
        private void ToggleInspector(bool visible)
        {
            if (inspectorView != null)
            {
                if (visible)
                {
                    inspectorView.style.width = inspectorWidth;
                    inspectorView.style.display = DisplayStyle.Flex;
                    resizer.style.display = DisplayStyle.Flex;
                }
                else
                {
                    inspectorWidth = inspectorView.style.width.value.value;
                    inspectorView.style.display = DisplayStyle.None;
                    resizer.style.display = DisplayStyle.None;
                }
            }
        }
        
        private void OnResizerMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                isDragging = true;
                evt.StopPropagation();
            }
        }
        
        private void OnResizerMouseMove(MouseMoveEvent evt)
        {
            if (isDragging)
            {
                Vector2 mousePos = evt.mousePosition;
                float windowWidth = rootVisualElement.worldBound.width;
                
                // Adjust inspector width based on mouse position
                float newWidth = windowWidth - mousePos.x;
                
                // Clamp to reasonable values
                newWidth = Mathf.Clamp(newWidth, 200, windowWidth * 0.7f);
                
                inspectorView.style.width = newWidth;
                evt.StopPropagation();
            }
        }
        
        private void OnResizerMouseUp(MouseUpEvent evt)
        {
            if (isDragging && evt.button == 0)
            {
                isDragging = false;
                inspectorWidth = inspectorView.style.width.value.value;
                evt.StopPropagation();
            }
        }
        
        private void ShowFileMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Graph"), false, CreateNewGraph);
            menu.AddItem(new GUIContent("Open Graph"), false, OpenGraphDialog);
            menu.AddItem(new GUIContent("Save Graph"), false, SaveGraph);
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
        
        public void OpenGraph(SmoothieGraph smoothieGraph)
        {
            if (graphView == null)
                return;
                
            graph = smoothieGraph;
            graphView.PopulateView(graph);
            
            if (smoothieGraph != null)
                titleContent = new GUIContent($"Smoothie Graph - {graph.name}");
            else
                titleContent = new GUIContent("Smoothie Graph");
        }
        
        private void SaveGraph()
        {
            if (graph == null)
                return;
                
            // Save container positions and connections
            foreach (var nodeView in graphView.nodes.ToList())
            {
                if (nodeView is SmoothieNodeView smoothieNodeView)
                {
                    SmoothieContainer container = smoothieNodeView.container;
                    container.position = smoothieNodeView.GetPosition().position;
                }
            }
            
            // Save view state
            graph.viewPosition = graphView.viewTransform.position;
            graph.zoomScale = graphView.viewTransform.scale.x;
            
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
        }
        
        public void UpdateSelection(SmoothieNodeView nodeView)
        {
            inspectorView.UpdateSelection(nodeView);
        }
    }
    
    // Inspector View class in SmoothieGraphEditorWindow.cs
public class InspectorView : VisualElement
{
    private UnityEditor.Editor containerEditor;
    private IMGUIContainer inspectorContainer;
    private VisualElement contentContainer;
    private SmoothieNodeView currentNodeView;
    
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
        contentContainer = new VisualElement();
        contentContainer.style.flexGrow = 1;
        contentContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        Add(contentContainer);
        
        // Add IMGUI container for editor
        inspectorContainer = new IMGUIContainer(() => {
            if (containerEditor != null && currentNodeView != null)
            {
                EditorGUI.BeginChangeCheck();
                
                EditorGUIUtility.labelWidth = 120; // Make labels fit better
                containerEditor.OnInspectorGUI();
                
                // If any changes were made, update the node view
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(containerEditor.target);
                    currentNodeView.UpdateViewFromModel();
                }
            }
        });
        contentContainer.Add(inspectorContainer);
        
        // Style the inspector
        style.flexGrow = 0;
        style.flexShrink = 0;
    }
    
    public void UpdateSelection(SmoothieNodeView nodeView)
    {
        // Store current node view reference
        currentNodeView = nodeView;
        
        // Remove previous editor
        if (containerEditor != null)
        {
            Object.DestroyImmediate(containerEditor);
            containerEditor = null;
        }
        
        // Update container
        contentContainer.Clear();
        
        if (nodeView != null)
        {
            // Create new editor for container
            containerEditor = UnityEditor.Editor.CreateEditor(nodeView.container);
            contentContainer.Add(inspectorContainer);
        }
        else
        {
            // No selection
            contentContainer.Add(new Label("No container selected"));
        }
    }
}
}