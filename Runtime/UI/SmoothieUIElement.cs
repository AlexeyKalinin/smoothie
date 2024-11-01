using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Smoothie;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SmoothieUIElement : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private int horizontalIndex;
    public int HorizontalIndex
    {
        get => horizontalIndex;
        set => horizontalIndex = value;
    }

    [SerializeField, ReadOnly]
    private int verticalIndex;
    public int VerticalIndex
    {
        get => verticalIndex;
        set => verticalIndex = value;
    }

    public SmoothieConfig smoothieConfig;
    public Vector4 padding;
    public RectTransform targetRect, movableRect, rotatableRect;
    public TextMeshProUGUI textMesh;

    public RectTransform selectRect;

    [Serializable, InlineProperty]
    public class GraphicGroup
    {
        [HideLabel, ValueDropdown("GetColorTypes")]
        public SmoothieColorTheme.ColorType selectedColorType;

        [HideLabel, ListDrawerSettings(Expanded = true), Space(3)]
        public Graphic[] graphics;

        internal Coroutine transitionCoroutine;

        private ValueDropdownList<SmoothieColorTheme.ColorType> GetColorTypes()
        {
            var colorTypes = new ValueDropdownList<SmoothieColorTheme.ColorType>();
            foreach (SmoothieColorTheme.ColorType colorType in Enum.GetValues(typeof(SmoothieColorTheme.ColorType)))
            {
                colorTypes.Add(colorType.ToString(), colorType);
            }
            return colorTypes;
        }
    }

    [SerializeField, Space(24)] private GraphicGroup background;
    [SerializeField, Space(12)] private GraphicGroup text;
    [SerializeField, Space(12)] private GraphicGroup select;
    [SerializeField, Space(12)] private GraphicGroup shadow;

    private TransformInterpolator transformInterpolator;
    private SmoothieColorTheme ActualTheme => GetComponentInParent<SmoothieUIBase>()?.ActualTheme;

    private Vector3 initialPosition;
    private Vector3 initialRotation;
    [SerializeField] // Добавлено сериализуемое поле для сохранения значения
    private Vector3 initialScale;

    private Vector3 currentBasePosition;
    private Vector3 additionalOffset = Vector3.zero;
    private SmoothVector3 additionalOffsetInterpolator;
    private Coroutine moveReturnCoroutine;

    private float focusAlphaMultiplier = 0f;
    private SmoothVector3 focusScaleInterpolator;
    private Coroutine focusAlphaCoroutine;
    private Vector3 focusDeselectedScale;

    private float alphaMultiplier = 1f;
    private float currentColorTransitionDuration = 0.15f;

    [ReadOnly] public bool isShown = false;
    [ReadOnly] public bool isHidden = true;
    [ReadOnly] public bool isShowing = false;
    [ReadOnly] public bool isHiding = false;

    private Coroutine animationCoroutine;
    private Coroutine focusAnimationCoroutine;

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
        Initialize();

        if (ActualTheme != null)
        {
            UpdateColorsImmediate();
        }

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
        if (state)
        {
            SetShownImmediate();
        }
        else
        {
            SetHiddenImmediate();
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void Awake()
    {
        Initialize();
        if (!visualize)
        {
            SetHiddenImmediate();
        }
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (movableRect != null)
        {
            initialPosition = movableRect.localPosition;
            currentBasePosition = initialPosition;
        }
        else
        {
            initialPosition = Vector3.zero;
            currentBasePosition = Vector3.zero;
        }

        if (rotatableRect != null)
        {
            initialRotation = rotatableRect.localRotation.eulerAngles;

            // Устанавливаем initialScale только в редакторе или если оно не установлено
            if (Application.isPlaying)
            {
                if (initialScale == Vector3.zero)
                {
                    initialScale = rotatableRect.localScale;
                    if (initialScale == Vector3.zero)
                    {
                        initialScale = Vector3.one;
                    }
                }
            }
            else
            {
                initialScale = rotatableRect.localScale;
                if (initialScale == Vector3.zero)
                {
                    initialScale = Vector3.one;
                }
            }
        }
        else
        {
            initialRotation = Vector3.zero;
            initialScale = Vector3.one;
        }

        transformInterpolator = new TransformInterpolator(this, new FloatInterpolator.Config(), initialPosition, initialRotation, initialScale);

        if (ActualTheme != null)
        {
            ActualTheme.OnThemeChanged -= UpdateColors;
            ActualTheme.OnThemeChanged += UpdateColors;
        }

        if (selectRect != null && smoothieConfig != null)
        {
            var focusConfig = smoothieConfig.GetFocusConfig();

            focusDeselectedScale = Vector3.one * focusConfig.deselectedScale;
            selectRect.localScale = focusDeselectedScale;
            focusAlphaMultiplier = 0f;
            UpdateColorsImmediate();
        }
    }

    private void OnDisable()
    {
        if (ActualTheme != null) ActualTheme.OnThemeChanged -= UpdateColors;
    }

    private void Start()
    {
        UpdateColors();
        ApplySize();
    }

    public void ApplySize() => SmoothieSizeHandler.ApplySize(targetRect, textMesh, padding);


    public void Show(string animationType)
    {
        Animate(animationType, isShowAction: true, resetInterpolator: true);
    }

    public void Hide(string animationType)
    {
        Animate(animationType, isShowAction: false, resetInterpolator: true);
    }

    public void Animate(string animationType, bool isShowAction, bool resetInterpolator, bool isStateChange = false)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (animationType == "Focus" || animationType == "Unfocus")
        {
            HandleFocusAnimation(animationType, resetInterpolator);
            return;
        }

        if (!isStateChange)
        {
            if (isHidden && !isShowAction) return;
            if (isShown && isShowAction) return;

            isShown = false;
            isHidden = false;
            isShowing = isShowAction;
            isHiding = !isShowAction;

            SetInteractable(false);

            if (isHiding)
            {
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                {
                    if (EventSystem.current.currentSelectedGameObject.transform.IsChildOf(this.transform))
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }
                }

                Animate("Normal", isShowAction: false, resetInterpolator: false, isStateChange: true);
            }
        }

        if (transformInterpolator == null)
        {
            Initialize();
        }

        if (smoothieConfig == null)
        {
            Debug.LogError("smoothieConfig is not assigned in SmoothieUIElement.", this);
            return;
        }

        StopAllCoroutinesSafe();

        HandleOtherAnimations(animationType, isShowAction, true, isStateChange);
    }

    private void HandleOtherAnimations(string animationType, bool isShowAction, bool resetInterpolator, bool isStateChange)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (animationType.StartsWith("Show") || animationType.StartsWith("Hide"))
        {
            HandleShowHideAnimation(animationType, isShowAction, resetInterpolator);
        }
        else if (animationType == "Normal" || animationType == "Over" || animationType == "Press")
        {
            HandleEventsAnimation(animationType, resetInterpolator);
        }
        else if (animationType.StartsWith("Move"))
        {
            HandleMoveAnimation(animationType, resetInterpolator);
        }

        if (!isStateChange)
        {
            animationCoroutine = StartCoroutine(AnimationCoroutine(animationType, isShowAction));
        }
    }

    private void HandleShowHideAnimation(string animationType, bool isShowAction, bool resetInterpolator)
    {
        if (isShowAction)
        {
            if (!smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.ShowConfig config))
            {
                Debug.LogWarning($"Animation type '{animationType}' not found in smoothieConfig.", this);
                return;
            }

            Vector3 fromPosition = initialPosition + GetOffset(animationType, config.offset);

            if (movableRect != null)
            {
                movableRect.localPosition = fromPosition;
                currentBasePosition = fromPosition;

                transformInterpolator.ApplyTransform(fromPosition, initialPosition, config.positionInterpolatorConfig, movableRect, true, UpdatePosition);
            }

            alphaMultiplier = 0f;
            UpdateColorsImmediate();
            StartCoroutine(FadeAlpha(1f, config.fadeIn));
        }
        else
        {
            if (!smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.HideConfig config))
            {
                Debug.LogWarning($"Animation type '{animationType}' not found in smoothieConfig.", this);
                return;
            }

            Vector3 toPosition = initialPosition + GetOffset(animationType, config.offset);

            if (movableRect != null)
            {
                transformInterpolator.ApplyTransform(movableRect.localPosition, toPosition, config.positionInterpolatorConfig, movableRect, true, UpdatePosition);
            }

            alphaMultiplier = 1f;
            UpdateColorsImmediate();
            StartCoroutine(FadeAlpha(0f, config.fadeOut));
        }
    }

    private void HandleEventsAnimation(string animationType, bool resetInterpolator)
    {
        if (!smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.EventsConfig config))
        {
            Debug.LogWarning($"Animation type '{animationType}' not found in smoothieConfig.", this);
            return;
        }

        transformInterpolator.ApplyTransform(config, movableRect, rotatableRect, initialPosition, initialRotation, initialScale,
            config.ChangePos ? UpdatePosition : null,
            config.ChangeRotation ? UpdateRotation : null,
            config.ChangeScale ? UpdateScale : null,
            resetInterpolator
        );

        if (config.ChangeColors)
        {
            currentColorTransitionDuration = config.colorTransitionDuration;
            UpdateGraphicGroupColors(config);
        }
    }

    private void HandleMoveAnimation(string animationType, bool resetInterpolator)
    {
        if (!smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.MoveConfig config))
        {
            Debug.LogWarning($"Animation type '{animationType}' not found in smoothieConfig.", this);
            return;
        }

        Vector3 offset = GetOffset(animationType, config.offset);

        if (additionalOffsetInterpolator == null || resetInterpolator)
        {
            additionalOffsetInterpolator = new SmoothVector3(
                this,
                additionalOffset,
                (Vector3Interpolator.Config.InterpolationType)config.interpolatorConfig.interpolationType,
                config.interpolatorConfig.interpolationSpeed,
                config.interpolatorConfig.interpolationElasticity,
                OnAdditionalOffsetUpdated);
        }
        else
        {
            additionalOffsetInterpolator.UpdateConfig(
                (Vector3Interpolator.Config.InterpolationType)config.interpolatorConfig.interpolationType,
                config.interpolatorConfig.interpolationSpeed,
                config.interpolatorConfig.interpolationElasticity);
        }

        additionalOffsetInterpolator.SetValue(offset);

        if (moveReturnCoroutine != null)
        {
            StopCoroutine(moveReturnCoroutine);
        }
        moveReturnCoroutine = StartCoroutine(ReturnAdditionalOffset(config));
    }

    private void HandleFocusAnimation(string animationType, bool resetInterpolator)
    {
        if (focusAnimationCoroutine != null)
        {
            StopCoroutine(focusAnimationCoroutine);
            focusAnimationCoroutine = null;
        }

        var focusConfig = smoothieConfig.GetFocusConfig();

        if (selectRect != null)
        {
            Vector3 targetScale = animationType == "Focus" ? Vector3.one : focusDeselectedScale;
            float targetAlpha = animationType == "Focus" ? 1f : 0f;

            if (focusScaleInterpolator == null || resetInterpolator)
            {
                focusScaleInterpolator = new SmoothVector3(
                    this,
                    selectRect.localScale,
                    (Vector3Interpolator.Config.InterpolationType)focusConfig.interpolatorConfig.interpolationType,
                    focusConfig.interpolatorConfig.interpolationSpeed,
                    focusConfig.interpolatorConfig.interpolationElasticity,
                    UpdateFocusScale);
            }
            else
            {
                focusScaleInterpolator.UpdateConfig(
                    (Vector3Interpolator.Config.InterpolationType)focusConfig.interpolatorConfig.interpolationType,
                    focusConfig.interpolatorConfig.interpolationSpeed,
                    focusConfig.interpolatorConfig.interpolationElasticity);
            }
            focusScaleInterpolator.SetValue(targetScale);

            if (focusAlphaCoroutine != null)
            {
                StopCoroutine(focusAlphaCoroutine);
                focusAlphaCoroutine = null;
            }

            focusAnimationCoroutine = StartCoroutine(AnimateFocusAlpha(focusAlphaMultiplier, targetAlpha, focusConfig.fadeDuration));
        }
    }

    private IEnumerator AnimationCoroutine(string animationType, bool isShowAction)
    {
        float duration = 0f;

        if (isShowAction)
        {
            if (smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.ShowConfig config))
            {
                duration = config.fadeIn;
            }
        }
        else
        {
            if (smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.HideConfig config))
            {
                duration = config.fadeOut;
            }
        }

        yield return new WaitForSeconds(duration);

        isShowing = false;
        isHiding = false;
        isShown = isShowAction;
        isHidden = !isShowAction;

        SetInteractable(isShown);

        animationCoroutine = null;
    }

    private void SetInteractable(bool interactable)
    {
        var selectables = GetComponentsInChildren<Selectable>(true);
        foreach (var selectable in selectables)
        {
            selectable.interactable = interactable;
        }
    }

    private Vector3 GetOffset(string animationType, float offsetValue)
    {
        return animationType switch
        {
            "ShowFromLeft" or "HideToLeft" or "MoveLeft" => new Vector3(-offsetValue, 0, 0),
            "ShowFromRight" or "HideToRight" or "MoveRight" => new Vector3(offsetValue, 0, 0),
            "ShowFromTop" or "HideToTop" or "MoveUp" => new Vector3(0, offsetValue, 0),
            "ShowFromBottom" or "HideToBottom" or "MoveDown" => new Vector3(0, -offsetValue, 0),
            _ => Vector3.zero,
        };
    }

    private void UpdateGraphicGroupColors(SmoothieConfig.EventsConfig config)
    {
        if (config.ChangeBackgroundColor)
        {
            background.selectedColorType = config.ChangeBackgroundColorTo;
            if (!config.ChangeAlpha) UpdateGraphicGroupColor(background);
        }

        if (config.ChangeTextColor)
        {
            text.selectedColorType = config.ChangeTextColorTo;
            if (!config.ChangeAlpha) UpdateGraphicGroupColor(text);
        }

        if (config.ChangeSelectColor)
        {
            select.selectedColorType = config.ChangeSelectColorTo;
            if (!config.ChangeAlpha) UpdateGraphicGroupColor(select);
        }

        if (config.ChangeShadowColor)
        {
            shadow.selectedColorType = config.ChangeShadowColorTo;
            if (!config.ChangeAlpha) UpdateGraphicGroupColor(shadow);
        }

        if (config.ChangeAlpha)
        {
            alphaMultiplier = config.alphaMultiplier;
            UpdateColors();
        }
    }

    private void OnAdditionalOffsetUpdated(Vector3 newOffset)
    {
        additionalOffset = newOffset;
        UpdatePosition(currentBasePosition);
    }

    private IEnumerator ReturnAdditionalOffset(SmoothieConfig.MoveConfig config)
    {
        yield return new WaitForSeconds(config.returnDelay);

        if (additionalOffsetInterpolator != null)
        {
            additionalOffsetInterpolator.SetValue(Vector3.zero);
        }

        moveReturnCoroutine = null;
    }

    private void UpdatePosition(Vector3 newPosition)
    {
        currentBasePosition = newPosition;

        if (movableRect != null)
            movableRect.localPosition = currentBasePosition + additionalOffset;
    }

    private void UpdateRotation(Vector3 newRotation)
    {
        if (rotatableRect != null)
            rotatableRect.localRotation = Quaternion.Euler(newRotation);
    }

    private void UpdateScale(Vector3 newScale)
    {
        if (rotatableRect != null)
            rotatableRect.localScale = newScale;
    }

    private void UpdateFocusScale(Vector3 newScale)
    {
        if (selectRect != null)
        {
            selectRect.localScale = newScale;
        }
    }

    private IEnumerator AnimateFocusAlpha(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            focusAlphaMultiplier = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            UpdateColors();
            yield return null;
        }

        focusAlphaMultiplier = toAlpha;
        UpdateColors();
        focusAnimationCoroutine = null;
    }

    public void UpdateColors()
    {
        if (this == null || gameObject == null) return;

        UpdateGraphicGroupColor(background);
        UpdateGraphicGroupColor(text);
        UpdateGraphicGroupColor(select);
        UpdateGraphicGroupColor(shadow);
    }

    private void UpdateGraphicGroupColor(GraphicGroup group)
    {
        if (ActualTheme == null || group.graphics == null || group.graphics.Length == 0) return;

        Color targetColor = ActualTheme.GetColorByType(group.selectedColorType);
        targetColor.a *= alphaMultiplier;

        if (group == select)
        {
            targetColor.a *= focusAlphaMultiplier;
        }

        if (Application.isPlaying)
        {
            if (group.transitionCoroutine != null)
            {
                StopCoroutine(group.transitionCoroutine);
            }

            group.transitionCoroutine = StartCoroutine(TransitionColorGroup(group, targetColor, currentColorTransitionDuration));
        }
        else
        {
            foreach (var graphic in group.graphics)
            {
                if (graphic != null)
                {
                    graphic.color = targetColor;
#if UNITY_EDITOR
                    graphic.SetAllDirty();
                    EditorUtility.SetDirty(graphic);
#endif
                }
            }
        }
    }

    private IEnumerator TransitionColorGroup(GraphicGroup group, Color targetColor, float duration)
    {
        float elapsed = 0f;
        Color[] initialColors = new Color[group.graphics.Length];

        for (int i = 0; i < group.graphics.Length; i++)
        {
            if (group.graphics[i] != null)
                initialColors[i] = group.graphics[i].color;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            for (int i = 0; i < group.graphics.Length; i++)
            {
                if (group.graphics[i] != null)
                    group.graphics[i].color = Color.Lerp(initialColors[i], targetColor, t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < group.graphics.Length; i++)
        {
            if (group.graphics[i] != null)
                group.graphics[i].color = targetColor;
        }
    }

    public void SnapToStart(string animationType)
    {
        if (transformInterpolator == null)
        {
            Initialize();
        }

        if (smoothieConfig == null)
        {
            Debug.LogError("smoothieConfig is not assigned in SmoothieUIElement.", this);
            return;
        }

        if (animationType.StartsWith("Show"))
        {
            if (!smoothieConfig.TryGetConfig(animationType, out SmoothieConfig.ShowConfig config))
            {
                Debug.LogWarning($"Animation type '{animationType}' not found in smoothieConfig.", this);
                return;
            }

            Vector3 fromPosition = initialPosition + GetOffset(animationType, config.offset);
            if (movableRect != null)
            {
                movableRect.localPosition = fromPosition;
                currentBasePosition = initialPosition;
            }

            alphaMultiplier = 0f;
            UpdateColorsImmediate();
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        float startAlpha = alphaMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            alphaMultiplier = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            UpdateColors();
            yield return null;
        }

        alphaMultiplier = targetAlpha;
        UpdateColors();
    }

    public void SetAlpha(float alpha)
    {
        alphaMultiplier = alpha;
        UpdateColors();
    }

    public void SetAlphaImmediate(float alpha)
    {
        alphaMultiplier = alpha;
        UpdateColorsImmediate();
    }

    private void UpdateColorsImmediate()
    {
        if (this == null || gameObject == null) return;

        UpdateGraphicGroupColorImmediate(background);
        UpdateGraphicGroupColorImmediate(text);
        UpdateGraphicGroupColorImmediate(select);
        UpdateGraphicGroupColorImmediate(shadow);
    }

    private void UpdateGraphicGroupColorImmediate(GraphicGroup group)
    {
        if (ActualTheme == null || group.graphics == null || group.graphics.Length == 0) return;

        Color targetColor = ActualTheme.GetColorByType(group.selectedColorType);
        targetColor.a *= alphaMultiplier;

        if (group == select)
        {
            targetColor.a *= focusAlphaMultiplier;
        }

        foreach (var graphic in group.graphics)
        {
            if (graphic != null)
            {
                graphic.color = targetColor;
#if UNITY_EDITOR
                graphic.SetAllDirty();
                EditorUtility.SetDirty(graphic);
#endif
            }
        }
    }

    public void SetHiddenImmediate()
    {
        StopAllCoroutinesSafe();
        ResetTransforms();
        SetAlphaImmediate(0f);
        isShown = false;
        isHidden = true;
        isShowing = false;
        isHiding = false;
        SetInteractable(false);
    }

    public void SetShownImmediate()
    {
        StopAllCoroutinesSafe();
        ResetTransforms();
        SetAlphaImmediate(1f);
        isShown = true;
        isHidden = false;
        isShowing = false;
        isHiding = false;
        SetInteractable(true);
    }

    private void ResetTransforms()
    {
        if (movableRect != null)
        {
            movableRect.localPosition = initialPosition;
            currentBasePosition = initialPosition;
        }
        if (rotatableRect != null)
        {
            rotatableRect.localRotation = Quaternion.Euler(initialRotation);
            rotatableRect.localScale = initialScale;
        }
        if (selectRect != null)
        {
            selectRect.localScale = focusDeselectedScale;
        }
    }

    private void StopAllCoroutinesSafe()
    {
        StopAllCoroutines();
        transformInterpolator?.Stop();
        additionalOffsetInterpolator?.Stop();
        focusScaleInterpolator?.Stop();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (focusAnimationCoroutine != null)
        {
            StopCoroutine(focusAnimationCoroutine);
            focusAnimationCoroutine = null;
        }

        if (focusAlphaCoroutine != null)
        {
            StopCoroutine(focusAlphaCoroutine);
            focusAlphaCoroutine = null;
        }

        if (moveReturnCoroutine != null)
        {
            StopCoroutine(moveReturnCoroutine);
            moveReturnCoroutine = null;
        }
    }
}
