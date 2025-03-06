using System;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    /// <summary>
    /// Base theme definition that stores all theme parameters and their actions
    /// </summary>
    [CreateAssetMenu(fileName = "New Theme Definition", menuName = "Smoothie/Theme Definition")]
    public class ThemeDefinition : ScriptableObject
    {
        [SerializeField] private List<ThemeParameter> parameters = new List<ThemeParameter>();
        
        /// <summary>
        /// List of all theme parameters defined in this theme
        /// </summary>
        public IReadOnlyList<ThemeParameter> Parameters => parameters;

        /// <summary>
        /// Add a new parameter to the theme definition
        /// </summary>
        public void AddParameter(string name, ThemeParameterType type)
        {
            parameters.Add(new ThemeParameter
            {
                Name = name,
                Type = type
            });
        }
        
        /// <summary>
        /// Remove a parameter from the theme definition
        /// </summary>
        public void RemoveParameter(int index)
        {
            if (index >= 0 && index < parameters.Count)
            {
                parameters.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// Move parameter up in the list
        /// </summary>
        public void MoveParameterUp(int index)
        {
            if (index > 0 && index < parameters.Count)
            {
                var param = parameters[index];
                parameters.RemoveAt(index);
                parameters.Insert(index - 1, param);
            }
        }
        
        /// <summary>
        /// Move parameter down in the list
        /// </summary>
        public void MoveParameterDown(int index)
        {
            if (index >= 0 && index < parameters.Count - 1)
            {
                var param = parameters[index];
                parameters.RemoveAt(index);
                parameters.Insert(index + 1, param);
            }
        }
    }

    /// <summary>
    /// Defines a single parameter in a theme with its type and associated action
    /// </summary>
    [Serializable]
    public class ThemeParameter
    {
        // Для параметров Space и Divider имя можно оставить пустым.
        public string Name;
        public ThemeParameterType Type;
        public List<ThemeAction> Actions = new List<ThemeAction>();
    }

    /// <summary>
    /// Types of parameters that can be defined in a theme
    /// </summary>
    public enum ThemeParameterType
    {
        Title,
        Color,
        Float,
        Vector3,
        Space,    // Новый тип: пустое пространство между элементами
        Divider   // Новый тип: разделительная линия
    }

    /// <summary>
    /// Defines an action that can be applied with a theme parameter
    /// </summary>
    [Serializable]
    public class ThemeAction
    {
        public ThemeActionType ActionType;
        public UnityEngine.Object TargetObject;
        public string PropertyName;
    }

    /// <summary>
    /// Types of actions that can be performed when applying a theme parameter
    /// </summary>
    public enum ThemeActionType
    {
        None,
        ChangeMaterialColor,
        ChangeLightColor,
        ChangeMaterialProperty,
        ChangeLightIntensity,
        ChangePosition,
        ChangeRotation,
        ChangeScale,
        CustomAction
    }
}
