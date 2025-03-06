using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Smoothie
{
    /// <summary>
    /// Base class for components that handle theme application
    /// </summary>
    public abstract class ThemeHandlerBase : MonoBehaviour, IThemeHandler
    {
        protected virtual void OnEnable()
        {
            ThemeManager.Instance.RegisterHandler(this);
        }

        protected virtual void OnDisable()
        {
            ThemeManager.Instance.UnregisterHandler(this);
        }

        /// <summary>
        /// Apply the new theme to this handler
        /// </summary>
        public virtual void OnThemeChanged(Theme newTheme)
        {
            if (newTheme?.ParentDefinition == null)
                return;
                
            foreach (var parameter in newTheme.ParentDefinition.Parameters)
            {
                // Skip title parameters as they don't have actions
                if (parameter.Type == ThemeParameterType.Title)
                    continue;
                    
                var paramName = parameter.Name;
                var paramValue = newTheme.GetParameterValue(paramName);
                
                // Skip null or disabled parameters
                if (paramValue == null || !newTheme.IsParameterEnabled(paramName))
                    continue;
                    
                foreach (var action in parameter.Actions)
                {
                    ApplyAction(action, paramValue);
                }
            }
        }

        /// <summary>
        /// Apply a single action from a theme parameter value
        /// </summary>
        protected virtual void ApplyAction(ThemeAction action, ThemeParameterValue paramValue)
        {
            if (action.TargetObject == null)
                return;
                
            switch (action.ActionType)
            {
                case ThemeActionType.ChangeMaterialColor when action.TargetObject is Material material && paramValue.Type == ThemeParameterType.Color:
                    material.color = paramValue.ColorValue;
                    break;
                    
                case ThemeActionType.ChangeLightColor when action.TargetObject is Light light && paramValue.Type == ThemeParameterType.Color:
                    light.color = paramValue.ColorValue;
                    break;
                    
                case ThemeActionType.ChangeMaterialProperty when action.TargetObject is Material material && !string.IsNullOrEmpty(action.PropertyName):
                    ApplyMaterialProperty(material, action.PropertyName, paramValue);
                    break;
                    
                case ThemeActionType.ChangeLightIntensity when action.TargetObject is Light light && paramValue.Type == ThemeParameterType.Float:
                    light.intensity = paramValue.FloatValue;
                    break;
                    
                case ThemeActionType.ChangePosition when action.TargetObject is Transform transform && paramValue.Type == ThemeParameterType.Vector3:
                    transform.position = paramValue.VectorValue;
                    break;
                    
                case ThemeActionType.ChangeRotation when action.TargetObject is Transform transform && paramValue.Type == ThemeParameterType.Vector3:
                    transform.eulerAngles = paramValue.VectorValue;
                    break;
                    
                case ThemeActionType.ChangeScale when action.TargetObject is Transform transform && paramValue.Type == ThemeParameterType.Vector3:
                    transform.localScale = paramValue.VectorValue;
                    break;
            }
        }
        
        /// <summary>
        /// Apply a parameter value to a material property
        /// </summary>
        private void ApplyMaterialProperty(Material material, string propertyName, ThemeParameterValue value)
        {
            switch (value.Type)
            {
                case ThemeParameterType.Color:
                    material.SetColor(propertyName, value.ColorValue);
                    break;
                case ThemeParameterType.Float:
                    material.SetFloat(propertyName, value.FloatValue);
                    break;
                case ThemeParameterType.Vector3:
                    material.SetVector(propertyName, value.VectorValue);
                    break;
            }
        }
    }

    /// <summary>
    /// Handles applying theme parameters to UI elements
    /// </summary>
    [AddComponentMenu("Smoothie/Theme Handler/UI Theme Handler")]
    public class UIThemeHandler : ThemeHandlerBase
    {
        protected override void ApplyAction(ThemeAction action, ThemeParameterValue paramValue)
        {
            // Handle UI-specific actions
            if (paramValue.Type == ThemeParameterType.Color && action.ActionType == ThemeActionType.ChangeMaterialColor)
            {
                // Handle UI components
                if (action.TargetObject is Graphic graphic)
                {
                    graphic.color = paramValue.ColorValue;
                    return;
                }
                else if (action.TargetObject is TMP_Text textMesh)
                {
                    textMesh.color = paramValue.ColorValue;
                    return;
                }
                else if (action.TargetObject is SpriteRenderer spriteRenderer)
                {
                    spriteRenderer.color = paramValue.ColorValue;
                    return;
                }
            }
            
            // For all other actions, use the base implementation
            base.ApplyAction(action, paramValue);
        }
    }
}