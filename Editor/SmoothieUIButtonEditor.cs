#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SmoothieUIButton))]
public class SmoothieUIButtonEditor : ButtonEditor
{
    SerializedProperty selectAfterShownProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        selectAfterShownProp = serializedObject.FindProperty("SelectAfterShown");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(selectAfterShownProp);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif