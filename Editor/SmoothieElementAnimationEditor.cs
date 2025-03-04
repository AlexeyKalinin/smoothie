#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Smoothie
{
    [CustomEditor(typeof(SmoothieElement))]
    public class SmoothieElementAnimationEditor : OdinEditor
    {
        private SmoothieElement _element;
        
        // For preview display
        private string _selectedEvent;
        private List<string> _availableEvents = new List<string>();
        private SmoothieElementAnimationStyle _currentStyle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _element = target as SmoothieElement;
            UpdateAvailableEvents();
        }

        private void UpdateAvailableEvents()
        {
            _availableEvents.Clear();
            if (_element == null) return;

            // Get animation style name
            string styleName = _element.ElementAnimationStyleName;
            if (string.IsNullOrEmpty(styleName)) return;
            
            // Find animation style
            var manager = FindObjectOfType<SmoothieRuntimeManager>()?.ElementAnimationManager;
            if (manager == null) return;
            
            _currentStyle = manager.dependentStyles.Find(s => s.styleName == styleName);
            if (_currentStyle == null) return;
            
            // Collect all available events from style event definitions
            var eventSet = new HashSet<string>();
            foreach (var eventDef in _currentStyle.eventDefinitions)
            {
                if (eventDef.eventKeys != null && eventDef.eventKeys.Count > 0)
                {
                    foreach (var eventKey in eventDef.eventKeys)
                    {
                        eventSet.Add(eventKey);
                    }
                }
            }
            
            _availableEvents.AddRange(eventSet);
            
            // Select first event by default
            if (_availableEvents.Count > 0 && string.IsNullOrEmpty(_selectedEvent))
            {
                _selectedEvent = _availableEvents[0];
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (_element == null || _availableEvents.Count == 0) return;
            
            EditorGUILayout.Space(10);
            SirenixEditorGUI.DrawThickHorizontalSeparator();
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Event dropdown
            EditorGUI.BeginChangeCheck();
            var eventIndex = _availableEvents.IndexOf(_selectedEvent);
            if (eventIndex < 0) eventIndex = 0;
            
            eventIndex = EditorGUILayout.Popup("Preview Event", eventIndex, _availableEvents.ToArray());
            if (EditorGUI.EndChangeCheck() && eventIndex >= 0 && eventIndex < _availableEvents.Count)
            {
                _selectedEvent = _availableEvents[eventIndex];
            }
            
            EditorGUILayout.Space(5);
            
            // Play animation button
            if (GUILayout.Button("Play Animation", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_selectedEvent))
                {
                    // In play mode, call animation method
                    if (Application.isPlaying)
                    {
                        _element.PlayAnimation(_selectedEvent);
                    }
                    else
                    {
                        // In edit mode, show message
                        Debug.Log($"Animation Preview: Event '{_selectedEvent}' would play in Play Mode.");
                    }
                }
            }
        }
    }
}
#endif