using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
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
    
        // Для режима редактора
        public bool visualizeInEditor;
    
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
            if (Application.isPlaying)
            {
                if (SmoothieRuntimeManager.Instance != null &&
                    SmoothieRuntimeManager.Instance.ScreenManager != null)
                {
                    return SmoothieRuntimeManager.Instance.ScreenManager.screens;
                }
            }
            else
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
            }
            return new List<SmoothieScreen>();
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