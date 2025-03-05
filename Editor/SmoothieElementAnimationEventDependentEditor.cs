#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;

namespace Smoothie
{
    [Sirenix.OdinInspector.Editor.OdinDrawer]
    public class SmoothieElementAnimationEventDependentEditor : OdinValueDrawer<SmoothieElementAnimationEventDependent>
    {
        private static Dictionary<SmoothieElementAnimationEventDependent, bool> s_foldoutMap
            = new Dictionary<SmoothieElementAnimationEventDependent, bool>();

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var target = entry.SmartValue;
            if (target == null)
            {
                base.CallNextDrawer(label);
                return;
            }

            if (!s_foldoutMap.TryGetValue(target, out bool isExpanded))
            {
                isExpanded = true;
                s_foldoutMap[target] = isExpanded;
            }

            SirenixEditorGUI.BeginBox();

            // Заголовок (Foldout + Delete Event)
            SirenixEditorGUI.BeginBoxHeader();
            {
                EditorGUILayout.BeginHorizontal();

                // Foldout: показываем eventKeys или "No events"
                var eventKeys = target.eventKeys;
                string foldoutLabel = (eventKeys != null && eventKeys.Count > 0)
                    ? string.Join(", ", eventKeys)
                    : "No events";

                isExpanded = SirenixEditorGUI.Foldout(isExpanded, foldoutLabel);
                s_foldoutMap[target] = isExpanded;

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Event", GUILayout.Width(100)))
                {
                    if (target.parentStyle != null)
                    {
                        target.parentStyle.eventDefinitions.Remove(target);
                        return; // объект удалён
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            SirenixEditorGUI.EndBoxHeader();

            if (isExpanded)
            {
                EditorGUILayout.Space(5);

                // 1) Отрисовываем eventKeys (пусть Odin нарисует всё сам)
                var eventKeysProperty = Property.Children.FirstOrDefault(c => c.Name == "eventKeys");
                if (eventKeysProperty != null)
                {
                    eventKeysProperty.Draw(eventKeysProperty.Label);
                }

                EditorGUILayout.Space(10);

                // 2) Рисуем uiElements вручную, без лишнего заголовка/рамки
                var uiElementsProperty = Property.Children.FirstOrDefault(c => c.Name == "uiElements");
                if (uiElementsProperty != null)
                {
                    var children = uiElementsProperty.Children;
                    int i = 0;

                    while (i < children.Count)
                    {
                        var uiElementChild = children[i];
                        var uiElementKeyProp = uiElementChild.Children.FirstOrDefault(c => c.Name == "uiElementKey");

                        // Одна горизонтальная строка: [dropdown (покрашенный, расширенный)] [Delete Element]
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                        // Покрасим dropdown в пастельный цвет (например, светло-зелёный)
                        Color oldBG = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0.75f, 1f, 0.75f); 
                        if (uiElementKeyProp != null)
                        {
                            // Вложим dropdown в вертикальный Layout, чтобы он занимал оставшееся место
                            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                            uiElementKeyProp.Draw(null);
                            EditorGUILayout.EndVertical();
                        }
                        GUI.backgroundColor = oldBG;

                        // Кнопка "Delete Element"
                        if (GUILayout.Button("Delete Element", GUILayout.Width(100)))
                        {
                            target.uiElements.RemoveAt(i);
                            uiElementsProperty.RecordForUndo("Remove UI Element");
                            EditorGUILayout.EndHorizontal();
                            continue;
                        }

                        EditorGUILayout.EndHorizontal();

                        // Теперь рисуем остальные поля (например, actions) ниже
                        foreach (var grandChild in uiElementChild.Children)
                        {
                            if (grandChild.Name == "uiElementKey") 
                                continue; 
                            grandChild.Draw(grandChild.Label);
                        }

                        EditorGUILayout.Space(5);
                        i++;
                    }

                    // === Кнопка "Add UI Element" внизу, на всю ширину ===
                    EditorGUILayout.Space(5);
                    if (GUILayout.Button("Add UI Element", GUILayout.ExpandWidth(true), GUILayout.Height(25)))
                    {
                        target.uiElements.Add(new SmoothieElementAnimationUIElement());
                        uiElementsProperty.RecordForUndo("Add UI Element");
                    }
                }

                EditorGUILayout.Space(5);
            }

            SirenixEditorGUI.EndBox();
        }
    }
}
#endif
