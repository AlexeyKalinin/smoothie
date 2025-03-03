using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieColorScheme", menuName = "Smoothie/Smoothie Color Scheme")]
    public class SmoothieColorScheme : ScriptableObject
    {
        [FoldoutGroup("Base Theme")]
        [ListDrawerSettings(Expanded = true)]
        [LabelText("Base Theme Colors")]
        public List<SmoothieColorDefinitionBase> baseThemeDefinitions = new List<SmoothieColorDefinitionBase>();

        // Simple button to add theme without text field
        [Button("Add Theme", ButtonHeight = 30)]
        [GUIColor(0.25f, 1f, 0.25f)]
        public void AddThemeSimple()
        {
            // Generate theme name based on count
            string themeName = $"Theme {dependentThemes.Count + 1}";
            
            // Make sure the name is unique
            int suffix = 1;
            while (dependentThemes.Any(t => t != null && t.ThemeName == themeName))
            {
                themeName = $"Theme {dependentThemes.Count + 1}_{suffix}";
                suffix++;
            }
            
            SmoothieTheme newTheme = ScriptableObject.CreateInstance<SmoothieTheme>();
            newTheme.ThemeName = themeName;
            newTheme.name = themeName;
            newTheme.ColorSchemeRef = this;
            newTheme.SynchronizeWithBase(baseThemeDefinitions);

        #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(newTheme, this);
            AssetDatabase.SaveAssets();
        #endif

            dependentThemes.Add(newTheme);
        }

        [FoldoutGroup("Dependent Themes")]
        [ListDrawerSettings(Expanded = true)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public List<SmoothieTheme> dependentThemes = new List<SmoothieTheme>();

        // Локальная копия списка, чтобы понять, что было удалено
        [System.NonSerialized]
        private List<SmoothieTheme> oldDependentThemes = new List<SmoothieTheme>();

        private void OnEnable()
        {
            // При загрузке ScriptableObject запоминаем текущий список
            oldDependentThemes.Clear();
            oldDependentThemes.AddRange(dependentThemes);
            
            // Set color scheme reference for all themes
            foreach (var theme in dependentThemes)
            {
                if (theme != null)
                {
                    theme.ColorSchemeRef = this;
                }
            }
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            // 1) Находим, какие темы были удалены из списка
            var removedThemes = oldDependentThemes.Except(dependentThemes).ToList();
            foreach (var removed in removedThemes)
            {
                if (removed != null)
                {
                    // Удаляем их из файла .asset
                    AssetDatabase.RemoveObjectFromAsset(removed);
                }
            }
            // 2) Синхронизируем все оставшиеся темы с базовой
            foreach (var theme in dependentThemes)
            {
                if (theme != null)
                {
                    theme.ColorSchemeRef = this;
                    theme.SynchronizeWithBase(baseThemeDefinitions);
                }
            }
            // 3) Обновляем локальную копию списка
            oldDependentThemes.Clear();
            oldDependentThemes.AddRange(dependentThemes);

            // Вместо прямого AssetDatabase.SaveAssets() – делаем это отложенно
            EditorApplication.delayCall += () =>
            {
                if (this != null && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
                {
                    AssetDatabase.SaveAssets();
                }
            };
        }
    #endif
    }
}