using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace Smoothie
{
    [System.Serializable]
    public class ElementAnimationEventDependent
    {
        [HideInInspector]
        public SmoothieElementAnimationStyle parentStyle;

        [HideInInspector]
        public string DisplayTitle
        {
            get
            {
                if (eventKeys == null || eventKeys.Count == 0)
                    return "No Events";
                return string.Join(", ", eventKeys);
            }
        }

        [ValueDropdown("GetAvailableEvents")]
        [LabelText("Event Keys")]
        [ListDrawerSettings(
            DraggableItems = false,
            Expanded = true,
            HideAddButton = false,
            HideRemoveButton = false, // Показываем стандартные крестики у eventKeys
            ShowIndexLabels = false,
            ShowItemCount = true
        )]
        public List<string> eventKeys = new List<string>();

        [LabelText("")] // убираем подпись "UI Elements"
        [ListDrawerSettings(
            DraggableItems = false,
            Expanded = true,
            HideAddButton = true,   // скрываем стандартную "+"
            HideRemoveButton = true,// скрываем крестики
            ShowIndexLabels = false,
            ShowItemCount = true
        )]
        public List<ElementAnimationUIElement> uiElements = new List<ElementAnimationUIElement>();

        private IEnumerable<string> GetAvailableEvents()
        {
            if (parentStyle != null && parentStyle.ManagerRef != null)
            {
                return parentStyle.ManagerRef.possibleEvents;
            }
            return new[] { "Click", "Hover" };
        }

        public void SetParentStyle(SmoothieElementAnimationStyle style)
        {
            parentStyle = style;
        }

        public void UpdateParentReferences()
        {
            foreach (var element in uiElements)
            {
                element.SetParentEvent(this);
                element.UpdateParentReferences();
            }
        }

        public bool HandlesEvent(string eventKey)
        {
            return eventKeys != null && eventKeys.Contains(eventKey);
        }
    }
}
