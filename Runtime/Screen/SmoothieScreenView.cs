using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;
#endif

namespace Smoothie
{
    [ExecuteAlways]
    public class SmoothieScreenView : MonoBehaviour
    {
        #if UNITY_EDITOR
        [ValueDropdown("GetScreensDropdown")]
        #endif
        public SmoothieScreen screen;

        // Only store the name of the selected screen animation style
        [SerializeField]
        private string selectedScreenAnimationStyle;

        public bool visualizeInEditor;

        public void PlayShowAnimation()
        {
            var animManager = SmoothieRuntimeManager.Instance?.ScreenAnimationManager;
            if (animManager == null) return;

            var style = animManager.dependentStyles
                .Find(st => st != null && st.styleName == selectedScreenAnimationStyle);
            if (style == null) return;

            // Look for base definition named "Show"
            if (style.TryGetDefinition("Show", out var def))
            {
                Debug.Log($"[SmoothieScreenView] Show animation: duration={def.duration}, useSpring={def.useSpring}");
                // TODO: Implement actual tween or any other animation here
            }
        }

        public void PlayHideAnimation()
        {
            var animManager = SmoothieRuntimeManager.Instance?.ScreenAnimationManager;
            if (animManager == null) return;

            var style = animManager.dependentStyles
                .Find(st => st != null && st.styleName == selectedScreenAnimationStyle);
            if (style == null) return;

            // Look for base definition named "Hide"
            if (style.TryGetDefinition("Hide", out var def))
            {
                Debug.Log($"[SmoothieScreenView] Hide animation: duration={def.duration}, useSpring={def.useSpring}");
                // TODO: Implement actual tween or any other animation here
            }
        }

        public void ApplyVisualize()
        {
            var elements = GetComponentsInChildren<SmoothieElement>(true);
            foreach (var element in elements)
            {
                element.gameObject.SetActive(visualizeInEditor);
            }
        }

        public void RefreshColor()
        {
            var elements = GetComponentsInChildren<SmoothieElement>(true);
            foreach (var element in elements)
            {
                element.RefreshColor();
            }
        }

        #if UNITY_EDITOR
        private IEnumerable<SmoothieScreen> GetScreensDropdown()
        {
            string[] guids = AssetDatabase.FindAssets("t:SmoothieScreenManager");
            if (guids != null && guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var manager = AssetDatabase.LoadAssetAtPath<SmoothieScreenManager>(assetPath);
                if (manager != null)
                {
                    return manager.screens;
                }
            }
            return new List<SmoothieScreen>();
        }

        [ShowInInspector, LabelText("Screen Animation Style")]
        [ValueDropdown("GetScreenAnimationStyles")]
        public string ScreenAnimationStyleName
        {
            get => selectedScreenAnimationStyle;
            set => selectedScreenAnimationStyle = value;
        }

        private IEnumerable<string> GetScreenAnimationStyles()
        {
            var animManager = SmoothieRuntimeManager.Instance?.ScreenAnimationManager;
            if (animManager == null) return new List<string>();

            var list = new List<string>();
            foreach (var style in animManager.dependentStyles)
            {
                if (style != null && !string.IsNullOrEmpty(style.styleName))
                    list.Add(style.styleName);
            }
            return list;
        }
        #endif

        private void Awake()
        {
            if (SmoothieRuntimeManager.Instance != null)
                SmoothieRuntimeManager.Instance.RegisterView(this);
        }

        private void OnDestroy()
        {
            if (SmoothieRuntimeManager.Instance != null)
                SmoothieRuntimeManager.Instance.UnregisterView(this);
        }
    }
}
