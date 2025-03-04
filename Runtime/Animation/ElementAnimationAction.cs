using Sirenix.OdinInspector;
using UnityEngine;

namespace Smoothie
{
    /// <summary>
    /// Перечисление для типов действий анимации
    /// </summary>
    public enum AnimationActionType
    {
        ChangeColor,
        Move,
        Scale,
        Rotate,
        Fade
    }

    /// <summary>
    /// Класс, представляющий действие анимации для элемента UI
    /// </summary>
    [System.Serializable]
    public class ElementAnimationAction
    {
        [HideInInspector]
        private ElementAnimationUIElement parentElement;

        public void SetParentElement(ElementAnimationUIElement parent)
        {
            parentElement = parent;
        }

        [LabelText("Action Type")]
        public AnimationActionType actionType;

        [LabelText("Duration")]
        [Range(0.01f, 5f)]
        public float duration = 0.3f;

        [LabelText("Delay")]
        [Range(0f, 2f)]
        public float delay = 0f;

        [LabelText("Use Spring")]
        public bool useSpring = false;

        [LabelText("Ease Type")]
        [ShowIf("@!useSpring")]
        public PrimeTween.Ease ease = PrimeTween.Ease.InOutSine;

        #region ChangeColor Properties
        [ShowIf("@actionType == AnimationActionType.ChangeColor")]
        [LabelText("Target Color")]
        public Color targetColor = Color.white;

        [ShowIf("@actionType == AnimationActionType.ChangeColor")]
        [LabelText("Use Theme Color")]
        public bool useThemeColor = false;

        [ShowIf("@actionType == AnimationActionType.ChangeColor && useThemeColor")]
        [LabelText("Color Key")]
        [ValueDropdown("GetColorKeysFromTheme")]
        public string colorKey;
        #endregion

        #region Move Properties
        [ShowIf("@actionType == AnimationActionType.Move")]
        [LabelText("Move Direction")]
        public Vector2 moveDirection = Vector2.zero;

        [ShowIf("@actionType == AnimationActionType.Move")]
        [LabelText("Relative Movement")]
        public bool relativeMovement = true;
        #endregion

        #region Scale Properties
        [ShowIf("@actionType == AnimationActionType.Scale")]
        [LabelText("Uniform Scale")]
        public bool uniformScale = true;

        [ShowIf("@actionType == AnimationActionType.Scale && uniformScale")]
        [LabelText("Scale Factor")]
        [Range(0.01f, 3f)]
        public float scaleFactor = 1.1f;

        [ShowIf("@actionType == AnimationActionType.Scale && !uniformScale")]
        [LabelText("Target Scale")]
        public Vector2 targetScale = Vector2.one;
        #endregion

        #region Rotate Properties
        [ShowIf("@actionType == AnimationActionType.Rotate")]
        [LabelText("Rotation Angle")]
        [Range(-360f, 360f)]
        public float rotationAngle = 0f;
        #endregion

        #region Fade Properties
        [ShowIf("@actionType == AnimationActionType.Fade")]
        [LabelText("Target Alpha")]
        [Range(0f, 1f)]
        public float targetAlpha = 1f;
        #endregion

        private System.Collections.Generic.IEnumerable<AnimationActionType> GetAllPossibleActions()
        {
            return System.Enum.GetValues(typeof(AnimationActionType)) as AnimationActionType[];
        }

        private System.Collections.Generic.IEnumerable<string> GetColorKeysFromTheme()
        {
#if UNITY_EDITOR
            var runtimeManager = UnityEngine.Object.FindObjectOfType<SmoothieRuntimeManager>();
            if (runtimeManager != null && runtimeManager.ColorScheme != null)
            {
                return System.Linq.Enumerable.Select(runtimeManager.ColorScheme.baseThemeDefinitions, d => d.key);
            }
            return System.Linq.Enumerable.Empty<string>();
#else
            return System.Linq.Enumerable.Empty<string>();
#endif
        }
    }
}