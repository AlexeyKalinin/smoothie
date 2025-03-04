#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;

namespace Smoothie
{
    [CustomEditor(typeof(SmoothieElement))]
    public class SmoothieElementAnimationEditor : OdinEditor
    {
        private SmoothieElement _element;
        
        // Для отображения в режиме предпросмотра
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

            // Получаем имя стиля анимации
            string styleName = _element.ElementAnimationStyleName;
            if (string.IsNullOrEmpty(styleName)) return;
            
            // Находим стиль анимации
            var manager = FindObjectOfType<SmoothieRuntimeManager>()?.ElementAnimationManager;
            if (manager == null) return;
            
            _currentStyle = manager.dependentStyles.Find(s => s.styleName == styleName);
            if (_currentStyle == null) return;
            
            // Собираем все доступные события из стиля
            foreach (var eventDef in _currentStyle.eventDefinitions)
            {
                if (!string.IsNullOrEmpty(eventDef.eventKey))
                {
                    _availableEvents.Add(eventDef.eventKey);
                }
            }
            
            // Выбираем первое событие по умолчанию
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
            
            // Выпадающий список с доступными событиями
            EditorGUI.BeginChangeCheck();
            var eventIndex = _availableEvents.IndexOf(_selectedEvent);
            if (eventIndex < 0) eventIndex = 0;
            
            eventIndex = EditorGUILayout.Popup("Preview Event", eventIndex, _availableEvents.ToArray());
            if (EditorGUI.EndChangeCheck() && eventIndex >= 0 && eventIndex < _availableEvents.Count)
            {
                _selectedEvent = _availableEvents[eventIndex];
            }
            
            EditorGUILayout.Space(5);
            
            // Кнопка для воспроизведения анимации
            if (GUILayout.Button("Play Animation", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_selectedEvent))
                {
                    // В режиме воспроизведения вызываем метод воспроизведения анимации
                    if (Application.isPlaying)
                    {
                        _element.PlayAnimation(_selectedEvent);
                    }
                    else
                    {
                        // В режиме редактирования показываем сообщение
                        Debug.Log($"Animation Preview: Event '{_selectedEvent}' would play in Play Mode.");
                        
                        // Можно добавить предпросмотр в редакторе, если необходимо
                        // SimulateAnimationInEditor(_selectedEvent);
                    }
                }
            }
        }

        /*
        // Опциональный метод для симуляции анимации в редакторе
        private void SimulateAnimationInEditor(string eventKey)
        {
            if (_currentStyle == null) return;
            
            // Находим определение события
            if (_currentStyle.TryGetEventDefinition(eventKey, out var eventDef))
            {
                Debug.Log($"[Editor Preview] {eventKey} => duration={eventDef.duration}, useSpring={eventDef.useSpring}");
                
                // Можно добавить логику для симуляции анимации в режиме редактора
                // Например, временно изменять цвета или позиции элементов
            }
        }
        */
    }
}
#endif