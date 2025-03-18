using UnityEngine;

namespace Smoothie
{
    [System.Serializable]
    public class Container : ScriptableObject
    {
        [Header("Base Settings")]
        public string containerName = "New Container";
        public bool toggle1 = false;
        public bool toggle2 = false;
        public float someFloat = 0f;

        [Header("Editor Position")]
        // Позиция ноды на холсте (сохраняется при перетаскивании)
        public Vector2 nodePosition = Vector2.zero;
    }
}