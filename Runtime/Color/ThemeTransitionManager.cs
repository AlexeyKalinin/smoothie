using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;

namespace Smoothie
{
    /// <summary>
/// Пример компонента для демонстрации переключения темы с эффектами
/// </summary>
public class ThemeTransitionManager : MonoBehaviour
{
    [SerializeField] private SmoothieRuntimeManager runtimeManager;
    
    [Title("Transition Settings")]
    [SerializeField, Range(0.1f, 2f)] private float transitionDuration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.InOutSine;
    
    [Title("Available Themes")]
    [SerializeField, ValueDropdown("GetAvailableThemes")] 
    private SmoothieTheme[] availableThemes;
    
    private void Reset()
    {
        runtimeManager = FindObjectOfType<SmoothieRuntimeManager>();
    }
    
    /// <summary>
    /// Переключает на выбранную тему с эффектом перехода
    /// </summary>
    [Button("Switch Theme")]
    public void SwitchTheme(SmoothieTheme newTheme)
    {
        if (runtimeManager == null)
        {
            Debug.LogError("SmoothieRuntimeManager не назначен!");
            return;
        }
        
        runtimeManager.SetCurrentTheme(newTheme);
    }
    
    /// <summary>
    /// Циклически переключает между доступными темами
    /// </summary>
    [Button("Cycle Next Theme")]
    public void CycleToNextTheme()
    {
        if (runtimeManager == null || availableThemes == null || availableThemes.Length == 0)
        {
            Debug.LogWarning("Невозможно переключить тему. Проверьте ссылки на RuntimeManager и список тем.");
            return;
        }
        
        // Найдем индекс текущей темы
        int currentIndex = -1;
        for (int i = 0; i < availableThemes.Length; i++)
        {
            if (availableThemes[i] == runtimeManager.CurrentTheme)
            {
                currentIndex = i;
                break;
            }
        }
        
        // Выберем следующую тему
        int nextIndex = (currentIndex + 1) % availableThemes.Length;
        runtimeManager.SetCurrentTheme(availableThemes[nextIndex]);
    }
    
#if UNITY_EDITOR
    private SmoothieTheme[] GetAvailableThemes()
    {
        if (runtimeManager != null && runtimeManager.ColorScheme != null)
        {
            return runtimeManager.ColorScheme.dependentThemes.ToArray();
        }
        return new SmoothieTheme[0];
    }
#endif
}

}