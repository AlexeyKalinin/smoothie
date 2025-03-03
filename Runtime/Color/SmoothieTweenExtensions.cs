using UnityEngine;
using PrimeTween;
using System;

namespace Smoothie
{
    /// <summary>
/// Расширения для удобной работы с PrimeTween в системе Smoothie
/// </summary>
public static class SmoothieTweenExtensions
{
    /// <summary>
    /// Анимирует цвет компонента Image с учетом текущей темы
    /// </summary>
    public static Tween ColorByKey(this SmoothieElement element, string colorKey, float duration)
    {
        if (element == null || string.IsNullOrEmpty(colorKey) || SmoothieRuntimeManager.Instance == null)
            return default;
            
        var image = element.GetComponent<UnityEngine.UI.Image>();
        if (image == null)
            return default;
            
        Color targetColor = SmoothieRuntimeManager.Instance.GetAnimatedColor(colorKey);
        return Tween.Color(image, targetColor, duration, Ease.InOutSine);
    }
    
    /// <summary>
    /// Переводит одну тему в другую с заданными параметрами
    /// </summary>
    public static void TransitionToTheme(this SmoothieRuntimeManager manager, SmoothieTheme targetTheme)
    {
        if (manager == null || targetTheme == null)
            return;
            
        // Запускаем смену темы (внутри которой идут анимации)
        manager.SetCurrentTheme(targetTheme);
    }
    
    /// <summary>
    /// Создает эффект пульсации для элемента с использованием цвета из темы
    /// </summary>
    public static Tween PulseColor(this SmoothieElement element, string colorKey, 
        float duration = 0.5f, float intensity = 0.3f, int cycles = 2)
    {
        if (element == null || string.IsNullOrEmpty(colorKey) || SmoothieRuntimeManager.Instance == null)
            return default;
            
        var image = element.GetComponent<UnityEngine.UI.Image>();
        if (image == null)
            return default;
            
        Color baseColor = SmoothieRuntimeManager.Instance.GetAnimatedColor(colorKey);
        Color brightColor = new Color(
            Mathf.Clamp01(baseColor.r + intensity),
            Mathf.Clamp01(baseColor.g + intensity),
            Mathf.Clamp01(baseColor.b + intensity),
            baseColor.a
        );
        
        // Используем прямой вызов с параметрами cycles и cycleMode
        return Tween.Color(image, brightColor, duration, cycles: cycles, cycleMode: CycleMode.Yoyo);
    }
}
}
