using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public class SmoothieUIBase : MonoBehaviour
{
    [InlineEditor, SerializeField]
    private SmoothieColorTheme CurrentTheme;

    public SmoothieColorTheme currentTheme
    {
        get { return CurrentTheme; }
    }

    [InlineEditor, SerializeField]
    private SmoothieColorTheme actualTheme;

    public SmoothieColorTheme ActualTheme
    {
        get { return actualTheme; }
    }

    private Coroutine _themeTransitionCoroutine;

    private void OnEnable()
    {
        UpdateColors(true);
        if (CurrentTheme != null)
        {
            CurrentTheme.OnThemeChanged += ThemeChanged;
        }
    }
    private void OnDisable()
    {
        if (CurrentTheme != null)
        {
            CurrentTheme.OnThemeChanged -= ThemeChanged;
        }
    }

    public void ChangeActualTheme()
    {
        OnValidate();
    }


    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateColors();
        }
        else
        {
            // Настройка прямой цветовой синхронизации без анимации для режима редактирования
            if (CurrentTheme != null && ActualTheme != null)
            {
                foreach (var type in Enum.GetValues(typeof(SmoothieColorTheme.ColorType)))
                {
                    ActualTheme.SetColorByType((SmoothieColorTheme.ColorType)type, CurrentTheme.GetColorByType((SmoothieColorTheme.ColorType)type));
                }
                ActualTheme.OnThemeChanged?.Invoke();
            }
        }

        UpdateAllChildUIElements();
    }

    private void UpdateAllChildUIElements()
    {
        SmoothieUIElement[] childElements = GetComponentsInChildren<SmoothieUIElement>();
        foreach (var element in childElements)
        {
            element.UpdateColors();
        }
    }

    private void ThemeChanged()
    {
        UpdateColors();
    }

    private void UpdateColors(bool instant = false)
    {
        if (CurrentTheme == null || ActualTheme == null) return;

        if (_themeTransitionCoroutine != null)
        {
            StopCoroutine(_themeTransitionCoroutine);
        }

        _themeTransitionCoroutine = StartCoroutine(TransitionThemeColors(instant));
    }

    private IEnumerator TransitionThemeColors(bool instant)
    {
        float duration = instant ? 0f : 5f;
        float elapsed = 0f;

        Dictionary<SmoothieColorTheme.ColorType, Color> initialColors = new Dictionary<SmoothieColorTheme.ColorType, Color>(ActualTheme.GetThemeColors());
        Dictionary<SmoothieColorTheme.ColorType, Color> targetColors = CurrentTheme.GetThemeColors();

        while (elapsed < duration)
        {
            foreach (var type in Enum.GetValues(typeof(SmoothieColorTheme.ColorType)))
            {
                ActualTheme.SetColorByType((SmoothieColorTheme.ColorType)type, Color.Lerp(initialColors[(SmoothieColorTheme.ColorType)type], targetColors[(SmoothieColorTheme.ColorType)type], elapsed / duration));
            }
            
            ActualTheme.OnThemeChanged?.Invoke(); // Переместите вызов события сюда

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var type in Enum.GetValues(typeof(SmoothieColorTheme.ColorType)))
        {
            ActualTheme.SetColorByType((SmoothieColorTheme.ColorType)type, targetColors[(SmoothieColorTheme.ColorType)type]);
        }
    }

}
