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

    // Поле для имени новой темы
    [Space(10)]
    [HorizontalGroup("CreateThemeRow")]
    [HideLabel]
    public string newThemeName = "New Theme";

    [HorizontalGroup("CreateThemeRow", width: 150)]
    [Button("Add New Theme")]
    public void AddNewDependentTheme()
    {
        if (string.IsNullOrWhiteSpace(newThemeName))
            newThemeName = "New Theme";

        SmoothieTheme newTheme = ScriptableObject.CreateInstance<SmoothieTheme>();
        newTheme.ThemeName = newThemeName;
        newTheme.name = newThemeName;
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
                theme.SynchronizeWithBase(baseThemeDefinitions);
            }
        }
        // 3) Обновляем локальную копию списка
        oldDependentThemes.Clear();
        oldDependentThemes.AddRange(dependentThemes);

        // Сохраняем изменения
        AssetDatabase.SaveAssets();
    }
#endif
}
}
