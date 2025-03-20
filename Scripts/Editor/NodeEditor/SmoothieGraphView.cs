using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie.Editor.NodeEditor
{
    public class SmoothieGraphView : GraphView
    {
        protected SmoothieGraphEditorWindow _editorWindow;
        private SmoothieGraph _graph;
        private List<SmoothieNodeView> _copyBuffer = new List<SmoothieNodeView>();
        
        public SmoothieGraphView(SmoothieGraphEditorWindow window)
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
            
            // Add label to show view is active
            var debugLabel = new Label("Smoothie Graph View");
            debugLabel.style.position = Position.Absolute;
            debugLabel.style.top = 10;
            debugLabel.style.left = 10;
            debugLabel.style.color = Color.white;
            Add(debugLabel);
            
            // Register for context menu
            this.RegisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuPopulateEvent);
            
            // Register selection changed callback
            this.RegisterCallback<KeyDownEvent>(OnKeyDown);
            
            // Register mouse up for selection tracking
            RegisterCallback<MouseUpEvent>(_ => {
                // Small delay to ensure selection is up to date
                schedule.Execute(() => UpdateSelection());
            });
            
            // Hook into the graphViewChanged event
            graphViewChanged += OnGraphViewChanged;
            
            // Set up clipboard callbacks
            serializeGraphElements = SerializeSelectedElements;
            canPasteSerializedData = CanPaste;
            unserializeAndPaste = UnserializeAndPaste;
        }
        
        private string SerializeSelectedElements(IEnumerable<GraphElement> elements)
        {
            // Store references to selected nodes
            _copyBuffer.Clear();
            foreach (var element in elements)
            {
                if (element is SmoothieNodeView nodeView)
                {
                    _copyBuffer.Add(nodeView);
                }
            }
            
            // Return a simple string to indicate data is in the buffer
            return "SMOOTHIE_CLIPBOARD_DATA";
        }
        
        private bool CanPaste(string data)
        {
            // Check if we have valid data in the buffer
            return data == "SMOOTHIE_CLIPBOARD_DATA" && _copyBuffer.Count > 0;
        }
        
        private void UnserializeAndPaste(string operationName, string data)
        {
            // Clear selection to prepare for new nodes
            ClearSelection();
            
            // Get the mouse position or center of view for paste location
            Vector2 pastePosition = GetMousePositionInContentViewContainer();
            
            // If we couldn't get mouse position, use center of view with offset
            if (pastePosition == Vector2.zero)
            {
                pastePosition = -viewTransform.position + new Vector3(viewTransform.scale.x * 100, viewTransform.scale.y * 100);
            }
            
            // Create offset for positioning new nodes
            Vector2 offset = pastePosition;
            if (_copyBuffer.Count > 0)
            {
                // Calculate average position of copied nodes
                Vector2 averagePosition = Vector2.zero;
                foreach (var nodeView in _copyBuffer)
                {
                    averagePosition += nodeView.container.position;
                }
                averagePosition /= _copyBuffer.Count;
                
                // Calculate offset from average position to paste position
                offset = pastePosition - averagePosition;
            }
            
            // Create new nodes from copied nodes
            foreach (var nodeView in _copyBuffer)
            {
                Vector2 newPosition = nodeView.container.position + offset + new Vector2(20, 20);
                CreateContainerFromCopy(nodeView.container, newPosition);
            }
            
            // Save the changes
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
        }
        
        private Vector2 GetMousePositionInContentViewContainer()
        {
            // Get current mouse position
            Vector2 mousePosition = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            
            // Convert to content view space
            return contentViewContainer.WorldToLocal(mousePosition);
        }
        
        private SmoothieNodeView CreateContainerFromCopy(SmoothieContainer originalContainer, Vector2 position)
        {
            if (_graph == null)
                return null;
                
            // Create a new container as a copy
            SmoothieContainer newContainer = ScriptableObject.CreateInstance<SmoothieContainer>();
            
            // Copy properties
            newContainer.title = originalContainer.title + " (Copy)";
            newContainer.headerColor = originalContainer.headerColor;
            newContainer.position = position;
            
            // Name the asset
            newContainer.name = newContainer.title;
            
            // Add to graph
            _graph.containers.Add(newContainer);
            
            // Save as sub-asset
            AssetDatabase.AddObjectToAsset(newContainer, _graph);
            
            // Create the visual node
            SmoothieNodeView newNodeView = CreateNodeView(newContainer);
            
            // Select the new node
            AddToSelection(newNodeView);
            
            return newNodeView;
        }
        
        protected void UpdateSelection()
        {
            if (selection.Count == 1 && selection[0] is UnityEditor.Experimental.GraphView.Node nodeView)
            {
                _editorWindow.UpdateSelection(nodeView);
            }
            else
            {
                _editorWindow.UpdateSelection(null);
            }
        }
        
        // Handle element changes
        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            // Handle elements being removed
            if (changes.elementsToRemove != null)
            {
                foreach (var element in changes.elementsToRemove)
                {
                    if (element is SmoothieNodeView nodeView)
                    {
                        // Remove container from graph
                        if (_graph != null && nodeView.container != null)
                        {
                            // Remove from graph
                            _graph.containers.Remove(nodeView.container);
                            
                            // Remove any connections
                            foreach (var container in _graph.containers)
                            {
                                container.connections.RemoveAll(c => c.targetContainer == nodeView.container);
                            }
                            
                            // Remove asset
                            AssetDatabase.RemoveObjectFromAsset(nodeView.container);
                            EditorUtility.SetDirty(_graph);
                        }
                    }
                }
                
                AssetDatabase.SaveAssets();
            }
            
            return changes; // Return the original changes to continue with the standard behavior
        }
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            // Handle keyboard shortcuts
            if (evt.ctrlKey || evt.commandKey)  // Support both Windows/Linux (Ctrl) and macOS (Cmd)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.C:  // Copy
                        // Let the built-in copy handler work
                        break;
                        
                    case KeyCode.X:  // Cut
                        // Let the built-in cut handler work
                        break;
                        
                    case KeyCode.V:  // Paste
                        // Let the built-in paste handler work
                        break;
                        
                    case KeyCode.D:  // Duplicate
                        // Get selected nodes
                        var selectedNodes = selection.OfType<SmoothieNodeView>().ToList();
                        if (selectedNodes.Count > 0)
                        {
                            // Clear selection
                            ClearSelection();
                            
                            // Duplicate each node
                            foreach (var nodeView in selectedNodes)
                            {
                                DuplicateNode(nodeView);
                            }
                            
                            evt.StopPropagation();
                        }
                        break;
                }
            }
            else if (evt.keyCode == KeyCode.Delete)  // Delete
            {
                List<GraphElement> elementsToRemove = new List<GraphElement>();
                
                foreach (var element in selection)
                {
                    if (element is SmoothieNodeView nodeView)
                    {
                        // Add the node to elements to remove
                        elementsToRemove.Add(nodeView);
                    }
                }
                
                // Actually delete the elements
                DeleteElements(elementsToRemove);
                
                evt.StopPropagation();
            }
        }
        
        private void DuplicateNode(SmoothieNodeView nodeView)
        {
            if (_graph == null)
                return;
                
            // Calculate new position slightly offset from original
            Vector2 newPosition = nodeView.container.position + new Vector2(20, 20);
            
            // Create the duplicate
            CreateContainerFromCopy(nodeView.container, newPosition);
            
            // Save the asset
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
        }
        
        // Override GetCompatiblePorts
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return new List<Port>(); // No connections needed
        }
        
        // Use a different name for the context menu handler to avoid conflict
        protected virtual void OnContextualMenuPopulateEvent(ContextualMenuPopulateEvent evt)
        {
            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            evt.menu.AppendAction("Add Container", _ => CreateNewContainer(nodePosition));
            
            // Add paste option if there's data in the clipboard
            if (_copyBuffer.Count > 0)
            {
                evt.menu.AppendAction("Paste", _ => {
                    UnserializeAndPaste("Paste", "SMOOTHIE_CLIPBOARD_DATA");
                });
            }
        }
        
        public void PopulateView(SmoothieGraph smoothieGraph)
        {
            _graph = smoothieGraph;
            
            // Clear the view
            graphElements.ForEach(element => RemoveElement(element));
            
            // If graph is null, create test nodes for debugging
            if (_graph == null)
            {
                // Add a debug container
                var testContainer = ScriptableObject.CreateInstance<SmoothieContainer>();
                testContainer.title = "Debug Node";
                testContainer.position = new Vector2(100, 100);
                
                CreateNodeView(testContainer);
                return;
            }
            
            // Restore view state
            viewTransform.position = _graph.viewPosition;
            viewTransform.scale = new Vector3(_graph.zoomScale, _graph.zoomScale, 1);
            
            // Add nodes for each container
            foreach (var container in _graph.containers)
            {
                CreateNodeView(container);
            }
            
            // If no nodes exist, create one for testing
            if (_graph.containers.Count == 0)
            {
                CreateNewContainer(new Vector2(300, 200));
            }
        }
        
        private void CreateNewContainer(Vector2 position)
        {
            if (_graph == null)
                return;
                
            SmoothieContainer container = ScriptableObject.CreateInstance<SmoothieContainer>();
            
            // Set initial values
            container.title = "Container";
            container.name = container.title;
            container.position = position;
            
            // Add to graph asset
            _graph.containers.Add(container);
            
            // Save as sub-asset
            AssetDatabase.AddObjectToAsset(container, _graph);
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
            
            // Create the visual node
            SmoothieNodeView nodeView = CreateNodeView(container);
            
            // Select the new node
            ClearSelection();
            AddToSelection(nodeView);
        }
        
        private SmoothieNodeView CreateNodeView(SmoothieContainer container)
        {
            SmoothieNodeView nodeView = new SmoothieNodeView(container);
            nodeView.SetPosition(new Rect(container.position, new Vector2(200, 150)));
            
            // Add the node to the view
            AddElement(nodeView);
            return nodeView;
        }
    }
}