#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;

namespace Smoothie
{
    [CustomEditor(typeof(SmoothieRuntimeManager))]
public class SmoothieRuntimeManagerEditor : OdinEditor
{
    private SmoothieRuntimeManager _manager;
    private readonly List<SmoothieTheme> _availableThemes = new List<SmoothieTheme>();
    private int _selectedThemeIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        _manager = target as SmoothieRuntimeManager;
        UpdateAvailableThemes();
    }

    private void UpdateAvailableThemes()
    {
        _availableThemes.Clear();
        if (_manager != null && _manager.ColorScheme != null)
        {
            _availableThemes.AddRange(_manager.ColorScheme.dependentThemes);
            
            // Найдем индекс текущей темы
            if (_manager.CurrentTheme != null)
            {
                _selectedThemeIndex = _availableThemes.IndexOf(_manager.CurrentTheme);
                if (_selectedThemeIndex < 0) _selectedThemeIndex = 0;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (_manager == null || _manager.ColorScheme == null || _availableThemes.Count == 0)
            return;

        EditorGUILayout.Space(10);
        SirenixEditorGUI.DrawThickHorizontalSeparator();
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Theme Transition Preview", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Выбор темы
        EditorGUI.BeginChangeCheck();
        _selectedThemeIndex = EditorGUILayout.Popup("Preview Theme", _selectedThemeIndex, 
            GetThemeNames(_availableThemes));
            
        if (EditorGUI.EndChangeCheck() && _selectedThemeIndex >= 0 && _selectedThemeIndex < _availableThemes.Count)
        {
            // Применяем тему в режиме редактора
            SmoothieTheme selectedTheme = _availableThemes[_selectedThemeIndex];
            if (selectedTheme != null && _manager.CurrentTheme != selectedTheme)
            {
                Undo.RecordObject(_manager, "Change Theme");
                _manager.SetCurrentTheme(selectedTheme);
                EditorUtility.SetDirty(_manager);
            }
        }

        EditorGUILayout.Space(10);
        
        // Кнопка для последовательного перебора тем (для демонстрации)
        if (GUILayout.Button("Cycle Through Themes", GUILayout.Height(30)))
        {
            _selectedThemeIndex = (_selectedThemeIndex + 1) % _availableThemes.Count;
            SmoothieTheme nextTheme = _availableThemes[_selectedThemeIndex];
            
            Undo.RecordObject(_manager, "Cycle Theme");
            _manager.SetCurrentTheme(nextTheme);
            EditorUtility.SetDirty(_manager);
        }
    }

    private string[] GetThemeNames(List<SmoothieTheme> themes)
    {
        string[] names = new string[themes.Count];
        for (int i = 0; i < themes.Count; i++)
        {
            names[i] = themes[i] != null ? themes[i].ThemeName : "Unknown Theme";
        }
        return names;
    }
}
#endif
}