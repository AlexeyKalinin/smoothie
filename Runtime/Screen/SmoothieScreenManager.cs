#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieScreenManager", menuName = "Smoothie/SmoothieScreenManager")]
    public class SmoothieScreenManager : ScriptableObject
    {
        [ListDrawerSettings(Expanded = true)]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public List<SmoothieScreen> screens = new List<SmoothieScreen>();

    #if UNITY_EDITOR
        [Title("Editor Visualization")]
        [LabelText("Visualize All In Editor")]
        [OnValueChanged("OnVisualizeAllInEditorChanged", true)]
        public bool visualizeAllInEditor;

        [System.NonSerialized]
        private List<SmoothieScreen> oldScreens = new List<SmoothieScreen>();
    #endif

        public void UpdateScreensStatus()
        {
            if (SmoothieRuntimeManager.Instance == null) return;
            foreach (var screen in screens)
            {
                screen.instancesCount = SmoothieRuntimeManager.Instance.GetInstancesCount(screen);
                screen.isShown = SmoothieRuntimeManager.Instance.GetIsShown(screen);
            }
        }

        [Button("Add New Screen", ButtonHeight = 30)]
        [GUIColor(0.25f, 1f, 0.25f)]
        public void AddNewScreen()
        {
            SmoothieScreen newScreen = CreateInstance<SmoothieScreen>();
            newScreen.screenName = "New Screen";
            newScreen.name = newScreen.screenName;
            newScreen.ScreenManagerRef = this;
            screens.Add(newScreen);

        #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(newScreen, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newScreen));
        #endif
        }

    #if UNITY_EDITOR
        private void OnEnable()
        {
            oldScreens.Clear();
            oldScreens.AddRange(screens);
            
            // Set manager reference for all screens
            foreach (var screen in screens)
            {
                if (screen != null)
                {
                    screen.ScreenManagerRef = this;
                }
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // 1) Находим экраны, которые были удалены из списка
                var removed = new List<SmoothieScreen>();
                foreach (var oldScr in oldScreens)
                {
                    if (!screens.Contains(oldScr))
                        removed.Add(oldScr);
                }

                // 2) Удаляем их из ассета
                foreach (var scr in removed)
                {
                    if (scr != null)
                    {
                        AssetDatabase.RemoveObjectFromAsset(scr);
                    }
                }

                // 3) Обновляем копию списка
                oldScreens.Clear();
                oldScreens.AddRange(screens);
                
                // 4) Set manager reference for all screens
                foreach (var screen in screens)
                {
                    if (screen != null)
                    {
                        screen.ScreenManagerRef = this;
                    }
                }

                // 4) Вместо прямого AssetDatabase.SaveAssets() – делаем это отложенно
                EditorApplication.delayCall += SaveAssetsDelayed;
            }
        }

        private void SaveAssetsDelayed()
        {
            if (this != null)  // проверяем, что объект не удалён
            {
                AssetDatabase.SaveAssets();
                // Debug.Log("[SmoothieScreenManager] Assets saved by delayed call.");
            }
        }

        private void OnVisualizeAllInEditorChanged()
        {
            if (Application.isPlaying)
                return;
            // Для каждого экрана в списке установить галку в соответствии с глобальным переключателем
            foreach (var screen in screens)
            {
                screen.visualizeInEditor = visualizeAllInEditor;
                screen.OnVisualizeChanged();
            }
        }
    #endif
    }
}