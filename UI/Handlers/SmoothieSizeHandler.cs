using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SmoothieSizeHandler
{
    public static void ApplySize(RectTransform targetRect, TextMeshProUGUI textMesh, Vector4 padding)
    {
        if (textMesh == null || targetRect == null) return;
        Vector2 anchoredPosition = new Vector2((-padding.y + padding.w) / 2, (-padding.x + padding.z) / 2);
        textMesh.rectTransform.anchoredPosition = anchoredPosition;
        textMesh.ForceMeshUpdate();
        Vector2 size = textMesh.GetRenderedValues(false);
        if (size == Vector2.zero) return;
        targetRect.sizeDelta = new Vector2(size.x + padding.w + padding.y, size.y + padding.z + padding.x);
    }
}