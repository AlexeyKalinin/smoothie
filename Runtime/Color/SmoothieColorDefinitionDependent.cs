using UnityEngine;
using Sirenix.OdinInspector;

namespace Smoothie
{
    [System.Serializable]
    public class SmoothieColorDefinitionDependent
    {
        [HideInInspector]
        public string key;

        [LabelText("$key")]
        [OnValueChanged("NotifyColorChanged")]
        public Color color;

        // Метод, который вызывается при изменении цвета
        private void NotifyColorChanged()
        {
#if UNITY_EDITOR
            // Найдем ScriptableObject (SmoothieTheme), которому принадлежит эта настройка цвета,
            // и оповестим его об изменении
            var currentSelection = UnityEditor.Selection.activeObject;
            if (currentSelection is SmoothieTheme)
            {
                // Делаем небольшую задержку, чтобы не блокировать UI редактора
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (currentSelection != null) // Убеждаемся, что объект все еще существует
                    {
                        // Помечаем объект как измененный, чтобы сработал OnValidate
                        UnityEditor.EditorUtility.SetDirty(currentSelection);
                    }
                };
            }
#endif
        }
    }

}
