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
    private SmoothieRuntimeManager manager;
    private List<SmoothieTheme> availableThemes = new List<SmoothieTheme>();
    private int selectedThemeIndex = 0;

    protected override void OnEnable()
    {
        base.OnEnable();
        manager = target as SmoothieRuntimeManager;
        UpdateAvailableThemes();
    }

    private void UpdateAvailableThemes()
    {
        availableThemes.Clear();
        if (manager != null && manager.ColorScheme != null)
        {
            availableThemes.AddRange(manager.ColorScheme.dependentThemes);
            
            // Найдем индекс текущей темы
            if (manager.CurrentTheme != null)
            {
                selectedThemeIndex = availableThemes.IndexOf(manager.CurrentTheme);
                if (selectedThemeIndex < 0) selectedThemeIndex = 0;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (manager == null || manager.ColorScheme == null || availableThemes.Count == 0)
            return;

        EditorGUILayout.Space(10);
        SirenixEditorGUI.DrawThickHorizontalSeparator();
        EditorGUILayout.Space(5);
        
        EditorGUILayout.LabelField("Theme Transition Preview", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Выбор темы
        EditorGUI.BeginChangeCheck();
        selectedThemeIndex = EditorGUILayout.Popup("Preview Theme", selectedThemeIndex, 
            GetThemeNames(availableThemes));
            
        if (EditorGUI.EndChangeCheck() && selectedThemeIndex >= 0 && selectedThemeIndex < availableThemes.Count)
        {
            // Применяем тему в режиме редактора
            SmoothieTheme selectedTheme = availableThemes[selectedThemeIndex];
            if (selectedTheme != null && manager.CurrentTheme != selectedTheme)
            {
                Undo.RecordObject(manager, "Change Theme");
                manager.SetCurrentTheme(selectedTheme);
                EditorUtility.SetDirty(manager);
            }
        }

        EditorGUILayout.Space(10);
        
        // Кнопка для последовательного перебора тем (для демонстрации)
        if (GUILayout.Button("Cycle Through Themes", GUILayout.Height(30)))
        {
            selectedThemeIndex = (selectedThemeIndex + 1) % availableThemes.Count;
            SmoothieTheme nextTheme = availableThemes[selectedThemeIndex];
            
            Undo.RecordObject(manager, "Cycle Theme");
            manager.SetCurrentTheme(nextTheme);
            EditorUtility.SetDirty(manager);
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