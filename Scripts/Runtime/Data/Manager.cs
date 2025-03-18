using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [CreateAssetMenu(fileName = "SmoothieManager", menuName = "Smoothie/Manager")]
    public class Manager : ScriptableObject
    {
        [Header("Containers")]
        public List<Container> containers = new List<Container>();

        [Header("Editor Settings")]
        // Сохраняем положение холста и масштаб, чтобы при повторном открытии окна
        // редактор восстанавливал тот же "ракурс".
        public Vector2 editorPanOffset = Vector2.zero;
        public float editorZoomScale = 1f;
    }
}