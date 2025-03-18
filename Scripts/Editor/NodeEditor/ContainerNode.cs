using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Smoothie
{
    /// <summary>
    /// Визуальный элемент, представляющий ноду (контейнер).
    /// </summary>
    public class ContainerNode : VisualElement
    {
        private Container container;
        private SmoothieManagerWindow6 window;

        private bool isDragging = false;
        private Vector2 dragStartPos;

        public ContainerNode(Container container, SmoothieManagerWindow6 window)
        {
            this.container = container;
            this.window = window;

            // Стили ноды
            style.position = Position.Absolute;
            style.width = 200;
            style.height = 150;
            style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.borderLeftColor = Color.white;
            style.borderRightColor = Color.white;
            style.borderTopColor = Color.white;
            style.borderBottomColor = Color.white;

            // Восстанавливаем позицию из контейнера
            style.left = container.nodePosition.x;
            style.top = container.nodePosition.y;

            // Поле имени
            var nameField = new TextField("Name:");
            nameField.value = container.containerName;
            nameField.RegisterValueChangedCallback(evt =>
            {
                container.containerName = evt.newValue;
                EditorUtility.SetDirty(container);
            });
            Add(nameField);

            // Пара тумблеров
            var toggle1 = new Toggle("Toggle 1");
            toggle1.value = container.toggle1;
            toggle1.RegisterValueChangedCallback(evt =>
            {
                container.toggle1 = evt.newValue;
                EditorUtility.SetDirty(container);
            });
            Add(toggle1);

            var toggle2 = new Toggle("Toggle 2");
            toggle2.value = container.toggle2;
            toggle2.RegisterValueChangedCallback(evt =>
            {
                container.toggle2 = evt.newValue;
                EditorUtility.SetDirty(container);
            });
            Add(toggle2);

            // Поле float
            var floatField = new FloatField("Some Float");
            floatField.value = container.someFloat;
            floatField.RegisterValueChangedCallback(evt =>
            {
                container.someFloat = evt.newValue;
                EditorUtility.SetDirty(container);
            });
            Add(floatField);

            // Кнопки Duplicate / Delete
            var duplicateBtn = new Button(() => window.DuplicateContainer(container)) { text = "Duplicate" };
            Add(duplicateBtn);

            var deleteBtn = new Button(() => window.DeleteContainer(container, this)) { text = "Delete" };
            Add(deleteBtn);

            // События для перетаскивания
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                isDragging = true;
                dragStartPos = evt.localMousePosition;
                evt.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDragging)
            {
                var delta = evt.localMousePosition - dragStartPos;
                style.left = style.left.value.value + delta.x;
                style.top = style.top.value.value + delta.y;

                // Сохраняем позицию в контейнер
                container.nodePosition = new Vector2(style.left.value.value, style.top.value.value);
                EditorUtility.SetDirty(container);

                evt.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (isDragging && evt.button == (int)MouseButton.LeftMouse)
            {
                isDragging = false;
                evt.StopPropagation();
            }
        }
    }
}
