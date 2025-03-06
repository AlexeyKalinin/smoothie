using UnityEngine;
using UnityEditor;

namespace Smoothie
{
    /// <summary>
    /// Menu items for Smoothie theme system
    /// </summary>
    public static class ThemeMenuItems
    {
        [MenuItem("GameObject/Smoothie/Theme Manager", false, 10)]
        public static void CreateThemeManager()
        {
            if (UnityEngine.Object.FindFirstObjectByType<ThemeManager>() != null)
            {
                EditorUtility.DisplayDialog("Smoothie", "There is already a Theme Manager in the scene.", "OK");
                return;
            }
            
            var go = new GameObject("Smoothie Theme Manager");
            go.AddComponent<ThemeManager>();
            Selection.activeGameObject = go;
            
            Undo.RegisterCreatedObjectUndo(go, "Create Theme Manager");
        }
        
        [MenuItem("GameObject/Smoothie/UI Theme Handler", false, 11)]
        public static void CreateUIThemeHandler()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Smoothie", "Please select a GameObject first.", "OK");
                return;
            }
            
            Undo.AddComponent<UIThemeHandler>(selected);
        }
    }
}