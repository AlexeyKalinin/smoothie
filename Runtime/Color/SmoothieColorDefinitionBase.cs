using UnityEngine;
using Sirenix.OdinInspector;

namespace Smoothie
{
    [System.Serializable]
    public class SmoothieColorDefinitionBase
    {
        [HorizontalGroup("Row", MaxWidth = 150)]
        [HideLabel]
        public string key;

        [HorizontalGroup("Row")]
        [HideLabel]
        public Color color;
    }
}
