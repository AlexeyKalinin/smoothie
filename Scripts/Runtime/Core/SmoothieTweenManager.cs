using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    public static class SmoothieFloatTweens
    {
        private static List<SmoothieFloat> _activeTweens = new List<SmoothieFloat>();

        public static SmoothieFloat Value(MonoBehaviour caller, float initialValue, FloatInterpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<float> onUpdate)
        {
            SmoothieFloat smoothieFloat = new SmoothieFloat(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothieFloat);
            return smoothieFloat;
        }

        public static void RemoveTween(SmoothieFloat tween)
        {
            _activeTweens.Remove(tween);
        }
    }

    public static class SmoothieVector2Tweens
    {
        private static List<SmoothieVector2> _activeTweens = new List<SmoothieVector2>();

        public static SmoothieVector2 Value(MonoBehaviour caller, Vector2 initialValue, Vector2Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector2> onUpdate)
        {
            SmoothieVector2 smoothieVector2 = new SmoothieVector2(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothieVector2);
            return smoothieVector2;
        }

        public static void RemoveTween(SmoothieVector2 tween)
        {
            _activeTweens.Remove(tween);
        }
    }

    public static class SmoothieVector3Tweens
    {
        private static List<SmoothieVector3> _activeTweens = new List<SmoothieVector3>();

        public static SmoothieVector3 Value(MonoBehaviour caller, Vector3 initialValue, Vector3Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector3> onUpdate)
        {
            SmoothieVector3 smoothieVector3 = new SmoothieVector3(caller, initialValue, interpolator, speed, elasticity, onUpdate);
            _activeTweens.Add(smoothieVector3);
            return smoothieVector3;
        }

        public static void RemoveTween(SmoothieVector3 tween)
        {
            _activeTweens.Remove(tween);
        }
    }
} 