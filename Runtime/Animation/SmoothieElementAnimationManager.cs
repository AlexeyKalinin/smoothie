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
        [LabelText("Possible Events")]
        public List<string> possibleEvents = new List<string>()
        {
            "PointerEnter",
            "PointerExit",
            "PointerClick",
            "TRR"
        };

        [Button("Add New Style", ButtonHeight = 30)]
        [GUIColor(0.25f, 1f, 0.25f)]
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

        [ListDrawerSettings(Expanded = true, DraggableItems = false)]
        [InlineEditor(Expanded = false)]
        [LabelText("Element Styles")]
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
    }
}
