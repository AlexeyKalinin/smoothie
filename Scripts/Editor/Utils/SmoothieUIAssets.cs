using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieUIAssets", menuName = "Smoothie/Smoothie UI Assets")]
    public class SmoothieUIAssets : ScriptableObject
    {
        public Texture2D backIcon;
        public Texture2D duplicateIcon;
        public Texture2D deleteIcon;
        public Texture2D editIcon;

        // При желании можно добавить поля-тинты
        public Color backIconTint = Color.white;
        public Color duplicateIconTint = Color.white;
        public Color deleteIconTint = Color.white;
        public Color editIconTint = Color.white;

        private static SmoothieUIAssets _instance;
        public static SmoothieUIAssets Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    string[] guids = AssetDatabase.FindAssets("t:SmoothieUIAssets", new[] { "Assets/Smoothie" });
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var candidate = AssetDatabase.LoadAssetAtPath<SmoothieUIAssets>(path);
                        if (candidate != null && candidate.GetType() == typeof(SmoothieUIAssets))
                        {
                            _instance = candidate;
                            break;
                        }
                    }
#else
                    _instance = Resources.Load<SmoothieUIAssets>("SmoothieUIAssets");
#endif
                }
                return _instance;
            }
        }
    }
}