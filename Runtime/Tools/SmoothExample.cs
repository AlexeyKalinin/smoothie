using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Smoothie;

public class SmoothExample : MonoBehaviour
{
    [SerializeField]
    FloatInterpolator.Config _interpolator = FloatInterpolator.Config.Direct;
    private SmoothieFloat _smoothieFloat;
    [SerializeField]
    Vector2Interpolator.Config _interpolatorV2 = Vector2Interpolator.Config.Direct;
    private SmoothieVector2 _smoothieAnchorMin;
    private SmoothieVector2 _smoothieAnchorMax;
    private Vector3 _cachedPosition;
    RectTransform rect;

    void Start()
    {
        _cachedPosition = transform.position;
        rect = GetComponent<RectTransform>();
        _smoothieFloat = new SmoothieFloat(this, transform.position.y, _interpolator.interpolationType, _interpolator.interpolationSpeed, _interpolator.interpolationElasticity, value => UpdatePositionY(value));
        _smoothieAnchorMin = new SmoothieVector2(this, transform.position, _interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity, value => UpdateAnchorMin (value));
        _smoothieAnchorMax = new SmoothieVector2(this, transform.position, _interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity, value => UpdateAnchorMax (value));
    }

    private void UpdatePositionY(float newY)
    {
        _cachedPosition.y = newY;
        _smoothieFloat.UpdateConfig(_interpolator.interpolationType, _interpolator.interpolationSpeed, _interpolator.interpolationElasticity);
        transform.position = _cachedPosition;
    }

    private void UpdateAnchorMin(Vector2 newPosition)
    {
        _smoothieAnchorMin.UpdateConfig(_interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity);
        rect.anchorMin = newPosition;
    }
    private void UpdateAnchorMax(Vector2 newPosition)
    {
        _smoothieAnchorMax.UpdateConfig(_interpolatorV2.interpolationType, _interpolatorV2.interpolationSpeed, _interpolatorV2.interpolationElasticity);
        rect.anchorMax = newPosition;
    }

    public void Test(float test)
    {
        _smoothieFloat.SetValue(test);
    }
    public void SetAnchorMin(Vector2 test)
    {
        _smoothieAnchorMin.SetValue(test);
    }
    public void SetAnchorMax(Vector2 test)
    {
        _smoothieAnchorMax.SetValue(test);
    }
}