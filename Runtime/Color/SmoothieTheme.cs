using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    [HideMonoScript]
    public class SmoothieTheme : ScriptableObject
    {
        // Reference to the parent color scheme
        [HideInInspector]
        public SmoothieColorScheme ColorSchemeRef;

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

        // Red button for removing this theme
        [Button("Remove This Theme", ButtonHeight = 25)]
        [GUIColor(1f, 0.25f, 0.25f)]
        #if UNITY_EDITOR
        private void RemoveThisTheme()
        {
            if (ColorSchemeRef != null)
            {
                ColorSchemeRef.dependentThemes.Remove(this);
                AssetDatabase.RemoveObjectFromAsset(this);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning($"Can't remove theme {name} because ColorSchemeRef is null.");
                
                // Try to find parent asset
                string path = AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path))
                {
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (mainAsset is SmoothieColorScheme colorScheme)
                    {
                        colorScheme.dependentThemes.Remove(this);
                        AssetDatabase.RemoveObjectFromAsset(this);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        #endif

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
                #if UNITY_2022_2_OR_NEWER
                var managers = Object.FindObjectsByType<SmoothieRuntimeManager>(FindObjectsSortMode.None);
                #else
                var managers = Object.FindObjectsOfType<SmoothieRuntimeManager>(true);
                #endif
                
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