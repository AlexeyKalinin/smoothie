// SmoothieContainer.cs - Updated to remove ports
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Smoothie
{
    [Serializable]
    public class SmoothieContainer : ScriptableObject
    {
        public string title = "Container";
        public Color headerColor = new Color(0.5f, 0.3f, 0.8f); // Default purple color
        public Vector2 position;
        
        // Connection data
        [Serializable]
        public class Connection
        {
            public string outputPort;
            public SmoothieContainer targetContainer;
            public string inputPort;
        }
        
        public List<Connection> connections = new List<Connection>();
    }
}