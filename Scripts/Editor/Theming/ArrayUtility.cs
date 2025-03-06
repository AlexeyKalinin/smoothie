using UnityEditor;

namespace Smoothie
{
    /// <summary>
    /// Utility methods for working with arrays in the editor
    /// </summary>
    public static class ArrayUtility
    {
        /// <summary>
        /// Find a property in a serialized array by a predicate
        /// </summary>
        public static SerializedProperty Find(SerializedProperty arrayProperty, System.Func<SerializedProperty, bool> predicate)
        {
            if (!arrayProperty.isArray)
                return null;
                
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var elementProperty = arrayProperty.GetArrayElementAtIndex(i);
                if (predicate(elementProperty))
                {
                    return elementProperty;
                }
            }
            
            return null;
        }
    }
}