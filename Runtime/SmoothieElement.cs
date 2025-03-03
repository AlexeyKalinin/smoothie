using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
#endif

namespace Smoothie
{
    [ExecuteAlways]
    public class SmoothieElement : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private bool useOverrideColor;
        [SerializeField, ShowIf("useOverrideColor")] private Color overrideColor = Color.white;

        #if UNITY_EDITOR
        [ValueDropdown("GetColorKeys")]
        [OnValueChanged("RefreshColor")]
        #endif
        [SerializeField] private string colorKey;

        // Storing only the style name
        [SerializeField]
        private string selectedElementAnimationStyle;

        private void OnEnable()
        {
            RefreshColor();
            if (SmoothieRuntimeManager.Instance != null)
            {
                SmoothieRuntimeManager.Instance.OnThemeChanged += RefreshColor;
                SmoothieRuntimeManager.Instance.OnColorsUpdated += RefreshColor;
            }
        }

        private void OnDisable()
        {
            if (SmoothieRuntimeManager.Instance != null)
            {
                SmoothieRuntimeManager.Instance.OnThemeChanged -= RefreshColor;
                SmoothieRuntimeManager.Instance.OnColorsUpdated -= RefreshColor;
            }
        }

        public void RefreshColor()
        {
            if (image == null) return;

            if (useOverrideColor)
            {
                image.color = overrideColor;
            }
            else
            {
                var mgr = SmoothieRuntimeManager.Instance;
                if (mgr != null && !string.IsNullOrEmpty(colorKey))
                {
                    image.color = mgr.GetAnimatedColor(colorKey);
                }
            }
        }

        // Example: on pointer enter
        public void OnPointerEnter()
        {
            var manager = SmoothieRuntimeManager.Instance?.ElementAnimationManager;
            if (manager == null) return;

            // Find the selected style
            var style = manager.dependentStyles
                .Find(s => s != null && s.styleName == selectedElementAnimationStyle);
            if (style == null) return;

            // Find the event definition with key "PointerEnter"
            if (style.TryGetEventDefinition("PointerEnter", out var def))
            {
                Debug.Log($"[SmoothieElement] PointerEnter => duration={def.duration}, useSpring={def.useSpring}");
                // TODO: Implement actual tween, e.g., animate scale or color
            }
        }

        #if UNITY_EDITOR
        private IEnumerable<string> GetColorKeys()
        {
            if (Application.isPlaying)
            {
                if (SmoothieRuntimeManager.Instance != null && SmoothieRuntimeManager.Instance.ColorScheme != null)
                    return SmoothieRuntimeManager.Instance.ColorScheme.baseThemeDefinitions.Select(d => d.key);
            }
            else
            {
                string[] guids = AssetDatabase.FindAssets("t:SmoothieColorScheme");
                if (guids != null && guids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var scheme = AssetDatabase.LoadAssetAtPath<SmoothieColorScheme>(assetPath);
                    if (scheme != null)
                        return scheme.baseThemeDefinitions.Select(d => d.key);
                }
            }
            return new List<string>();
        }

        [ShowInInspector, LabelText("Element Animation Style")]
        [ValueDropdown("GetElementAnimationStyles")]
        public string ElementAnimationStyleName
        {
            get => selectedElementAnimationStyle;
            set => selectedElementAnimationStyle = value;
        }

        private IEnumerable<string> GetElementAnimationStyles()
        {
            var manager = SmoothieRuntimeManager.Instance?.ElementAnimationManager;
            if (manager == null) return new List<string>();

            return manager.dependentStyles
                .Where(s => s != null && !string.IsNullOrEmpty(s.styleName))
                .Select(s => s.styleName);
        }

        private void OnValidate()
        {
            RefreshColor();
        }
        #endif
    }
}
