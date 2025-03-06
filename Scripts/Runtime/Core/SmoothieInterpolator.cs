using System.Diagnostics;
using UnityEngine;
using System;

namespace Smoothie
{
    public abstract class BaseInterpolator<T>
    {
        [Serializable]
        public class Config
        {
            public enum InterpolationType { Direct, Exponential, DampedSpring, Spring }

            [SerializeField]
            private InterpolationType _interpolationType = InterpolationType.DampedSpring;

            public InterpolationType interpolationType
            {
                get { return _interpolationType; }
                set { _interpolationType = value; }
            }

            public bool enabled
            {
                get { return interpolationType != InterpolationType.Direct; }
            }

            [SerializeField, Range(0.001f, 500)]
            private float _interpolationSpeed = 10;

            public float interpolationSpeed
            {
                get { return _interpolationSpeed; }
                set { _interpolationSpeed = value; }
            }

            [SerializeField, Range(0.01f, 5)]
            private float _interpolationElasticity = 0.5f;

            public float interpolationElasticity
            {
                get { return _interpolationElasticity; }
                set { _interpolationElasticity = value; }
            }

            public Config() { }

            public Config(InterpolationType type, float speed, float elasticity)
            {
                _interpolationType = type;
                _interpolationSpeed = speed;
                _interpolationElasticity = elasticity;
            }

            public static Config Direct
            {
                get { return new Config(InterpolationType.Direct, 10, 0.5f); }
            }

            public static Config Quick
            {
                get { return new Config(InterpolationType.DampedSpring, 50, 0.5f); }
            }
        }

        protected T _velocity;
        public Config config { get; set; }
        public T currentValue { get; set; }
        public T targetValue { get; set; }

        public abstract T Step(T targetValue);
        public abstract T Step();
    }

    public class FloatInterpolator : BaseInterpolator<float>
    {
        public FloatInterpolator(float initialValue, Config config)
        {
            this.config = config;
            currentValue = targetValue = initialValue;
            _velocity = 0;
        }

        public float GetVelocity()
        {
            return _velocity;
        }

        public override float Step(float targetValue)
        {
            this.targetValue = targetValue;
            return Step();
        }

        public override float Step()
        {
            if (config.interpolationType == Config.InterpolationType.Exponential)
            {
                currentValue = ETween.Step(currentValue, targetValue, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.DampedSpring)
            {
                currentValue = DTween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.Spring)
            {
                currentValue = STween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed, config.interpolationElasticity);
            }
            else
            {
                currentValue = targetValue;
            }
            return currentValue;
        }
    }

    public class Vector2Interpolator : BaseInterpolator<Vector2>
    {
        public Vector2Interpolator(Vector2 initialValue, Config config)
        {
            this.config = config;
            currentValue = targetValue = initialValue;
            _velocity = Vector2.zero;
        }
        public Vector2 GetVelocity()
        {
            return _velocity;
        }
        public override Vector2 Step(Vector2 targetValue)
        {
            this.targetValue = targetValue;
            return Step();
        }

        public override Vector2 Step()
        {
            if (config.interpolationType == Config.InterpolationType.Exponential)
            {
                currentValue = ETween.Step(currentValue, targetValue, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.DampedSpring)
            {
                currentValue = DTween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.Spring)
            {
                currentValue = STween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed, config.interpolationElasticity);
            }
            else
            {
                currentValue = targetValue;
            }
            return currentValue;
        }
    }

    public class Vector3Interpolator : BaseInterpolator<Vector3>
    {
        public Vector3Interpolator(Vector3 initialValue, Config config)
        {
            this.config = config;
            currentValue = targetValue = initialValue;
            _velocity = Vector3.zero;
        }
        public Vector3 GetVelocity()
        {
            return _velocity;
        }
        public override Vector3 Step(Vector3 targetValue)
        {
            this.targetValue = targetValue;
            return Step();
        }

        public override Vector3 Step()
        {
            if (config.interpolationType == Config.InterpolationType.Exponential)
            {
                currentValue = ETween.Step(currentValue, targetValue, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.DampedSpring)
            {
                currentValue = DTween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed);
            }
            else if (config.interpolationType == Config.InterpolationType.Spring)
            {
                currentValue = STween.Step(currentValue, targetValue, ref _velocity, config.interpolationSpeed, config.interpolationElasticity);
            }
            else
            {
                currentValue = targetValue;
            }
            return currentValue;
        }
    }
}
