// SmoothieNodeView.cs - Updated to sync title with asset name
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie.Editor
{
    public class SmoothieNodeView : Node
    {
        public SmoothieContainer container;
        
        private Label titleLabel;
        private VisualElement titleContainer;
        
        public SmoothieNodeView(SmoothieContainer containerData)
        {
            container = containerData;
            viewDataKey = container.GetInstanceID().ToString();
            
            // Configure node appearance
            SetupNodeAppearance();
            
            // Refresh the node's visual state
            RefreshExpandedState();
        }
        
        private void SetupNodeAppearance()
        {
            // Set node background color
            style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Set title and header color
            title = container.title;
            
            // Get the title container and apply styles
            titleContainer = this.Q("title");
            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor = container.headerColor;
                
                titleLabel = this.Q("title-label") as Label;
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
            
            // Add content element
            var contentContainer = new VisualElement();
            contentContainer.style.paddingTop = 10;
            contentContainer.style.paddingBottom = 10;
            contentContainer.style.paddingLeft = 10;
            contentContainer.style.paddingRight = 10;
            
            var debugLabel = new Label("Container: " + container.name);
            contentContainer.Add(debugLabel);
            
            mainContainer.Add(contentContainer);
        }
        
        public void UpdateViewFromModel()
        {
            // Update title
            title = container.title;
            
            // Update header color
            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor = container.headerColor;
            }
            
            // Also update the asset name to match the title
            if (container.name != container.title)
            {
                container.name = container.title;
                EditorUtility.SetDirty(container);
                AssetDatabase.SaveAssets();
            }
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            
            // Update container position
            if (container != null)
            {
                container.position = newPos.position;
            }
        }
    }
}