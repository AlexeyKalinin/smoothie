using System.Collections.Generic;

namespace Smoothie
{
    /// <summary>
    /// Defines all standard animation events as constants to ensure consistency
    /// </summary>
    public static class SmoothieAnimationEvents
    {
        // Basic state
        public const string Normal = "Normal";
        
        // Pointer events
        public const string PointerEnter = "PointerEnter";
        public const string PointerExit = "PointerExit";
        public const string PointerDown = "PointerDown";
        public const string PointerUp = "PointerUp";
        public const string PointerClick = "PointerClick";
        
        // Drag events
        public const string BeginDrag = "BeginDrag";
        public const string Drag = "Drag";
        public const string EndDrag = "EndDrag";
        public const string Drop = "Drop";
        public const string InitializePotentialDrag = "InitializePotentialDrag";
        
        // Selection events
        public const string Select = "Select";
        public const string Deselect = "Deselect";
        public const string UpdateSelected = "UpdateSelected";
        
        // Other events
        public const string Submit = "Submit";
        public const string Cancel = "Cancel";
        public const string Scroll = "Scroll";
        public const string Move = "Move";
        
        // Button-specific events
        public const string Press = "Press";
        public const string Release = "Release";
        
        // Custom events
        public const string Show = "Show";
        public const string Hide = "Hide";
        public const string Highlight = "Highlight";
        public const string Disable = "Disable";
        public const string Enable = "Enable";
        
        /// <summary>
        /// Returns a list of all standard events for easy initialization
        /// </summary>
        public static List<string> GetAllEvents()
        {
            return new List<string>
            {
                Normal, 
                PointerEnter, PointerExit, PointerDown, PointerUp, PointerClick,
                BeginDrag, Drag, EndDrag, Drop, InitializePotentialDrag,
                Select, Deselect, UpdateSelected,
                Submit, Cancel, Scroll, Move,
                Press, Release,
                Show, Hide, Highlight, Disable, Enable
            };
        }
    }

    /// <summary>
    /// Defines all standard UI elements as constants to ensure consistency
    /// </summary>
    public static class SmoothieUIElements
    {
        // Basic UI elements
        public const string Move = "Move";
        public const string Background = "Background";
        public const string Text = "Text";
        public const string Icon = "Icon";
        public const string Border = "Border";
        public const string Shadow = "Shadow";
        
        // Interactive elements
        public const string Button = "Button";
        public const string Toggle = "Toggle";
        public const string Handle = "Handle";
        public const string Indicator = "Indicator";
        public const string Fill = "Fill";
        public const string Checkmark = "Checkmark";
        
        // Container elements
        public const string Container = "Container";
        public const string Header = "Header";
        public const string Footer = "Footer";
        public const string Panel = "Panel";
        
        /// <summary>
        /// Returns a list of all standard UI elements for easy initialization
        /// </summary>
        public static List<string> GetAllElements()
        {
            return new List<string>
            {
                Move, Background, Text, Icon, Border, Shadow,
                Button, Toggle, Handle, Indicator, Fill, Checkmark,
                Container, Header, Footer, Panel
            };
        }
    }
}