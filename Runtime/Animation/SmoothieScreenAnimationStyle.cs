using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    [HideMonoScript]
    public class SmoothieScreenAnimationStyle : ScriptableObject
    {
        // Reference to the parent manager
        [HideInInspector]
        public SmoothieScreenAnimationManager ManagerRef;

        [SerializeField, OnValueChanged("UpdateAssetName")]
        [LabelText("Style Name")]
        private string _styleName;

        [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [LabelText("Overrides")]
        public List<ShowHideDependentDefinition> showHideDefinitions = 
            new List<ShowHideDependentDefinition>();

        public string styleName
        {
            get => _styleName;
            set => _styleName = value;
        }

        // Red button for removing this style
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

        public void SynchronizeWithBase(List<ShowHideBaseDefinition> baseDefinitions)
        {
            var newList = new List<ShowHideDependentDefinition>();

            foreach (var baseDef in baseDefinitions)
            {
                var existing = showHideDefinitions.FirstOrDefault(d => d.key == baseDef.key);
                if (existing != null)
                {
                    newList.Add(existing);
                }
                else
                {
                    newList.Add(new ShowHideDependentDefinition
                    {
                        key = baseDef.key,
                        duration = baseDef.duration,
                        useSpring = baseDef.useSpring
                    });
                }
            }

            showHideDefinitions = newList;
        }

        private void UpdateAssetName()
        {
            name = _styleName;
        }

        public bool TryGetDefinition(string key, out ShowHideDependentDefinition def)
        {
            def = showHideDefinitions.FirstOrDefault(d => d.key == key);
            return (def != null);
        }
    }

    [System.Serializable]
    public class ShowHideDependentDefinition
    {
        [HideInInspector]
        public string key;

        [LabelText("Duration")]
        public float duration;

        [LabelText("Spring?")]
        public bool useSpring;
    }
}