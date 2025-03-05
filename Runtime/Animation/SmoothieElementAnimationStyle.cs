using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    [HideMonoScript]
    public class SmoothieElementAnimationStyle : ScriptableObject
    {
        [HideInInspector]
        public SmoothieElementAnimationManager ManagerRef;

        [BoxGroup("Style", showLabel: false)]
        [HorizontalGroup("Style/Row", Width = 0.8f)]
        [SerializeField, OnValueChanged("UpdateAssetName")]
        [LabelText("Style Name"), LabelWidth(80)]
        private string _styleName = "New Style";

        // Удаляем (или комментируем) кнопку добавления нового события:
        // [HorizontalGroup("Style/Row")]
        // [Button("+", ButtonSizes.Small), GUIColor(0.4f, 0.8f, 0.4f)]
        // private void AddEventButton()
        // {
        //     AddNewEvent();
        // }

        [HorizontalGroup("Style/Row")]
        [Button("✕", ButtonSizes.Small), GUIColor(0.9f, 0.3f, 0.3f)]
        private void RemoveStyleButton()
        {
            RemoveThisStyle();
        }

        [ListDrawerSettings(
            DraggableItems = false,   // запрет на перетаскивание
            Expanded = true,          // сразу раскрыт
            HideAddButton = false,    // показывать кнопку "+" от Odin, если нужно
            HideRemoveButton = true,  // скрыть стандартные крестики "x"
            ShowIndexLabels = false,
            ShowItemCount = true
        )]
        [LabelText(" ")]
        public List<SmoothieElementAnimationEventDependent> eventDefinitions = new List<SmoothieElementAnimationEventDependent>();

        public string styleName
        {
            get => _styleName;
            set => _styleName = value;
        }

#if UNITY_EDITOR
        private void RemoveEventDefinition(SmoothieElementAnimationEventDependent eventDef)
        {
            if (eventDefinitions.Contains(eventDef))
            {
                eventDefinitions.Remove(eventDef);
            }
        }

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

        // Метод для добавления нового события можно оставить, если он используется где-то ещё
        public void AddNewEvent()
        {
            var newEvent = new SmoothieElementAnimationEventDependent();
            newEvent.SetParentStyle(this);
            
            // Если нужны события по умолчанию
            if (ManagerRef != null && ManagerRef.possibleEvents.Count > 0)
            {
                newEvent.eventKeys.Add(ManagerRef.possibleEvents[0]);
            }
            
            eventDefinitions.Add(newEvent);
        }

        public bool TryGetEventDefinition(string eventKey, out SmoothieElementAnimationEventDependent def)
        {
            def = eventDefinitions.FirstOrDefault(e => e.HandlesEvent(eventKey));
            return (def != null);
        }

        private void UpdateAssetName()
        {
            name = _styleName;
        }

        private void OnEnable()
        {
            UpdateParentReferences();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateParentReferences();
        }
#endif

        private void UpdateParentReferences()
        {
            foreach (var eventDef in eventDefinitions)
            {
                if (eventDef != null)
                {
                    eventDef.SetParentStyle(this);
                    eventDef.UpdateParentReferences();
                }
            }
        }
    }
}
