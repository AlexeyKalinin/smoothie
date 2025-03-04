using UnityEngine;
using Sirenix.OdinInspector;

namespace Smoothie
{
    /// <summary>
    /// Simplified animation action for UI elements
    /// </summary>
    [System.Serializable]
    public class ElementAnimationAction
    {
        [HideInInspector]
        private ElementAnimationUIElement parentElement;

        public void SetParentElement(ElementAnimationUIElement parent)
        {
            parentElement = parent;
        }

        [HideLabel]
        public AnimationActionType actionType = AnimationActionType.ChangeColor;

        [HorizontalGroup("Settings")]
        [LabelText("Duration"), LabelWidth(60)]
        [Range(0.01f, 2f)]
        public float duration = 0.3f;

        [HorizontalGroup("Settings")]
        [LabelText("Color"), LabelWidth(40)]
        [ValueDropdown("GetColorKeysFromTheme")]
        public string colorKey;

        private System.Collections.Generic.IEnumerable<string> GetColorKeysFromTheme()
        {
#if UNITY_EDITOR
            var runtimeManager = UnityEngine.Object.FindObjectOfType<SmoothieRuntimeManager>();
            if (runtimeManager != null && runtimeManager.ColorScheme != null)
            {
                return System.Linq.Enumerable.Select(runtimeManager.ColorScheme.baseThemeDefinitions, d => d.key);
            }
#endif
            return new[] { "Color1", "Color2" };
        }
    }

    /// <summary>
    /// Animation action types
    /// </summary>
    public enum AnimationActionType
    {
        ChangeColor
    }
}