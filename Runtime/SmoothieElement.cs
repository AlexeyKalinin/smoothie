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
                return new string[] { "Background", "Text", "Icon", "Border", "Shadow" };
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
            // The previous version used this to update Image colors based on theme
            // Since we've removed those fields, this is now just a placeholder
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

            // Find the event definition with the specified key
            if (style.TryGetEventDefinition(eventKey, out var eventDef))
            {
                Debug.Log($"[SmoothieElement] {eventKey} => duration={eventDef.duration}, useSpring={eventDef.useSpring}");
                
                // Для каждого UI элемента в событии
                foreach (var uiElement in eventDef.uiElements)
                {
                    // Находим соответствующий UI элемент в нашем компоненте
                    var elementRef = GetReferenceByKey(uiElement.uiElementKey);
                    if (elementRef == null || elementRef.targetObject == null) continue;

                    var targetRect = elementRef.targetObject.GetComponent<RectTransform>();
                    var targetGraphic = elementRef.targetObject.GetComponent<Graphic>();
                    if (targetRect == null && targetGraphic == null) continue;

                    // Для каждого действия анимации
                    foreach (var action in uiElement.actions)
                    {
                        // Выполняем действие в зависимости от его типа
                        switch (action.actionType)
                        {
                            case AnimationActionType.ChangeColor:
                                PlayColorAnimation(targetGraphic, action);
                                break;
                            case AnimationActionType.Move:
                                PlayMoveAnimation(targetRect, action);
                                break;
                            case AnimationActionType.Scale:
                                PlayScaleAnimation(targetRect, action);
                                break;
                            case AnimationActionType.Rotate:
                                PlayRotateAnimation(targetRect, action);
                                break;
                            case AnimationActionType.Fade:
                                PlayFadeAnimation(targetGraphic, action);
                                break;
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
        private void PlayColorAnimation(Graphic targetGraphic, ElementAnimationAction action)
        {
            if (targetGraphic == null) return;

            // Определяем цвет в зависимости от настроек
            Color targetColor = action.targetColor;
            if (action.useThemeColor && SmoothieRuntimeManager.Instance != null)
            {
                targetColor = SmoothieRuntimeManager.Instance.GetAnimatedColor(action.colorKey);
            }

            // Настройки анимации
            var tweenSettings = new TweenSettings(
                action.duration,
                action.useSpring ? Ease.OutBack : action.ease,
                startDelay: action.delay);

            // Запускаем анимацию цвета
            Tween.Color(targetGraphic, targetColor, tweenSettings);
        }

        private void PlayMoveAnimation(RectTransform targetRect, ElementAnimationAction action)
        {
            if (targetRect == null) return;

            // Определяем целевую позицию
            Vector2 targetPosition = action.relativeMovement
                ? (Vector2)targetRect.anchoredPosition + action.moveDirection
                : action.moveDirection;

            // Настройки анимации
            var tweenSettings = new TweenSettings(
                action.duration,
                action.useSpring ? Ease.OutBack : action.ease,
                startDelay: action.delay);

            // Запускаем анимацию движения
            Tween.UIAnchoredPosition(targetRect, targetPosition, tweenSettings);
        }

        private void PlayScaleAnimation(RectTransform targetRect, ElementAnimationAction action)
        {
            if (targetRect == null) return;

            // Определяем целевой масштаб
            Vector3 targetScale = action.uniformScale
                ? Vector3.one * action.scaleFactor
                : new Vector3(action.targetScale.x, action.targetScale.y, 1f);

            // Настройки анимации
            var tweenSettings = new TweenSettings(
                action.duration,
                action.useSpring ? Ease.OutBack : action.ease,
                startDelay: action.delay);

            // Запускаем анимацию масштабирования
            Tween.LocalScale(targetRect, targetScale, tweenSettings);
        }

        private void PlayRotateAnimation(RectTransform targetRect, ElementAnimationAction action)
        {
            if (targetRect == null) return;

            // Настройки анимации
            var tweenSettings = new TweenSettings(
                action.duration,
                action.useSpring ? Ease.OutBack : action.ease,
                startDelay: action.delay);

            // Запускаем анимацию вращения
            Tween.Rotation(targetRect, Quaternion.Euler(0, 0, action.rotationAngle), tweenSettings);
        }

        private void PlayFadeAnimation(Graphic targetGraphic, ElementAnimationAction action)
        {
            if (targetGraphic == null) return;

            // Настройки анимации
            var tweenSettings = new TweenSettings(
                action.duration,
                action.useSpring ? Ease.OutBack : action.ease,
                startDelay: action.delay);

            // Запускаем анимацию прозрачности
            Tween.Alpha(targetGraphic, action.targetAlpha, tweenSettings);
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