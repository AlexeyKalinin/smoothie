using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Smoothie;

[CreateAssetMenu(menuName = "Smoothie/AnimationStyle")]
public class SmoothieAnimationStyles : ScriptableObject
{
    [System.Serializable]
    public class AnimationStyle
    {
        [FoldoutGroup("SHOW")]
        public ShowHideConfig showFromLeft, showFromRight, showFromTop, showFromBottom;

        [FoldoutGroup("HIDE")]
        public ShowHideConfig hideToLeft, hideToRight, hideToTop, hideToBottom;

        [FoldoutGroup("UI EVENTS")]
        public ShowHideConfig normal, over, press;

        [FoldoutGroup("MOVE")]
        public ShowHideConfig moveUp, moveRight, moveDown, moveLeft;
    }

    [System.Serializable]
    public class ShowHideConfig
    {
        [HorizontalGroup("Pos"), HideLabel, LabelText("Animate Position"), Space(12)]
        public bool ChangePos;
        [HorizontalGroup("Pos"), HideLabel, ShowIf("ChangePos"), Space(12)]
        public Vector3 moveFromTo;

        [HorizontalGroup("Rot"), HideLabel, LabelText("Animate Rotation"), Space(6)]
        public bool ChangeRotation;
        [HorizontalGroup("Rot"), HideLabel, ShowIf("ChangeRotation"), Space(6)]
        public Vector3 rotation = Vector3.zero;

        [HorizontalGroup("Scale"), HideLabel, LabelText("Animate Scale"), Space(6)]
        public bool ChangeScale;
        [HorizontalGroup("Scale"), HideLabel, ShowIf("ChangeScale"), Space(6)]
        public Vector3 scale = Vector3.one;

        [ShowIf("IsAnyChangeEnabled"), Space(16)]
        public bool FromTo;
        [LabelText("Transform Interpolation"), ShowIf("IsAnyChangeEnabled"), Space(6)]
        public FloatInterpolator.Config interpolatorConfig;
        public bool IsAnyChangeEnabled() { return ChangePos || ChangeRotation || ChangeScale;}

        //
        
        [HorizontalGroup("Colors"), HideLabel, LabelText("Animate Colors"), Space(24)]
        public bool ChangeColors;

        [HorizontalGroup("ColorBg"), HideLabel, LabelText("Background To"), ShowIf("ChangeColors"), Space(12)]
        public bool ChangeBackgroundColor;
        [HorizontalGroup("ColorBg"), HideLabel, EnableIf("ChangeBackgroundColor"), ShowIf("ChangeColors"), Space(12)]
        public SmoothieColorTheme.ColorType ChangeBackgroundColorTo;

        [HorizontalGroup("ColorText"), HideLabel, LabelText("Text To"), ShowIf("ChangeColors"), Space(6)]
        public bool ChangeTextColor;
        [HorizontalGroup("ColorText"), HideLabel, EnableIf("ChangeTextColor"), ShowIf("ChangeColors"), Space(6)]
        public SmoothieColorTheme.ColorType ChangeTextColorTo;

        [HorizontalGroup("ColorSelect"), HideLabel, LabelText("Select To"), ShowIf("ChangeColors"), Space(6)]
        public bool ChangeSelectColor;
        [HorizontalGroup("ColorSelect"), HideLabel, EnableIf("ChangeSelectColor"), ShowIf("ChangeColors"), Space(6)]
        public SmoothieColorTheme.ColorType ChangeSelectColorTo;

        [HorizontalGroup("ColorShadow"), HideLabel, LabelText("Shadow To"), ShowIf("ChangeColors"), Space(6)]
        public bool ChangeShadowColor;
        [HorizontalGroup("ColorShadow"), HideLabel, EnableIf("ChangeShadowColor"), ShowIf("ChangeColors"), Space(6)]
        public SmoothieColorTheme.ColorType ChangeShadowColorTo;

        [HorizontalGroup("Alpha"), ShowIf("ChangeColors"), HideLabel, LabelText("Alpha To"), Space(12)]
        public bool ChangeAlpha;

        [EnableIf("ChangeAlpha"), Range(0, 1), HorizontalGroup("Alpha"), HideLabel, ShowIf("ChangeColors"), Space(12)]
        public float alphaMultiplier;

        [LabelText("Color Interpolation"), ShowIf("ChangeColors"), Space(12), Range(0.1f, 100)]
        public float colorInterpolatorSpeed = 1.5f;
    }

    public AnimationStyle style;

    public bool TryGetConfig(string animationType, out ShowHideConfig config)
    {
        config = animationType switch
        {
            "ShowFromLeft" => style.showFromLeft,
            "ShowFromRight" => style.showFromRight,
            "ShowFromTop" => style.showFromTop,
            "ShowFromBottom" => style.showFromBottom,
            "HideToLeft" => style.hideToLeft,
            "HideToRight" => style.hideToRight,
            "HideToTop" => style.hideToTop,
            "HideToBottom" => style.hideToBottom,
            "Normal" => style.normal,
            "Over" => style.over,
            "Press" => style.press,
            "MoveUp" => style.moveUp,
            "MoveRight" => style.moveRight,
            "MoveDown" => style.moveDown,
            "MoveLeft" => style.moveLeft,
            _ => null,
        };
        return config != null;
    }
}