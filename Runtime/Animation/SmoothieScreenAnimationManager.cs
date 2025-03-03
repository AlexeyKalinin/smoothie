using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace Smoothie
{
    [CreateAssetMenu(
        fileName = "SmoothieScreenAnimationManager",
        menuName = "Smoothie/Animation/Screen Animation Manager")]
    public class SmoothieScreenAnimationManager : ScriptableObject
    {
        [FoldoutGroup("Base", expanded: true)]
        [ListDrawerSettings(Expanded = true)]
        [LabelText("Show/Hide Definitions")]
        public List<ShowHideBaseDefinition> baseShowHideDefinitions =
            new List<ShowHideBaseDefinition>();

        [Button("Add New Style", ButtonHeight = 30)]
        [GUIColor(0.25f, 1f, 0.25f)]
        public void AddNewDependentStyle()
        {
            var newStyle = ScriptableObject.CreateInstance<SmoothieScreenAnimationStyle>();
            newStyle.styleName = "New Style";
            newStyle.name = newStyle.styleName;
            newStyle.ManagerRef = this;
            newStyle.SynchronizeWithBase(baseShowHideDefinitions);

        #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(newStyle, this);
        #endif
            dependentStyles.Add(newStyle);
        }

        [ListDrawerSettings(Expanded = true)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [LabelText("Screen Styles")]
        public List<SmoothieScreenAnimationStyle> dependentStyles =
            new List<SmoothieScreenAnimationStyle>();

        [System.NonSerialized]
        private List<SmoothieScreenAnimationStyle> oldDependentStyles =
            new List<SmoothieScreenAnimationStyle>();

        private void OnEnable()
        {
            oldDependentStyles.Clear();
            oldDependentStyles.AddRange(dependentStyles);
            
            // Set manager reference for all styles
            foreach (var style in dependentStyles)
            {
                if (style != null)
                {
                    style.ManagerRef = this;
                }
            }
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            var removed = oldDependentStyles.Except(dependentStyles).ToList();
            foreach (var r in removed)
            {
                if (r != null) AssetDatabase.RemoveObjectFromAsset(r);
            }

            foreach (var style in dependentStyles)
            {
                if (style != null)
                {
                    style.ManagerRef = this;
                    style.SynchronizeWithBase(baseShowHideDefinitions);
                }
            }

            oldDependentStyles.Clear();
            oldDependentStyles.AddRange(dependentStyles);

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

    [System.Serializable]
    public class ShowHideBaseDefinition
    {
        [LabelText("Key")]
        public string key; // e.g. "Show", "Hide"

        [LabelText("Duration")]
        public float duration = 0.5f;

        [LabelText("Spring?")]
        public bool useSpring = false;
    }
}