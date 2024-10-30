using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Smoothie;

public class SmoothExample : MonoBehaviour
{
    [SerializeField]
    FloatInterpolator.Config _interpolator = FloatInterpolator.Config.Direct;
    private SmoothFloat _smoothFloat;
    [SerializeField]
    Vector2Interpolator.Config _interpolatorV2 = Vector2Interpolator.Config.Direct;
    private SmoothVector2 _smoothAnchorMin;
    private SmoothVector2 _smoothAnchorMax;
    private Vector3 _cachedPosition;
    RectTransform rect;

    void Start()
    {
        _cachedPosition = transform.position;
        rect = GetComponent<RectTransform>();
        _smoothFloat = new SmoothFloat(this, transform.position.y, _interpolator.interpolationType, _interpolator.interpolationSpeed, _interpolator.interpolationElasticity, value => UpdatePositionY(value));
        _smoothAnchorMin = new SmoothVector2(this, transform.position, _interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity, value => UpdateAnchorMin (value));
        _smoothAnchorMax = new SmoothVector2(this, transform.position, _interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity, value => UpdateAnchorMax (value));
    }

    private void UpdatePositionY(float newY)
    {
        _cachedPosition.y = newY;
        _smoothFloat.UpdateConfig(_interpolator.interpolationType, _interpolator.interpolationSpeed, _interpolator.interpolationElasticity);
        transform.position = _cachedPosition;
    }

    private void UpdateAnchorMin(Vector2 newPosition)
    {
        _smoothAnchorMin.UpdateConfig(_interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity);
        rect.anchorMin = newPosition;
    }
    private void UpdateAnchorMax(Vector2 newPosition)
    {
        _smoothAnchorMax.UpdateConfig(_interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity);
        rect.anchorMax = newPosition;
    }

    public void Test(float test)
    {
        _smoothFloat.SetValue(test);
    }
    public void SetAnchorMin(Vector2 test)
    {
        _smoothAnchorMin.SetValue(test);
    }
    public void SetAnchorMax(Vector2 test)
    {
        _smoothAnchorMax.SetValue(test);
    }
}