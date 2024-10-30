using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Smoothie;

[RequireComponent(typeof(SmoothieUIElement))]
public class SmoothieUIButton : Button
{
    private SmoothieUIElement smoothieUIElement;

    [SerializeField, Range(-1, 10)]
    public int SelectAfterShown = -1;

    protected override void Awake()
    {
        base.Awake();
        smoothieUIElement = GetComponent<SmoothieUIElement>();
        if (smoothieUIElement == null)
        {
            Debug.LogError("SmoothieUIElement is not attached to the same GameObject as SmoothieUIButton.", this);
        }
    }

    public override bool IsInteractable()
    {
        return base.IsInteractable() && smoothieUIElement != null && smoothieUIElement.isShown;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnPointerEnter(eventData);
        smoothieUIElement?.Animate("Over", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnPointerExit(eventData);
        smoothieUIElement?.Animate("Normal", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnPointerDown(eventData);
        smoothieUIElement?.Animate("Press", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnPointerUp(eventData);
        smoothieUIElement?.Animate("Over", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnSelect(eventData);
        smoothieUIElement?.Animate("Focus", isShowAction: false, resetInterpolator: false, isStateChange: true);
        smoothieUIElement?.Animate("Over", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnDeselect(eventData);
        smoothieUIElement?.Animate("Unfocus", isShowAction: false, resetInterpolator: false, isStateChange: true);
        smoothieUIElement?.Animate("Normal", isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (!IsInteractable()) return;
        base.DoStateTransition(state, instant);
        if (smoothieUIElement == null) return;

        string animationType = state switch
        {
            SelectionState.Normal => "Normal",
            SelectionState.Highlighted => "Over",
            SelectionState.Pressed => "Press",
            SelectionState.Selected => "Over",
            SelectionState.Disabled => "Normal",
            _ => "Normal",
        };

        smoothieUIElement.Animate(animationType, isShowAction: false, resetInterpolator: false, isStateChange: true);
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (!IsInteractable()) return;

        string moveAnimation = eventData.moveDir switch
        {
            MoveDirection.Left => "MoveLeft",
            MoveDirection.Right => "MoveRight",
            MoveDirection.Up => "MoveUp",
            MoveDirection.Down => "MoveDown",
            _ => "",
        };

        if (!string.IsNullOrEmpty(moveAnimation))
        {
            smoothieUIElement?.Animate(moveAnimation, isShowAction: false, resetInterpolator: true, isStateChange: true);
        }

        base.OnMove(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        base.OnPointerClick(eventData);
    }
}
