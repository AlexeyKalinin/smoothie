using System;
using System.Collections;
using UnityEngine;

namespace Smoothie
{
    public abstract class SmoothValue<T>
    {
        protected MonoBehaviour _caller;
        protected System.Action<T> _onUpdate;
        protected Coroutine _currentCoroutine;
        protected float tolerance = 0.001f;
        protected float velocityTolerance = 0.01f;
        public abstract T CurrentValue { get; set; }
        public abstract void UpdateConfig(object interpolationType, float interpolationSpeed, float interpolatorElasticity);
        public abstract void SetValue(T targetValue);
        public abstract void SnapValue(T targetValue);
        public bool IsComplete { get; protected set; }
        public event Action OnAnimationCompleted;
        protected void RaiseOnAnimationCompleted()
        {
            OnAnimationCompleted?.Invoke();
        }
        public void Stop()
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
        }
    }

    public class SmoothFloat : SmoothValue<float>
    {
        private FloatInterpolator _floatInterpolator;

        public override float CurrentValue
        {
            get { return _floatInterpolator.currentValue; }
            set { _floatInterpolator.currentValue = value; }
        }

        public SmoothFloat(MonoBehaviour caller, float initialValue, FloatInterpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<float> onUpdate)
        {
            _caller = caller;
            FloatInterpolator.Config config = new FloatInterpolator.Config(interpolator, speed, elasticity);
            _floatInterpolator = new FloatInterpolator(initialValue, config);
            _onUpdate = onUpdate;
        }

        public override void UpdateConfig(object interpolationType, float interpolationSpeed, float interpolatorElasticity)
        {
            _floatInterpolator.config.interpolationType = (FloatInterpolator.Config.InterpolationType)interpolationType;
            _floatInterpolator.config.interpolationSpeed = interpolationSpeed;
            _floatInterpolator.config.interpolationElasticity = interpolatorElasticity;
        }

        public override void SetValue(float targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = _caller.StartCoroutine(InterpolateValue(targetValue));
        }

        public override void SnapValue(float targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _floatInterpolator.currentValue = targetValue;
            _onUpdate?.Invoke(_floatInterpolator.currentValue);
        }

        private IEnumerator InterpolateValue(float targetValue)
        {
            while (Mathf.Abs(_floatInterpolator.currentValue - targetValue) > tolerance ||
                Mathf.Abs(_floatInterpolator.GetVelocity()) > velocityTolerance)
            {
                _floatInterpolator.Step(targetValue);
                _onUpdate?.Invoke(_floatInterpolator.currentValue);
                yield return null;
            }

            SmoothFloatTweens.RemoveTween(this);
        }
    }
    
    public class SmoothVector2 : SmoothValue<Vector2>
    {
        private Vector2Interpolator _vector2Interpolator;

        public override Vector2 CurrentValue
        {
            get { return _vector2Interpolator.currentValue; }
            set { _vector2Interpolator.currentValue = value; }
        }

        public SmoothVector2(MonoBehaviour caller, Vector2 initialValue, Vector2Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector2> onUpdate)
        {
            _caller = caller;
            Vector2Interpolator.Config config = new Vector2Interpolator.Config(interpolator, speed, elasticity);
            _vector2Interpolator = new Vector2Interpolator(initialValue, config);
            _onUpdate = onUpdate;
        }

        public override void UpdateConfig(object interpolationType, float interpolationSpeed, float interpolatorElasticity)
        {
            _vector2Interpolator.config.interpolationType = (Vector2Interpolator.Config.InterpolationType)interpolationType;
            _vector2Interpolator.config.interpolationSpeed = interpolationSpeed;
            _vector2Interpolator.config.interpolationElasticity = interpolatorElasticity;
        }

        public override void SetValue(Vector2 targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = _caller.StartCoroutine(InterpolateValue(targetValue));
        }

        public override void SnapValue(Vector2 targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _vector2Interpolator.currentValue = targetValue;
            _onUpdate?.Invoke(_vector2Interpolator.currentValue);
        }

        private IEnumerator InterpolateValue(Vector2 targetValue)
        {
            while (Vector2.Distance(_vector2Interpolator.currentValue, targetValue) > tolerance ||
                Vector2.Distance(_vector2Interpolator.GetVelocity(), Vector2.zero) > velocityTolerance)
            {
                _vector2Interpolator.Step(targetValue);
                _onUpdate?.Invoke(_vector2Interpolator.currentValue);
                yield return null;
            }

            SmoothVector2Tweens.RemoveTween(this);
        }
    }

    public class SmoothVector3 : SmoothValue<Vector3>
    {
        private Vector3Interpolator _vector3Interpolator;

        public override Vector3 CurrentValue
        {
            get { return _vector3Interpolator.currentValue; }
            set { _vector3Interpolator.currentValue = value; }
        }
        
        protected void SetIsComplete(bool value)
        {
            IsComplete = value;
        }

        public SmoothVector3(MonoBehaviour caller, Vector3 initialValue, Vector3Interpolator.Config.InterpolationType interpolator, float speed, float elasticity, System.Action<Vector3> onUpdate)
        {
            _caller = caller;
            Vector3Interpolator.Config config = new Vector3Interpolator.Config(interpolator, speed, elasticity);
            _vector3Interpolator = new Vector3Interpolator(initialValue, config);
            _onUpdate = onUpdate;
        }

        public override void UpdateConfig(object interpolationType, float interpolationSpeed, float interpolatorElasticity)
        {
            _vector3Interpolator.config.interpolationType = (Vector3Interpolator.Config.InterpolationType)interpolationType;
            _vector3Interpolator.config.interpolationSpeed = interpolationSpeed;
            _vector3Interpolator.config.interpolationElasticity = interpolatorElasticity;
        }

        public override void SetValue(Vector3 targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _currentCoroutine = _caller.StartCoroutine(InterpolateValue(targetValue));
        }

        public override void SnapValue(Vector3 targetValue)
        {
            if (_currentCoroutine != null)
            {
                _caller.StopCoroutine(_currentCoroutine);
            }
            _vector3Interpolator.currentValue = targetValue;
            _onUpdate?.Invoke(_vector3Interpolator.currentValue);
        }

        private IEnumerator InterpolateValue(Vector3 targetValue)
        {
            while (Vector3.Distance(_vector3Interpolator.currentValue, targetValue) > tolerance ||
                Vector3.Distance(_vector3Interpolator.GetVelocity(), Vector3.zero) > velocityTolerance)
            {
                _vector3Interpolator.Step(targetValue);
                _onUpdate?.Invoke(_vector3Interpolator.currentValue);
                yield return null;
            }

            SmoothVector3Tweens.RemoveTween(this);
            SetIsComplete(true);
            RaiseOnAnimationCompleted();
        }
    }
}