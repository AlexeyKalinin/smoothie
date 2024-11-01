using System;
using UnityEngine;
using Smoothie;

public class TransformInterpolator
{
    private SmoothVector3 smoothPosition, smoothRotation, smoothScale;
    private FloatInterpolator.Config interpolatorConfig;
    private MonoBehaviour context;

    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 initialScale;

    public TransformInterpolator(MonoBehaviour context, FloatInterpolator.Config interpolatorConfig, Vector3 initialPosition, Vector3 initialRotation, Vector3 initialScale)
    {
        this.context = context;
        this.interpolatorConfig = interpolatorConfig;
        this.initialPosition = initialPosition;
        this.initialRotation = initialRotation;
        this.initialScale = initialScale;
    }

    public void ApplyTransform(Vector3 fromPosition, Vector3 toPosition, FloatInterpolator.Config config, Transform movable, bool resetInterpolator, Action<Vector3> updatePosition)
    {
        this.interpolatorConfig = config;
        HandleTransform(ref smoothPosition, fromPosition, toPosition, updatePosition, resetInterpolator);
    }

    public void ApplyScaleTransform(Vector3 fromScale, Vector3 toScale, FloatInterpolator.Config config, Transform target, bool resetInterpolator, Action<Vector3> updateScale)
    {
        this.interpolatorConfig = config;
        HandleTransform(ref smoothScale, fromScale, toScale, updateScale, resetInterpolator);
    }

    public void ApplyRotationTransform(Vector3 fromRotation, Vector3 toRotation, FloatInterpolator.Config config, Transform target, bool resetInterpolator, Action<Vector3> updateRotation)
    {
        this.interpolatorConfig = config;
        HandleTransform(ref smoothRotation, fromRotation, toRotation, updateRotation, resetInterpolator);
    }

    private void HandleTransform(ref SmoothVector3 smoothVector, Vector3 from, Vector3 to, Action<Vector3> updateAction, bool resetInterpolator)
    {
        if (resetInterpolator || smoothVector == null)
        {
            smoothVector = new SmoothVector3(context, from, (Vector3Interpolator.Config.InterpolationType)interpolatorConfig.interpolationType, interpolatorConfig.interpolationSpeed, interpolatorConfig.interpolationElasticity, updateAction);
        }
        else
        {
            smoothVector.UpdateConfig((Vector3Interpolator.Config.InterpolationType)interpolatorConfig.interpolationType, interpolatorConfig.interpolationSpeed, interpolatorConfig.interpolationElasticity);
        }
        smoothVector.SetValue(to);
    }

    public void ApplyTransform(SmoothieConfig.EventsConfig config, Transform movable, Transform rotatable, Vector3 initialPosition, Vector3 initialRotation, Vector3 initialScale, Action<Vector3> updatePosition, Action<Vector3> updateRotation, Action<Vector3> updateScale, bool resetInterpolator)
    {
        this.interpolatorConfig = config.interpolatorConfig;

        if (config.ChangePos && movable != null)
        {
            Vector3 from = movable.localPosition;
            Vector3 to = initialPosition + config.moveFromTo;
            HandleTransform(ref smoothPosition, from, to, updatePosition, resetInterpolator);
        }
        if (config.ChangeRotation && rotatable != null)
        {
            Vector3 from = rotatable.localRotation.eulerAngles;
            Vector3 to = initialRotation + config.rotation;
            HandleTransform(ref smoothRotation, from, to, updateRotation, resetInterpolator);
        }
        if (config.ChangeScale && rotatable != null)
        {
            Vector3 from = rotatable.localScale;
            Vector3 to = Vector3.Scale(initialScale, config.scale);
            HandleTransform(ref smoothScale, from, to, updateScale, resetInterpolator);
        }
    }

    public void Stop()
    {
        smoothPosition?.Stop();
        smoothRotation?.Stop();
        smoothScale?.Stop();
    }
}
