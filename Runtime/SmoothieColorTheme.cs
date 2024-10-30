using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[CreateAssetMenu(menuName = "Smoothie/ColorTheme")]
public class SmoothieColorTheme : ScriptableObject
{
    public enum ColorType
    {
        backgroundNormal, backgroundOver, backgroundPressed, backgroundDeactivated, textNormal, textOver, textPressed, textDeactivated, select, shadow, descriptionText, mainText, backgroundPanel
    }

    public Color backgroundNormal, backgroundOver, backgroundPressed, backgroundDeactivated, textNormal, textOver, textPressed, textDeactivated, select, shadow, descriptionText, mainText, backgroundPanel;
    
    public Action OnThemeChanged;

    private void OnValidate()
    {
        OnThemeChanged?.Invoke();
        foreach (var uiBase in FindObjectsOfType<SmoothieUIBase>())
        {
            if (uiBase.currentTheme == this)
            {
                uiBase.ChangeActualTheme();
            }
        }
    }
    public Dictionary<ColorType, Color> GetThemeColors()
    {
        Dictionary<ColorType, Color> themeColors = new Dictionary<ColorType, Color>();
        foreach (var type in Enum.GetValues(typeof(ColorType)))
        {
            themeColors[(ColorType)type] = GetColorByType((ColorType)type);
        }
        return themeColors;
    }

    public void SetColorByType(ColorType type, Color color)
    {
        switch (type)
        {
            case ColorType.backgroundNormal:
                backgroundNormal = color;
                break;
            case ColorType.backgroundOver:
                backgroundOver = color;
                break;
            case ColorType.backgroundPressed:
                backgroundPressed = color;
                break;
            case ColorType.backgroundDeactivated:
                backgroundDeactivated = color;
                break;
            case ColorType.textNormal:
                textNormal = color;
                break;
            case ColorType.textOver:
                textOver = color;
                break;
            case ColorType.textPressed:
                textPressed = color;
                break;
            case ColorType.textDeactivated:
                textDeactivated = color;
                break;
            case ColorType.select:
                select = color;
                break;
            case ColorType.shadow:
                shadow = color;
                break;
            case ColorType.descriptionText:
                descriptionText = color;
                break;
            case ColorType.mainText:
                mainText = color;
                break;
            case ColorType.backgroundPanel:
                backgroundPanel = color;
                break;
        }
    }

    public Color GetColorByType(ColorType type)
    {
        switch (type)
        {
            case ColorType.backgroundNormal:
                return backgroundNormal;
            case ColorType.backgroundOver:
                return backgroundOver;
            case ColorType.backgroundPressed:
                return backgroundPressed;
            case ColorType.backgroundDeactivated:
                return backgroundDeactivated;
            case ColorType.textNormal:
                return textNormal;
            case ColorType.textOver:
                return textOver;
            case ColorType.textPressed:
                return textPressed;
            case ColorType.textDeactivated:
                return textDeactivated;
            case ColorType.select:
                return select;
            case ColorType.shadow:
                return shadow;
            case ColorType.descriptionText:
                return descriptionText;
            case ColorType.mainText:
                return mainText;
            case ColorType.backgroundPanel:
                return backgroundPanel;
            default:
                return Color.white;
        }
    }
}
