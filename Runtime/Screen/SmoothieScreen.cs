using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieScreen", menuName = "Smoothie/Screen")]
    public class SmoothieScreen : ScriptableObject
    {
        // Reference to the parent screen manager
        [HideInInspector]
        public SmoothieScreenManager ScreenManagerRef;

        [FoldoutGroup("$FoldoutTitle")]
        [OnValueChanged("UpdateObjectName")]
        public string screenName;
        
        [HideInInspector]
        public int instancesCount;

        [HideInInspector]
        public bool isShown;

        [FoldoutGroup("$FoldoutTitle")]
        [LabelText("Visualize in Editor")]
        [OnValueChanged("OnVisualizeChanged", true)]
        public bool visualizeInEditor;

        // Red button for removing this screen
        [FoldoutGroup("$FoldoutTitle")]
        [Button("Remove This Screen", ButtonHeight = 25)]
        [GUIColor(1f, 0.25f, 0.25f)]
        #if UNITY_EDITOR
        private void RemoveThisScreen()
        {
            if (ScreenManagerRef != null)
            {
                ScreenManagerRef.screens.Remove(this);
                AssetDatabase.RemoveObjectFromAsset(this);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning($"Can't remove screen {name} because ScreenManagerRef is null.");
                
                // Try to find parent asset
                string path = AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrEmpty(path))
                {
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (mainAsset is SmoothieScreenManager screenManager)
                    {
                        screenManager.screens.Remove(this);
                        AssetDatabase.RemoveObjectFromAsset(this);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        #endif

        [FoldoutGroup("$FoldoutTitle")]
        [Button("Toggle Screen")]
        public void ToggleScreen()
        {
            Debug.Log($"[ToggleScreen] {screenName} called. isShown = {isShown}");
            if (SmoothieRuntimeManager.Instance != null)
            {
                if (isShown)
                {
                    SmoothieRuntimeManager.Instance.HideScreen(this);
                    Debug.Log($"[ToggleScreen] Hiding screen: {screenName}");
                }
                else
                {
                    SmoothieRuntimeManager.Instance.ShowScreen(this);
                    Debug.Log($"[ToggleScreen] Showing screen: {screenName}");
                }
            }
            else
            {
                // Если хотим видеть эффект в Editor, но Manager отсутствует,
                // можно напрямую искать ScreenView и включать/выключать:
                Debug.LogWarning("SmoothieRuntimeManager.Instance not found! Trying direct approach...");
                var views = Object.FindObjectsOfType<SmoothieScreenView>(true);
                foreach (var v in views)
                {
                    if (v.screen == this)
                    {
                        var elements = v.GetComponentsInChildren<SmoothieElement>(true);
                        foreach (var e in elements)
                            e.gameObject.SetActive(!isShown);
                    }
                }
            }

            // Инвертируем флаг, чтобы визуально отразить изменение
            isShown = !isShown;
        }

    #if UNITY_EDITOR
        private void UpdateObjectName()
        {
            name = screenName;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            AssetDatabase.SaveAssets();
        }

        // Вызывается при изменении галки "Visualize in Editor" в инспекторе
        public void OnVisualizeChanged()
        {
            // Если хотим работать и в Editor, убираем if (Application.isPlaying)
            var views = Object.FindObjectsOfType<SmoothieScreenView>(true);
            foreach (var view in views)
            {
                if (view.screen == this)
                {
                    view.visualizeInEditor = visualizeInEditor;
                    view.ApplyVisualize();
                }
            }
            Debug.Log($"[OnVisualizeChanged] {screenName}, visualizeInEditor = {visualizeInEditor}");
        }
    #endif

        private string FoldoutTitle => $"{screenName} [{(isShown ? "shown" : "hidden")}] : {instancesCount}";
    }
}