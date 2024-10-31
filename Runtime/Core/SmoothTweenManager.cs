using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    public static class SmoothFloatTweens
    {
        private static List<SmoothFloat> _activeTweens = new List<SmoothFloat>();

        public static SmoothFloat Value(MonoBehaviour caller, float initialValue, FloatInterpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<float> onUpdate)
        {
            SmoothFloat smoothFloat = new SmoothFloat(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothFloat);
            return smoothFloat;
        }

        public static void RemoveTween(SmoothFloat tween)
        {
            _activeTweens.Remove(tween);
        }
    }

    public static class SmoothVector2Tweens
    {
        private static List<SmoothVector2> _activeTweens = new List<SmoothVector2>();

        public static SmoothVector2 Value(MonoBehaviour caller, Vector2 initialValue, Vector2Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector2> onUpdate)
        {
            SmoothVector2 smoothVector2 = new SmoothVector2(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothVector2);
            return smoothVector2;
        }

        public static void RemoveTween(SmoothVector2 tween)
        {
            _activeTweens.Remove(tween);
        }
    }

    public static class SmoothVector3Tweens
    {
        private static List<SmoothVector3> _activeTweens = new List<SmoothVector3>();

        public static SmoothVector3 Value(MonoBehaviour caller, Vector3 initialValue, Vector3Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector3> onUpdate)
        {
            SmoothVector3 smoothVector3 = new SmoothVector3(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothVector3);
            return smoothVector3;
        }

        public static void RemoveTween(SmoothVector3 tween)
        {
            _activeTweens.Remove(tween);
        }
    }
}