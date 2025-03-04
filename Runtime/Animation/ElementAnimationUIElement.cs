using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    /// <summary>
    /// Класс, представляющий UI элемент в событии анимации
    /// </summary>
    [System.Serializable]
    public class ElementAnimationUIElement
    {
        [HideInInspector]
        private ElementAnimationEventDependent parentEvent;

        public void SetParentEvent(ElementAnimationEventDependent parent)
        {
            parentEvent = parent;
        }

        [HorizontalGroup("Header")]
        [ValueDropdown("GetAllPossibleUIElements")]
        [LabelText("UI Element")]
        [LabelWidth(80)]
        public string uiElementKey;

        [HorizontalGroup("Header")]
        [Button("Add Action", ButtonSizes.Small), GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddActionButton()
        {
            AddNewAction();
        }

        [ListDrawerSettings(
            Expanded = true, 
            DraggableItems = true, 
            HideAddButton = true, 
            ShowItemCount = false,
            ShowPaging = false)]
        [LabelText("Actions")]
        public List<ElementAnimationAction> actions = new List<ElementAnimationAction>();

        private IEnumerable<string> GetAllPossibleUIElements()
        {
            if (parentEvent != null && parentEvent.parentStyle != null && 
                parentEvent.parentStyle.ManagerRef != null)
            {
                return parentEvent.parentStyle.ManagerRef.possibleUIElements;
            }
            return new[] { "<No UI Elements>" };
        }

        /// <summary>
        /// Обновляет родительскую ссылку у всех действий
        /// </summary>
        public void UpdateParentReferences()
        {
            foreach (var action in actions)
            {
                action.SetParentElement(this);
            }
        }

        /// <summary>
        /// Добавляет новое действие
        /// </summary>
        public void AddNewAction()
        {
            var newAction = new ElementAnimationAction();
            newAction.SetParentElement(this);
            actions.Add(newAction);
        }
    }
}