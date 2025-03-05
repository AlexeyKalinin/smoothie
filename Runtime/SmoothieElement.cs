using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
#endif

namespace Smoothie
{
    [ExecuteAlways]
    public class SmoothieElement : MonoBehaviour
    {
        // Animation style selection using dropdown
        [ValueDropdown("GetElementAnimationStyles")]
        [LabelText("Animation Style")]
        public string animationStyle;

        // Simple storage for UI element references
        [System.Serializable]
        public class UIElementReference
        {
            [HorizontalGroup, HideLabel]
            [ValueDropdown("GetPossibleUIElements")]
            public string elementKey;
            
            [HorizontalGroup, HideLabel]
            public GameObject targetObject;

            // Method to get UI element options from the animation manager
            #if UNITY_EDITOR
            public static string[] GetPossibleUIElements()
            {
                var manager = Object.FindObjectOfType<SmoothieRuntimeManager>()?.ElementAnimationManager;
                if (manager != null)
                {
                    return manager.possibleUIElements.ToArray();
                }
                return new string[] { "Background", "Text", "Icon" };
            }
            #endif
        }

        // Section for UI element references
        [Title("References")]
        [ListDrawerSettings(
            ShowPaging = false, 
            DraggableItems = false, 
            ShowIndexLabels = false, 
            ShowItemCount = false,
            HideAddButton = false,
            CustomAddFunction = "AddUIElement",
            Expanded = true)]
        [LabelText("UI Element References")]
        [SerializeField]
        private List<UIElementReference> uiElementReferences = new List<UIElementReference>();

        // Add new UI element reference
        private void AddUIElement()
        {
            // Add a new reference with the first available element key
            string[] keys = UIElementReference.GetPossibleUIElements();
            string key = keys.Length > 0 ? keys[0] : "Element";
            
            uiElementReferences.Add(new UIElementReference 
            { 
                elementKey = key, 
                targetObject = null 
            });
        }

        // Method for refreshing colors - needed for compatibility with SmoothieScreenView
        public void RefreshColor()
        {
            // This is a stub method to maintain compatibility
        }

        #region Animation Events
        // Example: on pointer enter
        public void OnPointerEnter()
        {
            PlayAnimation("PointerEnter");
        }

        // Example: on pointer exit
        public void OnPointerExit()
        {
            PlayAnimation("PointerExit");
        }

        // Example: on pointer click
        public void OnPointerClick()
        {
            PlayAnimation("PointerClick");
        }

        // Example: on button press
        public void OnPress()
        {
            PlayAnimation("Press");
        }

        // Example: on button release
        public void OnRelease()
        {
            PlayAnimation("Release");
        }
        #endregion

        // Воспроизводит анимацию для заданного события
        public void PlayAnimation(string eventKey)
        {
            var manager = SmoothieRuntimeManager.Instance?.ElementAnimationManager;
            if (manager == null) return;

            // Find the selected style
            var style = manager.dependentStyles
                .Find(s => s != null && s.styleName == animationStyle);
            if (style == null) return;

            // Find the event definition that handles this event key
            if (style.TryGetEventDefinition(eventKey, out var eventDef))
            {
                Debug.Log($"[SmoothieElement] Playing animation for event: {eventKey}");
                
                // Для каждого UI элемента в событии
                foreach (var uiElement in eventDef.uiElements)
                {
                    // Находим соответствующий UI элемент в нашем компоненте
                    var elementRef = GetReferenceByKey(uiElement.uiElementKey);
                    if (elementRef == null || elementRef.targetObject == null) continue;

                    var targetGraphic = elementRef.targetObject.GetComponent<Graphic>();
                    if (targetGraphic == null) continue;

                    // Для каждого действия анимации
                    foreach (var action in uiElement.actions)
                    {
                        // В упрощенной версии у нас есть только ChangeColor
                        if (action.actionType == AnimationActionType.ChangeColor)
                        {
                            PlayColorAnimation(targetGraphic, action);
                        }
                    }
                }
            }
        }

        private UIElementReference GetReferenceByKey(string key)
        {
            return uiElementReferences.FirstOrDefault(r => r.elementKey == key);
        }

        #region Animation Methods
        private void PlayColorAnimation(Graphic targetGraphic, SmoothieElementAnimationAction action)
        {
            if (targetGraphic == null) return;

            // Get color from theme
            Color targetColor = Color.white;
            if (!string.IsNullOrEmpty(action.colorKey) && SmoothieRuntimeManager.Instance != null)
            {
                targetColor = SmoothieRuntimeManager.Instance.GetAnimatedColor(action.colorKey);
            }

            // Запускаем анимацию цвета
            Tween.Color(targetGraphic, targetColor, action.duration);
        }
        #endregion

#if UNITY_EDITOR
        // Compatibility property for editors
        public string ElementAnimationStyleName
        {
            get => animationStyle;
            set => animationStyle = value;
        }

        private IEnumerable<string> GetElementAnimationStyles()
        {
            var manager = SmoothieRuntimeManager.Instance?.ElementAnimationManager;
            if (manager == null) return new List<string>();

            return manager.dependentStyles
                .Where(s => s != null && !string.IsNullOrEmpty(s.styleName))
                .Select(s => s.styleName);
        }
#endif
    }
}