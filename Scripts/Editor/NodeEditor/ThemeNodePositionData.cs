using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Smoothie.Editor.NodeEditor
{
    // ScriptableObject to store theme node positions
    [Serializable]
    public class ThemeNodePositionData : ScriptableObject
    {
        private const string PositionAssetSuffix = "_NodePositions";
        
        [Serializable]
        public class ThemePosition
        {
            public string themeId;  // Using theme name for identification
            public Vector2 position;
        }
        
        public List<ThemePosition> positions = new List<ThemePosition>();
        
        public Vector2? GetPositionForTheme(Theme theme)
        {
            string id = GetThemeId(theme);
            var position = positions.FirstOrDefault(p => p.themeId == id);
            return position != null ? (Vector2?)position.position : null;
        }
        
        public void SetPositionForTheme(Theme theme, Vector2 position)
        {
            string id = GetThemeId(theme);
            var existing = positions.FirstOrDefault(p => p.themeId == id);
            
            if (existing != null)
            {
                existing.position = position;
            }
            else
            {
                positions.Add(new ThemePosition { themeId = id, position = position });
            }
        }
        
        private string GetThemeId(Theme theme)
        {
            // Use theme name as identifier
            return theme.ThemeName;
        }
        
        public static ThemeNodePositionData GetOrCreateForDefinition(ThemeDefinition definition)
        {
            if (definition == null)
                return null;
                
            string assetPath = AssetDatabase.GetAssetPath(definition);
            
            // Try to find existing position data
            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            foreach (var asset in assets)
            {
                if (asset is ThemeNodePositionData positionData)
                    return positionData;
            }
            
            // If not found, create a new one
            var newPositionData = ScriptableObject.CreateInstance<ThemeNodePositionData>();
            newPositionData.name = definition.name + PositionAssetSuffix;
            AssetDatabase.AddObjectToAsset(newPositionData, definition);
            AssetDatabase.SaveAssets();
            return newPositionData;
        }
    }
}