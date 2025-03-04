#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Smoothie
{
    [CustomEditor(typeof(SmoothieElement))]
    public class SmoothieElementEditor : OdinEditor
    {
        private SmoothieElement _element;
        private SmoothieElementAnimationManager _animationManager;
        
        // For animation preview
        private string _selectedEvent;
        private List<string> _availableEvents = new List<string>();
        private SmoothieElementAnimationStyle _currentStyle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _element = target as SmoothieElement;
            
            // Get references to manager
            var runtimeManager = Object.FindObjectOfType<SmoothieRuntimeManager>();
            if (runtimeManager != null)
            {
                _animationManager = runtimeManager.ElementAnimationManager;
            }
            
            UpdateAvailableEvents();
        }

        private void UpdateAvailableEvents()
        {
            _availableEvents.Clear();
            if (_element == null) return;

            // Get style name
            string styleName = _element.animationStyle;
            if (string.IsNullOrEmpty(styleName) || _animationManager == null) return;
            
            // Find animation style
            _currentStyle = _animationManager.dependentStyles.Find(s => s.styleName == styleName);
            if (_currentStyle == null) return;
            
            // Collect available events
            foreach (var eventDef in _currentStyle.eventDefinitions)
            {
                if (!string.IsNullOrEmpty(eventDef.eventKey))
                {
                    _availableEvents.Add(eventDef.eventKey);
                }
            }
            
            // Select first event by default
            if (_availableEvents.Count > 0 && string.IsNullOrEmpty(_selectedEvent))
            {
                _selectedEvent = _availableEvents[0];
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw the default inspector provided by Odin
            base.OnInspectorGUI();
            
            // Draw animation preview section if possible
            if (_element != null && _availableEvents.Count > 0)
            {
                DrawAnimationPreviewSection();
            }
        }

        private void DrawAnimationPreviewSection()
        {
            EditorGUILayout.Space(10);
            SirenixEditorGUI.DrawThickHorizontalSeparator();
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);
            
            // Event dropdown
            EditorGUI.BeginChangeCheck();
            var eventIndex = _availableEvents.IndexOf(_selectedEvent);
            if (eventIndex < 0) eventIndex = 0;
            
            eventIndex = EditorGUILayout.Popup("Event", eventIndex, _availableEvents.ToArray());
            if (EditorGUI.EndChangeCheck() && eventIndex >= 0 && eventIndex < _availableEvents.Count)
            {
                _selectedEvent = _availableEvents[eventIndex];
            }
            
            // Play button
            if (GUILayout.Button("Play Animation", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_selectedEvent))
                {
                    if (Application.isPlaying)
                    {
                        _element.PlayAnimation(_selectedEvent);
                    }
                    else
                    {
                        Debug.Log($"Animation Preview: Event '{_selectedEvent}' would play in Play Mode.");
                    }
                }
            }
        }
    }
}
#endif