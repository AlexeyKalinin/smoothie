using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    public class SmoothieElementAnimationStyle : ScriptableObject
    {
        // Скрываем от инспектора, но храним ссылку на менеджера
        [HideInInspector]
        public SmoothieElementAnimationManager ManagerRef;

        [SerializeField, OnValueChanged("UpdateAssetName")]
        [LabelText("Style Name")]
        private string _styleName = "Untitled";

        public string styleName
        {
            get => _styleName;
            set => _styleName = value;
        }

        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = false,
            HideAddButton = false,
            HideRemoveButton = false)]
        [LabelText("Event Overrides")]
        public List<ElementAnimationEventDependent> eventDefinitions = new List<ElementAnimationEventDependent>();

        // Красная кнопка для удаления этого стиля
        [Button("Remove This Style", ButtonHeight = 25)]
        [GUIColor(1f, 0.25f, 0.25f)]
#if UNITY_EDITOR
        private void RemoveThisStyle()
        {
            if (ManagerRef != null)
            {
                ManagerRef.dependentStyles.Remove(this);
                AssetDatabase.RemoveObjectFromAsset(this);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning($"Can't remove style {name} because ManagerRef is null.");
            }
        }
#endif

        public bool TryGetEventDefinition(string key, out ElementAnimationEventDependent def)
        {
            def = eventDefinitions.FirstOrDefault(e => e.eventKey == key);
            return (def != null);
        }

        private void UpdateAssetName()
        {
            name = _styleName;
        }

        private void OnEnable()
        {
            ForceParentStyleForAll();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ForceParentStyleForAll();
        }
#endif

        private void ForceParentStyleForAll()
        {
            for (int i = 0; i < eventDefinitions.Count; i++)
            {
                if (eventDefinitions[i] != null)
                {
                    eventDefinitions[i].SetParentStyle(this);
                }
            }
        }
    }
}
