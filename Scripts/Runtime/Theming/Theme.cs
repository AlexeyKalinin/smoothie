using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Smoothie
{
    /// <summary>
    /// A Theme asset that contains actual values for a ThemeDefinition
    /// </summary>
    public class Theme : ScriptableObject
    {
        [SerializeField] private string themeName;
        [SerializeField] private ThemeDefinition parentDefinition;
        [SerializeField] private List<ThemeParameterValue> parameterValues = new List<ThemeParameterValue>();
        
        /// <summary>
        /// Name of this theme
        /// </summary>
        public string ThemeName
        {
            get => themeName;
            set => themeName = value;
        }
        
        /// <summary>
        /// The parent theme definition this theme is based on
        /// </summary>
        public ThemeDefinition ParentDefinition => parentDefinition;
        
        /// <summary>
        /// Initialize this theme with a parent definition
        /// </summary>
        public void Initialize(ThemeDefinition definition)
        {
            parentDefinition = definition;
            SynchronizeWithDefinition();
        }
        
        /// <summary>
        /// Synchronize parameter values with the parent definition
        /// </summary>
        public void SynchronizeWithDefinition()
        {
            if (parentDefinition == null)
                return;
                
            var definitionParams = parentDefinition.Parameters;
            
            // Remove any values that don't exist in the definition anymore
            parameterValues.RemoveAll(v => !definitionParams.Any(p => p.Name == v.ParameterName));
                
            // Add any new parameters from the definition
            foreach (var param in definitionParams)
            {
                bool exists = parameterValues.Exists(v => v.ParameterName == param.Name);
                
                if (!exists)
                {
                    var newValue = new ThemeParameterValue
                    {
                        ParameterName = param.Name,
                        Type = param.Type,
                        Enabled = true // Enable by default
                    };
                    
                    // Set default values based on type
                    switch (param.Type)
                    {
                        case ThemeParameterType.Color:
                            newValue.ColorValue = Color.white;
                            break;
                        case ThemeParameterType.Float:
                            newValue.FloatValue = 1.0f;
                            break;
                        case ThemeParameterType.Vector3:
                            newValue.VectorValue = Vector3.zero;
                            break;
                    }
                    
                    parameterValues.Add(newValue);
                }
            }
        }
        
        /// <summary>
        /// Get a parameter value by name
        /// </summary>
        public ThemeParameterValue GetParameterValue(string name)
        {
            return parameterValues.Find(v => v.ParameterName == name);
        }
        
        /// <summary>
        /// Check if a parameter is enabled in this theme
        /// </summary>
        public bool IsParameterEnabled(string parameterName)
        {
            var param = GetParameterValue(parameterName);
            if (param == null)
                return false;
                
            return param.Enabled;
        }
        
        /// <summary>
        /// Apply this theme to all registered handlers
        /// </summary>
        public void Apply()
        {
            ThemeManager.Instance.ApplyTheme(this);
        }
    }

    /// <summary>
    /// Value container for a theme parameter
    /// </summary>
    [Serializable]
    public class ThemeParameterValue
    {
        public string ParameterName;
        public ThemeParameterType Type;
        public bool Enabled = true;
        
        // Values for each type
        public Color ColorValue;
        public float FloatValue;
        public Vector3 VectorValue;
    }
}