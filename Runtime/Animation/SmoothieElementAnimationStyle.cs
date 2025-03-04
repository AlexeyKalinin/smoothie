using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    public class SmoothieElementAnimationStyle : ScriptableObject
    {
        // Скрываем от инспектора, но храним ссылку на менеджера
        [HideInInspector]
        public SmoothieElementAnimationManager ManagerRef;

        [BoxGroup("Style Info", showLabel: false)]
        [HorizontalGroup("Style Info/Name")]
        [SerializeField, OnValueChanged("UpdateAssetName")]
        [LabelText("Style Name")]
        [LabelWidth(80)]
        private string _styleName = "Untitled";

        public string styleName
        {
            get => _styleName;
            set => _styleName = value;
        }

        [HorizontalGroup("Style Info/Name")]
        [Button("Add Event", ButtonSizes.Small), GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddEventButton()
        {
            AddNewEvent();
        }

        [HorizontalGroup("Style Info/Name", Width = 120)]
        [Button("Remove Style", ButtonSizes.Small), GUIColor(0.9f, 0.3f, 0.3f)]
        private void RemoveStyleButton()
        {
            RemoveThisStyle();
        }

        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = true,
            HideAddButton = true,
            ShowItemCount = true)]
        [LabelText("Event Definitions")]
        public List<ElementAnimationEventDependent> eventDefinitions = new List<ElementAnimationEventDependent>();

        // Красная кнопка для удаления этого стиля
#if UNITY_EDITOR
        private void RemoveThisStyle()
        {
            if (ManagerRef != null)
            {
                if (EditorUtility.DisplayDialog("Remove Style", 
                    $"Are you sure you want to remove style '{_styleName}'?", 
                    "Yes, Remove", "Cancel"))
                {
                    ManagerRef.dependentStyles.Remove(this);
                    AssetDatabase.RemoveObjectFromAsset(this);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                Debug.LogWarning($"Can't remove style {name} because ManagerRef is null.");
            }
        }
#endif

        [Button("Add Event", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddNewEvent()
        {
            var newEvent = new ElementAnimationEventDependent();
            newEvent.SetParentStyle(this);
            
            // Если есть доступные события, выбираем первое
            if (ManagerRef != null && ManagerRef.possibleEvents.Count > 0)
            {
                newEvent.eventKey = ManagerRef.possibleEvents[0];
            }
            
            eventDefinitions.Add(newEvent);
        }

        public bool TryGetEventDefinition(string key, out ElementAnimationEventDependent def)
        {
            def = eventDefinitions.FirstOrDefault(e => e.eventKey == key);
            return (def != null);
        }

        private void UpdateAssetName()
        {
            name = _styleName;
        }

        private void OnEnable()
        {
            ForceParentStyleForAll();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ForceParentStyleForAll();
        }
#endif

        private void ForceParentStyleForAll()
        {
            for (int i = 0; i < eventDefinitions.Count; i++)
            {
                if (eventDefinitions[i] != null)
                {
                    eventDefinitions[i].SetParentStyle(this);
                    eventDefinitions[i].UpdateParentReferences();
                }
            }
        }
    }
}