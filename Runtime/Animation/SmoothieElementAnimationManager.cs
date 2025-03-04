using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieElementAnimationManager", menuName = "Smoothie/Animation/Element Animation Manager")]
    public class SmoothieElementAnimationManager : ScriptableObject
    {
        // --- 1) Сворачиваемая группа для Events ---
        [FoldoutGroup("Events", expanded: false)]
        [LabelText("Events")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<string> possibleEvents = new List<string>()
        {
            "Normal"
        };

        // --- 2) Сворачиваемая группа для UI Elements ---
        [FoldoutGroup("UI Elements", expanded: false)]
        [LabelText("UI Elements")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<string> possibleUIElements = new List<string>()
        {
            "Move",
            "Background",
            "Text"
        };

        // --- 3) Несворачиваемая секция «Styles» ---
        //[BoxGroup("Styles", showLabel: false)]
        [LabelText("Styles")]
        [ListDrawerSettings(
            ShowIndexLabels = false,
            ShowPaging = false,
            ShowItemCount = false,
            HideAddButton = true,
            DraggableItems = false,
            HideRemoveButton = true,
            Expanded = true)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public List<SmoothieElementAnimationStyle> dependentStyles = new List<SmoothieElementAnimationStyle>();

        [Button("Add New Style", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        public void AddNewStyle()
        {
            var newStyle = ScriptableObject.CreateInstance<SmoothieElementAnimationStyle>();
            newStyle.styleName = "New Style";
            newStyle.name = newStyle.styleName;
            newStyle.ManagerRef = this;

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(newStyle, this);
#endif
            dependentStyles.Add(newStyle);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Устанавливаем ссылку ManagerRef для всех стилей, если её нет
            foreach (var style in dependentStyles)
            {
                if (style != null && style.ManagerRef == null)
                {
                    style.ManagerRef = this;
                }
            }

            // Удаляем из ассета стили, которые больше не в списке
            var stylesInAsset = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this))
                .OfType<SmoothieElementAnimationStyle>()
                .ToList();

            foreach (var style in stylesInAsset)
            {
                if (!dependentStyles.Contains(style))
                {
                    AssetDatabase.RemoveObjectFromAsset(style);
                }
            }

            // Отложенное сохранение
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isCompiling &&
                    !EditorApplication.isUpdating &&
                    this != null)
                {
                    AssetDatabase.SaveAssets();
                }
            };
        }
#endif
    }
}
