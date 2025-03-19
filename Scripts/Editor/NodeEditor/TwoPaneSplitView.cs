// TwoPaneSplitView.cs - Removed cursor styling completely
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie.Editor
{
    public enum TwoPaneSplitViewOrientation
    {
        Horizontal,
        Vertical
    }

    public class TwoPaneSplitView : VisualElement
    {
        private VisualElement leftContainer;
        private VisualElement rightContainer;
        private VisualElement handle;
        private bool dragging;
        private Vector2 dragStartPos;
        private float dragStartDimension;

        public float fixedPaneInitialDimension { get; set; }
        public TwoPaneSplitViewOrientation orientation { get; }

        public TwoPaneSplitView(float initialDimension, float minDimension, TwoPaneSplitViewOrientation orientation)
        {
            this.orientation = orientation;
            this.fixedPaneInitialDimension = initialDimension;

            style.flexGrow = 1;
            style.flexDirection = orientation == TwoPaneSplitViewOrientation.Horizontal 
                ? FlexDirection.Row : FlexDirection.Column;

            leftContainer = new VisualElement();
            leftContainer.name = "left-container";
            leftContainer.style.flexShrink = 0;
            leftContainer.style.flexGrow = 0;
            
            if (orientation == TwoPaneSplitViewOrientation.Horizontal)
                leftContainer.style.width = initialDimension;
            else
                leftContainer.style.height = initialDimension;

            handle = new VisualElement();
            handle.name = "handle";
            handle.style.flexShrink = 0;
            
            if (orientation == TwoPaneSplitViewOrientation.Horizontal)
            {
                handle.style.width = 2;
                handle.RegisterCallback<MouseDownEvent>(OnMouseDown);
            }
            else
            {
                handle.style.height = 2;
                handle.RegisterCallback<MouseDownEvent>(OnMouseDown);
            }

            handle.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

            rightContainer = new VisualElement();
            rightContainer.name = "right-container";
            rightContainer.style.flexGrow = 1;

            Add(leftContainer);
            Add(handle);
            Add(rightContainer);

            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                dragging = true;
                dragStartPos = evt.mousePosition;
                
                if (orientation == TwoPaneSplitViewOrientation.Horizontal)
                    dragStartDimension = leftContainer.resolvedStyle.width;
                else
                    dragStartDimension = leftContainer.resolvedStyle.height;
                
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (dragging)
            {
                Vector2 delta = evt.mousePosition - dragStartPos;
                float dimension = dragStartDimension;
                
                if (orientation == TwoPaneSplitViewOrientation.Horizontal)
                {
                    dimension += delta.x;
                    leftContainer.style.width = Mathf.Max(0, dimension);
                }
                else
                {
                    dimension += delta.y;
                    leftContainer.style.height = Mathf.Max(0, dimension);
                }
                
                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (dragging && evt.button == 0)
            {
                dragging = false;
                evt.StopPropagation();
            }
        }

        public new void Add(VisualElement element)
        {
            if (leftContainer.childCount == 0)
                leftContainer.Add(element);
            else
                rightContainer.Add(element);
        }
    }
}