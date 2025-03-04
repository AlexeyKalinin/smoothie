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
        fileName = "SmoothieElementAnimationManager",
        menuName = "Smoothie/Animation/Element Animation Manager")]
    public class SmoothieElementAnimationManager : ScriptableObject
    {
        [TitleGroup("Configuration")]
        [HorizontalGroup("Configuration/Split")]
        [VerticalGroup("Configuration/Split/Left")]
        [BoxGroup("Configuration/Split/Left/Events")]
        [LabelText("Possible Events")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<string> possibleEvents = new List<string>()
        {
            "PointerEnter",
            "PointerExit",
            "PointerClick",
            "Press",
            "Release"
        };

        [VerticalGroup("Configuration/Split/Right")]
        [BoxGroup("Configuration/Split/Right/UI Elements")]
        [LabelText("Possible UI Elements")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<string> possibleUIElements = new List<string>()
        {
            "Background",
            "Text",
            "Icon",
            "Border",
            "Shadow"
        };

        [TitleGroup("Animation Styles", "Available Animation Styles")]
        [Button("Add New Style", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        public void AddNewDependentStyle()
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

        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowIndexLabels = false)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public List<SmoothieElementAnimationStyle> dependentStyles = new List<SmoothieElementAnimationStyle>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Проставляем ManagerRef для стилей, у которых она не заполнена (для старых стилей)
            foreach (var style in dependentStyles)
            {
                if (style != null && style.ManagerRef == null)
                {
                    style.ManagerRef = this;
                }
            }

            // Удаляем из ассета те стили, которых уже нет в списке
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

            // Отложенное сохранение, чтобы избежать ошибки во время импорта ассетов
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

        [Button("Event Reference Guide", ButtonSizes.Medium)]
        private void ShowEventReferenceGuide()
        {
            // Show a quick reference for events in the console
            string guide = "Event Reference Guide:\n";
            
            foreach (var evt in possibleEvents)
            {
                guide += $"- {evt}: Call OnElement.{evt}() to trigger this event\n";
            }
            
            Debug.Log(guide);
        }
    }
}