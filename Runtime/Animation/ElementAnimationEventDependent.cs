using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [System.Serializable]
    public class ElementAnimationEventDependent
    {
        [HideInInspector]
        public SmoothieElementAnimationStyle parentStyle;

        public void SetParentStyle(SmoothieElementAnimationStyle style)
        {
            parentStyle = style;
        }

        [BoxGroup("Event")]
        [HorizontalGroup("Event/Info")]
        [ValueDropdown("GetAllPossibleEvents")]
        [LabelText("Event")]
        [LabelWidth(50)]
        public string eventKey;

        [HorizontalGroup("Event/Info")]
        [LabelText("Duration")]
        [LabelWidth(60)]
        [Range(0.01f, 5f)]
        public float duration = 0.3f;

        [HorizontalGroup("Event/Info")]
        [LabelText("Spring")]
        [LabelWidth(50)]
        public bool useSpring = false;

        [BoxGroup("Event")]
        [HorizontalGroup("Event/Actions")]
        [Button("Add UI Element", ButtonSizes.Small), GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddUIElementButton()
        {
            AddNewUIElement();
        }

        [FoldoutGroup("UI Elements", expanded: true)]
        [ListDrawerSettings(
            Expanded = true, 
            DraggableItems = true, 
            HideAddButton = true,
            ShowItemCount = false,
            ShowPaging = false)]
        public List<ElementAnimationUIElement> uiElements = new List<ElementAnimationUIElement>();

        private IEnumerable<string> GetAllPossibleEvents()
        {
            if (parentStyle != null && parentStyle.ManagerRef != null)
            {
                return parentStyle.ManagerRef.possibleEvents;
            }
            return new[] { "<No Manager>" };
        }

        /// <summary>
        /// Обновляет родительские ссылки у всех UI элементов
        /// </summary>
        public void UpdateParentReferences()
        {
            foreach (var element in uiElements)
            {
                element.SetParentEvent(this);
                element.UpdateParentReferences();
            }
        }

        /// <summary>
        /// Добавляет новый UI элемент
        /// </summary>
        public void AddNewUIElement()
        {
            var newElement = new ElementAnimationUIElement();
            newElement.SetParentEvent(this);
            
            // Установим первый доступный тип элемента, если есть
            if (parentStyle != null && parentStyle.ManagerRef != null && 
                parentStyle.ManagerRef.possibleUIElements.Count > 0)
            {
                newElement.uiElementKey = parentStyle.ManagerRef.possibleUIElements[0];
            }
            
            uiElements.Add(newElement);
        }

        /// <summary>
        /// Находит UI элемент по ключу
        /// </summary>
        public bool TryGetUIElement(string key, out ElementAnimationUIElement element)
        {
            element = uiElements.Find(e => e.uiElementKey == key);
            return element != null;
        }
    }
}