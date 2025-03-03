using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;

namespace Smoothie
{
    [HideMonoScript]
public class SmoothieTheme : ScriptableObject
{
    [SerializeField, OnValueChanged("UpdateAssetName")]
    [LabelText("Theme Name")]
    private string themeName;

    [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
    [LabelText("Colors")]
    public List<SmoothieColorDefinitionDependent> colorDefinitions = new List<SmoothieColorDefinitionDependent>();

    public string ThemeName
    {
        get => themeName;
        set => themeName = value;
    }

    public void SynchronizeWithBase(List<SmoothieColorDefinitionBase> baseDefinitions)
    {
        var newList = new List<SmoothieColorDefinitionDependent>();

        foreach (var baseDef in baseDefinitions)
        {
            var existing = colorDefinitions.FirstOrDefault(d => d.key == baseDef.key);
            if (existing != null)
            {
                newList.Add(existing);
            }
            else
            {
                newList.Add(new SmoothieColorDefinitionDependent
                {
                    key = baseDef.key,
                    color = baseDef.color
                });
            }
        }

        colorDefinitions = newList;
    }

    private void UpdateAssetName()
    {
        name = themeName; 
        // Убираем лишние вызовы ImportAsset, чтобы не вызывать реимпорт в OnValidate
    }

    public Color GetColor(string key)
    {
        var def = colorDefinitions.FirstOrDefault(d => d.key == key);
        return def != null ? def.color : Color.white;
    }

    // Добавляем метод, который безопасно пытается получить цвет по ключу
    public bool TryGetColor(string key, out Color color)
    {
        var def = colorDefinitions.FirstOrDefault(d => d.key == key);
        if (def != null)
        {
            color = def.color;
            return true;
        }
        
        color = Color.white;
        return false;
    }

#if UNITY_EDITOR
    // Вызывается при изменении в Inspector
    private void OnValidate()
    {
        // Оповещаем RuntimeManager о изменениях в теме с небольшой задержкой
        UnityEditor.EditorApplication.delayCall += () => 
        {
            if (this == null) // Проверка, что объект не был уничтожен
                return;
                
            // Найдем все RuntimeManagers и проверим, использует ли кто-то из них эту тему
            var managers = UnityEngine.Object.FindObjectsOfType<SmoothieRuntimeManager>(true);
            foreach (var manager in managers)
            {
                if (manager.CurrentTheme == this)
                {
                    // Если тема является активной в этом менеджере, вызываем обновление
                    manager.ApplyThemeImmediate();
                }
            }
        };
    }
#endif
}

}