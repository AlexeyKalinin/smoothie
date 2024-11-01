using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Smoothie;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Smoothie/New Config")]
public class SmoothieConfig : ScriptableObject
{
    [System.Serializable]
    public class AnimationConfig
    {
        [BoxGroup("Show"), InlineProperty, HideLabel][GUIColor(1f, 1f, 0.8f)]
        public ShowConfig show;

        [BoxGroup("Hide"), InlineProperty, HideLabel][GUIColor(1f, 0.8f, 0.8f)]
        public HideConfig hide;

        [BoxGroup("Move"), InlineProperty, HideLabel][GUIColor(0.8f, 0.9f, 1f)]
        public MoveConfig move;

        [BoxGroup("Events")]
        public EventsConfig normal, over, press;

        [BoxGroup("Focus"), InlineProperty, HideLabel][GUIColor(0.8f, 1f, 0.8f)]
        public FocusConfig focus;
    }

    [System.Serializable]
    public class ShowConfig
    {
        [TabGroup("Settings", "Position"), Space(12), LabelText("Offset Position")] public float offset = 10;
        [TabGroup("Settings", "Position"), Range(0.0f, 1), LabelText("Add Randomness")] public float offset_random = 0;
        [FormerlySerializedAs("interpolatorConfig")] [TabGroup("Settings", "Position"), LabelText("Position Interpolation"), Space(6)]
        public FloatInterpolator.Config positionInterpolatorConfig;

        [TabGroup("Settings", "Scale"), Space(12), Range(0, 2), LabelText("Squeeze Main")] public float scale_main = 1;
        [TabGroup("Settings", "Scale"), Range(0, 2), LabelText("Squeeze Secondary")] public float scale_secondary = 1;
        [TabGroup("Settings", "Scale"), Range(0.0f, 1), LabelText("Add Randomness")] public float scale_random = 1;
        [TabGroup("Settings", "Scale"), LabelText("Scale Interpolation"), Space(6)]
        public FloatInterpolator.Config scaleInterpolatorConfig;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float tilt = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@tilt != 0")] public bool tilt_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@tilt != 0")] public float tilt_random = 0;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float yaw = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@yaw != 0")] public bool yaw_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@yaw != 0")] public float yaw_random = 0;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float roll = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@roll != 0")] public bool roll_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@roll != 0")] public float roll_random = 0;

        [TabGroup("Settings", "Rotation"), LabelText("Rotation Interpolation"), Space(6)]
        public FloatInterpolator.Config rotationInterpolatorConfig;

        [TabGroup("Settings", "Fade")]
        [LabelText("Fade In Transition (sec)"), Space(12), Range(0.0f, 5)]
        public float fadeIn = 0.15f;
    }

    [System.Serializable]
    public class HideConfig
    {
        [TabGroup("Settings", "Position"), Space(12), LabelText("Offset Position")] public float offset = 10;
        [TabGroup("Settings", "Position"), Range(0.0f, 1), LabelText("Add Randomness")] public float offset_random = 0;
        [FormerlySerializedAs("interpolatorConfig")] [TabGroup("Settings", "Position"), LabelText("Position Interpolation"), Space(6)]
        public FloatInterpolator.Config positionInterpolatorConfig;

        [TabGroup("Settings", "Scale"), Space(12), Range(0, 2), LabelText("Squeeze Main")] public float scale_main = 1;
        [TabGroup("Settings", "Scale"), Range(0, 2), LabelText("Squeeze Secondary")] public float scale_secondary = 1;
        [TabGroup("Settings", "Scale"), Range(0.0f, 1), LabelText("Add Randomness")] public float scale_random = 1;
        [TabGroup("Settings", "Scale"), LabelText("Scale Interpolation"), Space(6)]
        public FloatInterpolator.Config scaleInterpolatorConfig;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float tilt = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@tilt != 0")] public bool tilt_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@tilt != 0")] public float tilt_random = 0;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float yaw = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@yaw != 0")] public bool yaw_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@yaw != 0")] public float yaw_random = 0;

        [TabGroup("Settings", "Rotation"), Space(12), Range(-360, 360)] public float roll = 0.0f;
        [TabGroup("Settings", "Rotation"), HideLabel, LabelText("Use Negative"), EnableIf("@roll != 0")] public bool roll_negative = false;
        [TabGroup("Settings", "Rotation"), Range(-360, 360), HideLabel, LabelText("Add Randomness"), EnableIf("@roll != 0")] public float roll_random = 0;

        [TabGroup("Settings", "Rotation"), LabelText("Rotation Interpolation"), Space(6)]
        public FloatInterpolator.Config rotationInterpolatorConfig;

        [TabGroup("Settings", "Fade"), LabelText("Fade Out Transition (sec)"), Space(12), Range(0.0f, 5)]
        public float fadeOut = 0.15f;
    }

    [System.Serializable]
    public class EventsConfig
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

        [LabelText("Transform Interpolation"), ShowIf("IsAnyChangeEnabled"), Space(6)]
        public FloatInterpolator.Config interpolatorConfig;
        public bool IsAnyChangeEnabled() { return ChangePos || ChangeRotation || ChangeScale; }

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

        [LabelText("Color Transition Duration"), ShowIf("ChangeColors"), Space(12), Range(0.1f, 100)]
        public float colorTransitionDuration = 0.15f;
    }

    [System.Serializable]
    public class MoveConfig
    {
        [Space(12)] public float offset = 10;
        [LabelText("Interpolation"), Space(6)] public FloatInterpolator.Config interpolatorConfig;
        [LabelText("Return Delay (sec)"), Space(6)] public float returnDelay = 0.5f;
    }

    [System.Serializable]
    public class FocusConfig
    {
        [Space(12)] public float deselectedScale = 0.8f;
        [LabelText("Interpolation"), Space(6)] public FloatInterpolator.Config interpolatorConfig;
        [LabelText("Fade Duration (sec)"), Space(6)] public float fadeDuration = 0.2f;
    }

    [InlineProperty, HideLabel]
    public AnimationConfig style;

    public bool TryGetConfig(string animationType, out ShowConfig config)
    {
        config = animationType switch
        {
            "ShowFromLeft" => style.show,
            "ShowFromRight" => style.show,
            "ShowFromTop" => style.show,
            "ShowFromBottom" => style.show,
            _ => null,
        };
        return config != null;
    }

    public bool TryGetConfig(string animationType, out HideConfig config)
    {
        config = animationType switch
        {
            "HideToLeft" => style.hide,
            "HideToRight" => style.hide,
            "HideToTop" => style.hide,
            "HideToBottom" => style.hide,
            _ => null,
        };
        return config != null;
    }

    public bool TryGetConfig(string animationType, out EventsConfig config)
    {
        config = animationType switch
        {
            "Normal" => style.normal,
            "Over" => style.over,
            "Press" => style.press,
            _ => null,
        };
        return config != null;
    }

    public bool TryGetConfig(string animationType, out MoveConfig config)
    {
        config = animationType switch
        {
            "MoveUp" => style.move,
            "MoveRight" => style.move,
            "MoveDown" => style.move,
            "MoveLeft" => style.move,
            _ => null,
        };
        return config != null;
    }

    public FocusConfig GetFocusConfig()
    {
        return style.focus;
    }
}
