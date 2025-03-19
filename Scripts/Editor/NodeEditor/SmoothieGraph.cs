// SmoothieGraph.cs
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [CreateAssetMenu(fileName = "New Smoothie Graph", menuName = "Smoothie/Graph")]
    public class SmoothieGraph : ScriptableObject
    {
        public List<SmoothieContainer> containers = new List<SmoothieContainer>();
        public Vector2 viewPosition = Vector2.zero;
        public float zoomScale = 1f;
    }
}