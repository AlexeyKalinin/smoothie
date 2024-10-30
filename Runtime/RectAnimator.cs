using UnityEngine;
using UnityEngine.UI;
using Smoothie;

public class RectAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform current;

    [SerializeField] FloatInterpolator.Config _iForward = FloatInterpolator.Config.Direct;
    [SerializeField] FloatInterpolator.Config _iBack = FloatInterpolator.Config.Direct;

    private SmoothFloat _smoothTop;
    private SmoothFloat _smoothRight;
    private SmoothFloat _smoothBottom;
    private SmoothFloat _smoothLeft;
    
    void Start()
    {
        _smoothTop = new SmoothFloat(this, current.anchorMax.y, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMaxY(value));
        _smoothRight = new SmoothFloat(this, current.anchorMax.x, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMaxX(value));
        _smoothBottom = new SmoothFloat(this, current.anchorMin.y, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMinY(value));
        _smoothLeft = new SmoothFloat(this, current.anchorMin.x, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMinX(value));
    }

    public void MoveTo(GameObject targetObject)
    {   
        if (targetObject == null)
            return;
        
        RectTransform targetRect = targetObject.GetComponent<RectTransform>();
        
        if (targetRect == null)
            return;

        float deltaX = targetRect.localPosition.x - current.localPosition.x;
        float deltaY = targetRect.localPosition.y - current.localPosition.y;
        bool movingRight = deltaX > 0;
        bool movingUp = deltaY > 0;

        if (movingRight)
        {
            _smoothRight.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
            _smoothLeft.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
        }
        else
        {
            _smoothRight.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
            _smoothLeft.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
        }

        if (movingUp)
        {
            _smoothTop.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
            _smoothBottom.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
        }
        else
        {
            _smoothTop.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
            _smoothBottom.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
        }

        _smoothTop.SetValue(targetRect.anchorMax.y);
        _smoothRight.SetValue(targetRect.anchorMax.x);
        _smoothBottom.SetValue(targetRect.anchorMin.y);
        _smoothLeft.SetValue(targetRect.anchorMin.x);
    }

    private void SetAnchorMinX(float value)
    {
        Vector2 anchorMin = current.anchorMin;
        anchorMin.x = value;
        current.anchorMin = anchorMin;  
    }

    private void SetAnchorMaxX(float value)
    {
        Vector2 anchorMax = current.anchorMax;
        anchorMax.x = value;
        current.anchorMax = anchorMax;
    }

    private void SetAnchorMinY(float value)
    {
        Vector2 anchorMin = current.anchorMin;
        anchorMin.y = value;
        current.anchorMin = anchorMin;
    }
    
    private void SetAnchorMaxY(float value)
    {
        Vector2 anchorMax = current.anchorMax;
        anchorMax.y = value;
        current.anchorMax = anchorMax;
    }

    public void MoveToImmediately(RectTransform targetRect)
    {
        SetRect(targetRect);
    }

    private void SetRect(RectTransform source)
    {
        current.anchorMin = source.anchorMin;
        current.anchorMax = source.anchorMax;
        current.pivot = source.pivot;
        current.anchoredPosition = source.anchoredPosition;
        current.localPosition = source.localPosition;
        current.localRotation = source.localRotation;
    }
}
