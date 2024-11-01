using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using Smoothie;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SmoothieUIScreen : MonoBehaviour
{
    public List<SmoothieUIElement> Elements { get; private set; } = new List<SmoothieUIElement>();

    private List<float> uniqueXPositions;
    private List<float> uniqueYPositions;
    private int numColumns;
    private int numRows;

    private Coroutine animationCoroutine;

    [ReadOnly] public bool isShown;
    [ReadOnly] public bool isHidden;
    [ReadOnly] public bool isShowing;
    [ReadOnly] public bool isHiding;

    private List<Selectable> selectables = new List<Selectable>();

    [SerializeField]
    [OnValueChanged("OnVisualizeChanged")]
    private bool visualize;
    public bool Visualize
    {
        get => visualize;
        set
        {
            visualize = value;
            OnVisualizeChanged();
        }
    }

    private void OnValidate()
    {
        if (visualize)
        {
            SetVisualizeState(true);
        }
        else
        {
            SetVisualizeState(false);
        }
    }

    private void OnVisualizeChanged()
    {
        if (visualize)
        {
            SetVisualizeState(true);
        }
        else
        {
            SetVisualizeState(false);
        }
    }

    private void SetVisualizeState(bool state)
    {
        Elements.Clear();
        Elements.AddRange(GetComponentsInChildren<SmoothieUIElement>(true));

        foreach (var element in Elements)
        {
            element.Visualize = state;
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void Awake()
    {
        Elements.Clear();
        Elements.AddRange(GetComponentsInChildren<SmoothieUIElement>(true));

        selectables.Clear();
        selectables.AddRange(GetComponentsInChildren<Selectable>(true));

        isHidden = true;

        IndexElements();

        if (!visualize)
        {
            SetHiddenImmediate();
        }
    }

    private void IndexElements()
    {
        uniqueXPositions = new List<float>();
        uniqueYPositions = new List<float>();
        float tolerance = 1f;
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();

        foreach (var element in Elements)
        {
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            xPositions.Add(anchoredPosition.x);
            yPositions.Add(anchoredPosition.y);
        }

        xPositions.Sort();
        uniqueXPositions = ClusterPositions(xPositions, tolerance);
        yPositions.Sort();
        yPositions.Reverse();
        uniqueYPositions = ClusterPositions(yPositions, tolerance);

        numColumns = uniqueXPositions.Count;
        numRows = uniqueYPositions.Count;

        foreach (var element in Elements)
        {
            RectTransform rectTransform = element.GetComponent<RectTransform>();
            Vector2 anchoredPosition = rectTransform.anchoredPosition;

            int horizontalIndex = GetIndex(uniqueXPositions, anchoredPosition.x, tolerance);
            int verticalIndex = GetIndex(uniqueYPositions, anchoredPosition.y, tolerance);

            element.HorizontalIndex = horizontalIndex;
            element.VerticalIndex = verticalIndex;
        }
    }

    private List<float> ClusterPositions(List<float> positions, float tolerance)
    {
        List<float> uniquePositions = new List<float>();

        if (positions.Count > 0)
        {
            float lastPosition = positions[0];
            uniquePositions.Add(lastPosition);

            for (int i = 1; i < positions.Count; i++)
            {
                float currentPosition = positions[i];
                if (Mathf.Abs(currentPosition - lastPosition) > tolerance)
                {
                    uniquePositions.Add(currentPosition);
                    lastPosition = currentPosition;
                }
            }
        }

        return uniquePositions;
    }

    private int GetIndex(List<float> positions, float value, float tolerance)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            if (Mathf.Abs(value - positions[i]) <= tolerance)
            {
                return i;
            }
        }
        return 0;
    }

    public void Show(string animationType)
    {
        PlayAnimation(animationType, true);
    }

    public void Hide(string animationType)
    {
        PlayAnimation(animationType, false);
    }

    private void PlayAnimation(string animationType, bool isShowAction)
    {
        if (isHidden && !isShowAction) return;
        if (isShown && isShowAction) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (isShowAction && isHidden)
        {
            foreach (var element in Elements)
            {
                element.SnapToStart(animationType);
            }
        }

        isShown = false;
        isHidden = false;
        isShowing = isShowAction;
        isHiding = !isShowAction;

        SetInteractable(false);

        animationCoroutine = StartCoroutine(PlayElementsWithDelay(animationType, isShowAction));
    }

    private IEnumerator PlayElementsWithDelay(string animationType, bool isShowAction)
    {
        List<SmoothieUIElement> sortedElements = GetSortedElements(animationType, isShowAction);

        float baseDelay = 0.035f;

        int totalElements = sortedElements.Count;
        int completedElements = 0;

        for (int i = 0; i < sortedElements.Count; i++)
        {
            SmoothieUIElement element = sortedElements[i];
            float delay = baseDelay * i;

            StartCoroutine(PlayElementWithDelay(element, delay, animationType, isShowAction, () =>
            {
                completedElements++;
                if (completedElements >= totalElements)
                {
                    isShowing = false;
                    isHiding = false;
                    isShown = isShowAction;
                    isHidden = !isShowAction;

                    SetInteractable(isShown);

                    if (isShown)
                    {
                        SelectDefaultButton();
                    }
                }
            }));
        }

        yield return null;
    }

    private IEnumerator PlayElementWithDelay(SmoothieUIElement element, float delay, string animationType, bool isShowAction, System.Action onComplete)
    {
        if (!element.gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(delay);

        if (!element.gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (isShowAction)
        {
            element.Show(animationType);
        }
        else
        {
            element.Hide(animationType);
        }
        onComplete?.Invoke();
    }

    private List<SmoothieUIElement> GetSortedElements(string animationType, bool isShow)
    {
        IComparer<SmoothieUIElement> comparer = GetComparer(animationType, isShow);

        List<SmoothieUIElement> sortedElements = new List<SmoothieUIElement>(Elements);
        sortedElements.Sort(comparer);
        return sortedElements;
    }

    private IComparer<SmoothieUIElement> GetComparer(string animationType, bool isShow)
    {
        return animationType switch
        {
            "ShowFromRight" or "HideToRight" => isShow ? new ElementComparer(true, false) : new ElementComparer(false, true),
            "ShowFromTop" or "HideToTop" => isShow ? new ElementComparer(false, true) : new ElementComparer(true, false),
            "ShowFromLeft" or "HideToLeft" => isShow ? new ElementComparer(true, true) : new ElementComparer(false, false),
            "ShowFromBottom" or "HideToBottom" => isShow ? new ElementComparer(true, true) : new ElementComparer(false, false),
            _ => new ElementComparer(true, true),
        };
    }

    private class ElementComparer : IComparer<SmoothieUIElement>
    {
        private bool verticalAscending;
        private bool horizontalAscending;

        public ElementComparer(bool verticalAscending, bool horizontalAscending)
        {
            this.verticalAscending = verticalAscending;
            this.horizontalAscending = horizontalAscending;
        }

        public int Compare(SmoothieUIElement a, SmoothieUIElement b)
        {
            int verticalCompare = verticalAscending ? a.VerticalIndex.CompareTo(b.VerticalIndex) : b.VerticalIndex.CompareTo(a.VerticalIndex);
            int horizontalCompare = horizontalAscending ? a.HorizontalIndex.CompareTo(b.HorizontalIndex) : b.HorizontalIndex.CompareTo(a.HorizontalIndex);

            if (verticalCompare != 0)
            {
                return verticalCompare;
            }
            else
            {
                return horizontalCompare;
            }
        }
    }

    private void SetInteractable(bool interactable)
    {
        foreach (var selectable in selectables)
        {
            selectable.interactable = interactable;
        }
    }

    private void SelectDefaultButton()
    {
        var buttons = GetComponentsInChildren<SmoothieUIButton>(true);

        var validButtons = new List<SmoothieUIButton>();

        foreach (var button in buttons)
        {
            if (button.SelectAfterShown >= 0 && button.gameObject.activeInHierarchy)
            {
                validButtons.Add(button);
            }
        }

        if (validButtons.Count > 0)
        {
            validButtons.Sort((a, b) => b.SelectAfterShown.CompareTo(a.SelectAfterShown));

            var buttonToSelect = validButtons[0];

            buttonToSelect.Select();
        }
    }

    private void SetHiddenImmediate()
    {
        foreach (var element in Elements)
        {
            element.SetHiddenImmediate();
        }
#if UNITY_EDITOR
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
    }

    private void SetShownImmediate()
    {
        foreach (var element in Elements)
        {
            element.SetShownImmediate();
        }
#if UNITY_EDITOR
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
    }

    [Button("Show")]
    public void EditorShow()
    {
        SetShownImmediate();
    }

    [Button("Hide")]
    public void EditorHide()
    {
        SetHiddenImmediate();
    }
}
