using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [System.Serializable]
    public class ElementAnimationEventDependent
    {
        [HideInInspector]
        private SmoothieElementAnimationStyle parentStyle;

        public void SetParentStyle(SmoothieElementAnimationStyle style)
        {
            parentStyle = style;
        }

        [ValueDropdown("GetAllPossibleEvents")]
        [LabelText("Event Key")]
        public string eventKey;

        [LabelText("Duration")]
        [Range(0.01f, 5f)]
        public float duration = 0.3f;

        [LabelText("Spring?")]
        public bool useSpring = false;

        private IEnumerable<string> GetAllPossibleEvents()
        {
            if (parentStyle != null && parentStyle.ManagerRef != null)
            {
                return parentStyle.ManagerRef.possibleEvents;
            }
            return new[] { "<No Manager>" };
        }
    }
}