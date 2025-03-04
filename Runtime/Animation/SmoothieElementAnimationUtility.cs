using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;

namespace Smoothie
{
    /// <summary>
    /// Утилитарный класс для упрощения управления анимациями элементов
    /// </summary>
    public static class SmoothieElementAnimationUtility
    {
        /// <summary>
        /// Создает и воспроизводит анимацию для заданного элемента
        /// </summary>
        /// <param name="element">Целевой элемент</param>
        /// <param name="eventKey">Ключ события</param>
        /// <returns>Последовательность анимаций</returns>
        public static Sequence PlayAnimation(SmoothieElement element, string eventKey)
        {
            if (element == null) return default;
            
            // Создаем новую последовательность
            var sequence = Sequence.Create();
            
            // Воспроизводим анимацию
            element.PlayAnimation(eventKey);
            
            return sequence;
        }

        /// <summary>
        /// Создает пульсацию для указанного элемента с использованием цвета из темы
        /// </summary>
        public static Tween CreatePulse(Graphic targetGraphic, string colorKey, 
            float duration = 0.5f, float intensity = 0.3f, int cycles = 2)
        {
            if (targetGraphic == null || string.IsNullOrEmpty(colorKey) || 
                SmoothieRuntimeManager.Instance == null)
                return default;
            
            Color baseColor = SmoothieRuntimeManager.Instance.GetAnimatedColor(colorKey);
            Color brightColor = new Color(
                Mathf.Clamp01(baseColor.r + intensity),
                Mathf.Clamp01(baseColor.g + intensity),
                Mathf.Clamp01(baseColor.b + intensity),
                baseColor.a
            );
            
            // Используем циклическую анимацию
            return Tween.Color(targetGraphic, brightColor, duration, 
                cycles: cycles, cycleMode: CycleMode.Yoyo);
        }

        /// <summary>
        /// Создает анимацию для группы элементов с задержкой между ними
        /// </summary>
        public static Sequence CreateSequentialAnimation(List<SmoothieElement> elements, 
            string eventKey, float delayBetweenElements = 0.1f)
        {
            if (elements == null || elements.Count == 0 || string.IsNullOrEmpty(eventKey))
                return default;
            
            var sequence = Sequence.Create();
            
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element == null) continue;
                
                // Создаем задержку между элементами
                if (i > 0)
                {
                    sequence.ChainDelay(delayBetweenElements);
                }
                
                // Добавляем активацию анимации
                sequence.ChainCallback(() => element.PlayAnimation(eventKey));
            }
            
            return sequence;
        }

        /// <summary>
        /// Находит все элементы Smoothie на сцене и воспроизводит для них анимацию
        /// </summary>
        public static void PlayGlobalAnimation(string eventKey)
        {
            var elements = Object.FindObjectsOfType<SmoothieElement>();
            foreach (var element in elements)
            {
                element.PlayAnimation(eventKey);
            }
        }
    }
}