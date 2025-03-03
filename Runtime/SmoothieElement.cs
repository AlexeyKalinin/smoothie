using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#endif

namespace Smoothie
{
    [ExecuteAlways]
public class SmoothieElement : MonoBehaviour
{
    // Ссылка на компонент Image, который мы перекрашиваем
    [SerializeField] private Image image;

    // Если true – всегда использовать overrideColor независимо от темы
    [SerializeField] private bool useOverrideColor = false;
    [SerializeField, ShowIf("useOverrideColor")] private Color overrideColor = Color.white;

#if UNITY_EDITOR
    // Дропдаун для выбора ключа, если не используется override
    [ValueDropdown("GetColorKeys")]
    [OnValueChanged("RefreshColor")]
#endif
    [SerializeField] private string colorKey;

    private void OnEnable()
    {
        RefreshColor();
        if (SmoothieRuntimeManager.Instance != null)
        {
            SmoothieRuntimeManager.Instance.OnThemeChanged += RefreshColor;
            // Подписываемся на обновление цветов при анимации
            SmoothieRuntimeManager.Instance.OnColorsUpdated += RefreshColor;
        }
    }

    private void OnDisable()
    {
        if (SmoothieRuntimeManager.Instance != null)
        {
            SmoothieRuntimeManager.Instance.OnThemeChanged -= RefreshColor;
            // Отписываемся от обновления цветов при анимации
            SmoothieRuntimeManager.Instance.OnColorsUpdated -= RefreshColor;
        }
    }

    /// <summary>
    /// Обновляет цвет компонента Image.
    /// Если useOverrideColor включён, используется overrideColor.
    /// Иначе – берётся цвет из текущего кэша анимированных цветов по colorKey.
    /// </summary>
    public void RefreshColor()
    {
        if (image == null)
            return;
            
        if (useOverrideColor) 
        {
            image.color = overrideColor;
        } 
        else 
        {
            var mgr = SmoothieRuntimeManager.Instance;
            if (mgr != null && !string.IsNullOrEmpty(colorKey)) 
            {
                // Используем анимированный цвет вместо прямого доступа к теме
                image.color = mgr.GetAnimatedColor(colorKey);
            }
        }
    }

#if UNITY_EDITOR
    private IEnumerable<string> GetColorKeys()
    {
        if (Application.isPlaying) 
        {
            if (SmoothieRuntimeManager.Instance != null && SmoothieRuntimeManager.Instance.ColorScheme != null)
                return SmoothieRuntimeManager.Instance.ColorScheme.baseThemeDefinitions.Select(d => d.key);
        } 
        else 
        {
            string[] guids = AssetDatabase.FindAssets("t:SmoothieColorScheme");
            if (guids != null && guids.Length > 0) 
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var scheme = AssetDatabase.LoadAssetAtPath<SmoothieColorScheme>(assetPath);
                if (scheme != null)
                    return scheme.baseThemeDefinitions.Select(d => d.key);
            }
        }
        return new List<string>();
    }
    
    // Реакция на изменения в инспекторе
    private void OnValidate()
    {
        // При изменении настроек в инспекторе сразу обновляем цвет
        RefreshColor();
    }
#endif
}
}