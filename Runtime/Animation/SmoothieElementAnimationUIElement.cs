using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [System.Serializable]
    public class SmoothieElementAnimationUIElement
    {
        [HideInInspector]
        private SmoothieElementAnimationEventDependent parentEvent;

        public void SetParentEvent(SmoothieElementAnimationEventDependent parent)
        {
            parentEvent = parent;
        }

        [HideLabel]
        [ValueDropdown("GetUIElements")]
        public string uiElementKey;

        [ListDrawerSettings(
            ShowPaging = false,
            ShowItemCount = false,
            HideAddButton = false,
            DraggableItems = false,
            Expanded = true)]
        [LabelText(" ")]
        public List<SmoothieElementAnimationAction> actions = new List<SmoothieElementAnimationAction>();

        private IEnumerable<string> GetUIElements()
        {
            if (parentEvent != null && parentEvent.parentStyle != null &&
                parentEvent.parentStyle.ManagerRef != null)
            {
                return parentEvent.parentStyle.ManagerRef.possibleUIElements;
            }
            return new[] { "Background", "Text", "Icon" };
        }

        public void UpdateParentReferences()
        {
            foreach (var action in actions)
            {
                action.SetParentElement(this);
            }
        }
    }
}