using UnityEngine;
using System.Collections.Generic;
using System;
using PrimeTween;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace Smoothie
{
    [ExecuteAlways]
public class SmoothieRuntimeManager : MonoBehaviour
{
    public static SmoothieRuntimeManager Instance { get; private set; }

    [Header("Screen Manager")]
    [SerializeField] private SmoothieScreenManager screenManager;
    public SmoothieScreenManager ScreenManager => screenManager;

    [Header("Color Scheme")]
    [SerializeField] private SmoothieColorScheme colorScheme;  // Основной ScriptableObject с темами
    public SmoothieColorScheme ColorScheme => colorScheme;

    [Header("Animation Managers")]
    [SerializeField] private SmoothieScreenAnimationManager screenAnimationManager;
    public SmoothieScreenAnimationManager ScreenAnimationManager => screenAnimationManager;

    [SerializeField] private SmoothieElementAnimationManager elementAnimationManager;
    public SmoothieElementAnimationManager ElementAnimationManager => elementAnimationManager;

    [Header("Theme Transition")]
    [SerializeField, Range(0.1f, 2f)] private float themeTransitionDuration = 0.5f;
    [SerializeField] private Ease themeTransitionEase = Ease.InOutSine;

    // Выбор текущей темы через дропдаун (в редакторе)
#if UNITY_EDITOR
    [FormerlySerializedAs("_currentTheme")]
    [ValueDropdown("GetThemeDropdown")]
    [OnValueChanged("ApplyThemeImmediate")]
#endif
    [SerializeField] private SmoothieTheme currentTheme;
    public SmoothieTheme CurrentTheme {
        get { return currentTheme; }
        set {
            if (currentTheme != value) {
                var oldTheme = currentTheme;
                currentTheme = value;
                
                if (Application.isPlaying)
                    AnimateThemeTransition(oldTheme, currentTheme);
                else
                    ApplyThemeImmediate();
            }
        }
    }

    // Словарь для хранения текущих анимированных цветов
    private readonly Dictionary<string, Color> _animatedColors = new Dictionary<string, Color>();
    public IReadOnlyDictionary<string, Color> AnimatedColors => _animatedColors;

    // Событие, которое вызывается при смене темы
    public event Action OnThemeChanged;
    
    // Событие, которое вызывается при обновлении цветов (во время анимации)
    public event Action OnColorsUpdated;

    // Кэш цветов для текущей темы для быстрого доступа (ключ -> цвет)
    private readonly Dictionary<string, Color> _cachedColors = new Dictionary<string, Color>();
    public IReadOnlyDictionary<string, Color> CachedColors => _cachedColors;

    // Список зарегистрированных ScreenView
    private readonly List<SmoothieScreenView> _allViews = new List<SmoothieScreenView>();

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // В редакторе не вызываем DontDestroyOnLoad
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // Если текущая тема не установлена, берем первую из списка зависимых тем
        if (currentTheme == null && colorScheme != null && colorScheme.dependentThemes.Count > 0)
            currentTheme = colorScheme.dependentThemes[0];
        
        ApplyThemeImmediate();
    }

    private void Update()
    {
        if (screenManager != null)
            screenManager.UpdateScreensStatus();
    }

    // Применить тему немедленно, без анимации
    public void ApplyThemeImmediate()
    {
        if (currentTheme == null)
            return;
            
        // Обновляем анимированные цвета напрямую из текущей темы
        _animatedColors.Clear();
        foreach (var def in currentTheme.colorDefinitions)
        {
            if (!string.IsNullOrEmpty(def.key))
            {
                _animatedColors[def.key] = def.color;
            }
        }
        
        // Обновляем кэш цветов
        UpdateCachedColors();
        
        // Уведомляем об изменении цветов
        OnThemeChanged?.Invoke();
        OnColorsUpdated?.Invoke();
        
        // Обновляем все элементы с новыми цветами
        RefreshAllElements();
        
        //Debug.Log($"Применена тема {_currentTheme.ThemeName} с {_currentTheme.colorDefinitions.Count} цветами");
    }

    // Анимируем переход между темами
    private void AnimateThemeTransition(SmoothieTheme oldTheme, SmoothieTheme newTheme)
    {
        if (oldTheme == null || newTheme == null)
        {
            // Если одна из тем равна null, просто обновляем кэш без анимации
            ApplyThemeImmediate();
            return;
        }

        // Остановим все текущие анимации, относящиеся к теме
        Tween.StopAll(this);

        // Создаем словарь начальных и конечных цветов для всех ключей
        Dictionary<string, (Color startColor, Color endColor)> colorTransitions = 
            new Dictionary<string, (Color, Color)>();

        // Собираем все уникальные ключи из обеих тем
        HashSet<string> allKeys = new HashSet<string>();
        foreach (var def in oldTheme.colorDefinitions)
            if (!string.IsNullOrEmpty(def.key))
                allKeys.Add(def.key);
        foreach (var def in newTheme.colorDefinitions)
            if (!string.IsNullOrEmpty(def.key))
                allKeys.Add(def.key);

        // Инициализируем переходы цветов
        foreach (string key in allKeys)
        {
            Color startColor = Color.white;
            Color endColor = Color.white;
            
            // Получаем начальный цвет (из текущего анимированного состояния или из старой темы)
            if (_animatedColors.TryGetValue(key, out Color currentAnimatedColor))
                startColor = currentAnimatedColor;
            else if (oldTheme.TryGetColor(key, out Color oldColor))
                startColor = oldColor;

            // Получаем конечный цвет из новой темы
            if (newTheme.TryGetColor(key, out Color newColor))
                endColor = newColor;

            colorTransitions[key] = (startColor, endColor);
        }

        // Запускаем анимацию для каждого ключа
        foreach (var entry in colorTransitions)
        {
            string key = entry.Key;
            Color startColor = entry.Value.startColor;
            Color endColor = entry.Value.endColor;

            // Если цвета не отличаются, пропускаем анимацию
            if (startColor == endColor)
            {
                _animatedColors[key] = endColor;
                continue;
            }

            // Анимируем каждый компонент цвета отдельно
            AnimateColorComponents(key, startColor, endColor);
        }

        // Обновляем кэш цветов для новой темы
        UpdateCachedColors();
        
        // Уведомляем о смене темы
        OnThemeChanged?.Invoke();
    }

    // Вспомогательный метод для анимации компонентов цвета по отдельности
    void AnimateColorComponents(string colorKey, Color startColor, Color endColor)
    {
        // Создаем текущий цвет, который будем изменять
        if (!_animatedColors.ContainsKey(colorKey))
            _animatedColors[colorKey] = startColor;
            
        // Сохраняем ссылку на ключ, чтобы использовать в лямбда-выражениях
        string key = colorKey;
            
        // Анимация для R компонента
        Tween.Custom(startColor.r, endColor.r, themeTransitionDuration, value => {
            if (_animatedColors.TryGetValue(key, out Color currentColor)) {
                currentColor.r = value;
                _animatedColors[key] = currentColor;
                OnColorsUpdated?.Invoke();
            }
        });
        
        // Анимация для G компонента
        Tween.Custom(startColor.g, endColor.g, themeTransitionDuration, value => {
            if (_animatedColors.TryGetValue(key, out Color currentColor)) {
                currentColor.g = value;
                _animatedColors[key] = currentColor;
                OnColorsUpdated?.Invoke();
            }
        });
        
        // Анимация для B компонента
        Tween.Custom(startColor.b, endColor.b, themeTransitionDuration, value => {
            if (_animatedColors.TryGetValue(key, out Color currentColor)) {
                currentColor.b = value;
                _animatedColors[key] = currentColor;
                OnColorsUpdated?.Invoke();
            }
        });
        
        // Анимация для A компонента
        Tween.Custom(startColor.a, endColor.a, themeTransitionDuration, value => {
            if (_animatedColors.TryGetValue(key, out Color currentColor)) {
                currentColor.a = value;
                _animatedColors[key] = currentColor;
                OnColorsUpdated?.Invoke();
            }
        });
    }

    public void RegisterView(SmoothieScreenView view)
    {
        if (!_allViews.Contains(view))
            _allViews.Add(view);
    }

    public void UnregisterView(SmoothieScreenView view)
    {
        if (_allViews.Contains(view))
            _allViews.Remove(view);
    }

    public void ShowScreen(SmoothieScreen screen)
    {
        foreach (var view in _allViews)
        {
            if (view != null && view.screen == screen)
            {
                var elements = view.GetComponentsInChildren<SmoothieElement>(true);
                foreach (var element in elements)
                    element.gameObject.SetActive(true);
            }
        }
    }

    public void HideScreen(SmoothieScreen screen)
    {
        foreach (var view in _allViews)
        {
            if (view != null && view.screen == screen)
            {
                var elements = view.GetComponentsInChildren<SmoothieElement>(true);
                foreach (var element in elements)
                    element.gameObject.SetActive(false);
            }
        }
    }

    public void HideAllScreens()
    {
        foreach (var view in _allViews)
        {
            var elements = view.GetComponentsInChildren<SmoothieElement>(true);
            foreach (var element in elements)
                element.gameObject.SetActive(false);
        }
    }

    public int GetInstancesCount(SmoothieScreen screen)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // В редакторе (без запущенной игры) – ищем все ScreenView напрямую
            var viewsInEditor = UnityEngine.Object.FindObjectsByType<SmoothieScreenView>(FindObjectsSortMode.None);
            
            int editorCount = 0;
            foreach (var v in viewsInEditor)
            {
                if (v != null && v.screen == screen)
                    editorCount++;
            }
            return editorCount;
        }
#endif

        // В Play Mode – используем список allViews
        int count = 0;
        foreach (var view in _allViews)
        {
            if (view != null && view.screen == screen)
                count++;
        }
        return count;
    }

    public bool GetIsShown(SmoothieScreen screen)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // В редакторе ищем все ScreenView напрямую
            var viewsInEditor = UnityEngine.Object.FindObjectsByType<SmoothieScreenView>(FindObjectsSortMode.None);
            foreach (var v in viewsInEditor)
            {
                if (v != null && v.screen == screen)
                {
                    var elements = v.GetComponentsInChildren<SmoothieElement>(true);
                    foreach (var e in elements)
                    {
                        if (e.gameObject.activeInHierarchy)
                            return true;
                    }
                }
            }
            return false;
        }
#endif

        // В Play Mode – используем список allViews
        foreach (var view in _allViews)
        {
            if (view != null && view.screen == screen)
            {
                var elements = view.GetComponentsInChildren<SmoothieElement>(true);
                foreach (var element in elements)
                {
                    if (element.gameObject.activeInHierarchy)
                        return true;
                }
            }
        }
        return false;
    }

    private void RefreshAllElements()
    {
        if (Application.isPlaying) 
        {
            foreach (var view in _allViews)
                if (view != null)
                    view.RefreshColor();
        } 
        else 
        {
#if UNITY_EDITOR
            // В редакторе ищем все экраны напрямую
            var views = UnityEngine.Object.FindObjectsByType<SmoothieScreenView>(FindObjectsSortMode.None);
            foreach (var view in views)
            {
                if (view != null)
                    view.RefreshColor();
            }
            
            // Также можно принудительно перерисовать иерархию, чтобы обновились цвета
            UnityEditor.SceneView.RepaintAll();
            UnityEditor.EditorApplication.RepaintHierarchyWindow();
#endif
        }
    }

#if UNITY_EDITOR
    private IEnumerable<SmoothieTheme> GetThemeDropdown()
    {
        if (colorScheme != null)
            return colorScheme.dependentThemes;
        return new List<SmoothieTheme>();
    }
    
    // Метод для отслеживания изменений в самой теме
    public void OnThemeModified()
    {
        // Если тема была изменена извне, применяем изменения немедленно
        ApplyThemeImmediate();
    }
#endif

    public void SetCurrentTheme(SmoothieTheme newTheme)
    {
        CurrentTheme = newTheme;
    }

    // Обновляет кэш цветов для текущей темы
    private void UpdateCachedColors()
    {
        _cachedColors.Clear();
        if (currentTheme != null)
        {
            foreach (var def in currentTheme.colorDefinitions)
                if (!string.IsNullOrEmpty(def.key))
                    _cachedColors[def.key] = def.color;
        }
    }

    // Получение текущего анимированного цвета по ключу
    public Color GetAnimatedColor(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Color.white;

        // Проверяем, есть ли цвет в анимированных цветах
        if (_animatedColors.TryGetValue(key, out Color color))
            return color;

        // Если нет, возвращаем цвет из текущей темы (или белый, если ключ не найден)
        if (currentTheme != null && currentTheme.TryGetColor(key, out color))
            return color;

        return Color.white;
    }
}
}