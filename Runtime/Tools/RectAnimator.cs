using UnityEngine;
using UnityEngine.UI;
using Smoothie;

public class RectAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform current;

    [SerializeField] FloatInterpolator.Config _iForward = FloatInterpolator.Config.Direct;
    [SerializeField] FloatInterpolator.Config _iBack = FloatInterpolator.Config.Direct;

    private SmoothieFloat _smoothieTop;
    private SmoothieFloat _smoothieRight;
    private SmoothieFloat _smoothieBottom;
    private SmoothieFloat _smoothieLeft;
    
    void Start()
    {
        _smoothieTop = new SmoothieFloat(this, current.anchorMax.y, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMaxY(value));
        _smoothieRight = new SmoothieFloat(this, current.anchorMax.x, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMaxX(value));
        _smoothieBottom = new SmoothieFloat(this, current.anchorMin.y, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMinY(value));
        _smoothieLeft = new SmoothieFloat(this, current.anchorMin.x, _iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity, value => SetAnchorMinX(value));
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
            _smoothieRight.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
            _smoothieLeft.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
        }
        else
        {
            _smoothieRight.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
            _smoothieLeft.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
        }

        if (movingUp)
        {
            _smoothieTop.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
            _smoothieBottom.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
        }
        else
        {
            _smoothieTop.UpdateConfig(_iBack.interpolationType, _iBack.interpolationSpeed, _iBack.interpolationElasticity);
            _smoothieBottom.UpdateConfig(_iForward.interpolationType, _iForward.interpolationSpeed, _iForward.interpolationElasticity);
        }

        _smoothieTop.SetValue(targetRect.anchorMax.y);
        _smoothieRight.SetValue(targetRect.anchorMax.x);
        _smoothieBottom.SetValue(targetRect.anchorMin.y);
        _smoothieLeft.SetValue(targetRect.anchorMin.x);
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
