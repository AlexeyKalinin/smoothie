using System;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    /// <summary>
    /// Manages themes and applies them to registered handlers
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        [SerializeField] private Theme activeTheme;
        
        private static ThemeManager _instance;
        private readonly List<IThemeHandler> _themeHandlers = new List<IThemeHandler>();

        /// <summary>
        /// Singleton instance of the ThemeManager
        /// </summary>
        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("Smoothie Theme Manager");
                    _instance = go.AddComponent<ThemeManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Currently active theme
        /// </summary>
        public Theme ActiveTheme => activeTheme;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (activeTheme != null)
            {
                ApplyTheme(activeTheme);
            }
        }
        
        /// <summary>
        /// Set and apply a new active theme
        /// </summary>
        public void SetActiveTheme(Theme theme)
        {
            activeTheme = theme;
            ApplyTheme(theme);
        }
        
        /// <summary>
        /// Apply the specified theme to all registered handlers
        /// </summary>
        public void ApplyTheme(Theme theme)
        {
            if (theme == null)
                return;
                
            foreach (var handler in _themeHandlers)
            {
                handler.OnThemeChanged(theme);
            }
        }
        
        /// <summary>
        /// Register a theme handler to receive theme updates
        /// </summary>
        public void RegisterHandler(IThemeHandler handler)
        {
            if (!_themeHandlers.Contains(handler))
            {
                _themeHandlers.Add(handler);
                
                // Apply the current theme to the new handler
                if (activeTheme != null)
                {
                    handler.OnThemeChanged(activeTheme);
                }
            }
        }
        
        /// <summary>
        /// Unregister a theme handler
        /// </summary>
        public void UnregisterHandler(IThemeHandler handler)
        {
            _themeHandlers.Remove(handler);
        }
    }

    /// <summary>
    /// Interface for objects that can respond to theme changes
    /// </summary>
    public interface IThemeHandler
    {
        void OnThemeChanged(Theme newTheme);
    }
}
