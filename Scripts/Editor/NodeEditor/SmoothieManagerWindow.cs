using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental; // для experimental.transform

namespace Smoothie
{
    /// <summary>
    /// Окно редактора с "бесконечным" холстом, панорамированием (средняя кнопка) и зумом (колесо + Ctrl/Command).
    /// Без использования Toolbar, CaptureMouse, ReleaseMouse и пр.
    /// </summary>
    public class SmoothieManagerWindow6 : EditorWindow
    {
        private Manager manager;

        // Контейнер, где лежат все ноды
        private VisualElement graphContainer;

        // Параметры панорамирования
        private bool isPanning = false;
        private Vector2 lastMousePos;
        private Vector2 panOffset = Vector2.zero;

        // Параметры зума
        private float zoomScale = 1f;
        private const float MIN_ZOOM = 0.1f;
        private const float MAX_ZOOM = 5f;
        private const float ZOOM_SPEED = 0.01f;

        [MenuItem("Window/Smoothie Manager Editor (Unity6)")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<SmoothieManagerWindow6>();
            wnd.titleContent = new GUIContent("Smoothie Manager Editor (Unity6)");
        }

        private void OnEnable()
        {
            // Ищем или создаём Manager
            string[] guids = AssetDatabase.FindAssets("t:Smoothie.Manager");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                manager = AssetDatabase.LoadAssetAtPath<Manager>(path);
            }
            else
            {
                manager = CreateInstance<Manager>();
                AssetDatabase.CreateAsset(manager, "Assets/SmoothieManager.asset");
                AssetDatabase.SaveAssets();
            }

            // Загружаем сохранённые настройки
            panOffset = manager.editorPanOffset;
            zoomScale = manager.editorZoomScale;

            // Настраиваем окно
            var root = rootVisualElement;
            root.Clear();

            // Верхняя панель (без Toolbar, просто VisualElement + Button)
            var topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.paddingLeft = 5;
            topBar.style.paddingTop = 5;

            var addButton = new Button(AddContainer) { text = "Add Container" };
            topBar.Add(addButton);

            root.Add(topBar);

            // "Бесконечный" холст
            var outerCanvas = new VisualElement();
            outerCanvas.style.flexGrow = 1;
            outerCanvas.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            // Отключаем обрезание
            outerCanvas.style.overflow = Overflow.Hidden;
            root.Add(outerCanvas);

            // Контейнер, который будем двигать/масштабировать
            graphContainer = new VisualElement();
            graphContainer.style.overflow = Overflow.Visible;
            outerCanvas.Add(graphContainer);

            // События
            outerCanvas.RegisterCallback<WheelEvent>(OnWheelEvent, TrickleDown.TrickleDown);
            outerCanvas.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            outerCanvas.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            outerCanvas.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);

            // Создаём ноды
            RefreshContainerNodes();

            // Применяем изначальный pan/zoom
            UpdateTransform();
        }

        /// <summary>
        /// Пересоздаём все ноды из manager.containers.
        /// </summary>
        private void RefreshContainerNodes()
        {
            graphContainer.Clear();

            foreach (var c in manager.containers)
            {
                var node = new ContainerNode(c, this);
                graphContainer.Add(node);
            }
        }

        /// <summary>
        /// Добавить новый контейнер (sub-asset).
        /// </summary>
        private void AddContainer()
        {
            var newContainer = CreateInstance<Container>();
            newContainer.containerName = "Container " + (manager.containers.Count + 1);

            AssetDatabase.AddObjectToAsset(newContainer, manager);
            manager.containers.Add(newContainer);
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();

            var node = new ContainerNode(newContainer, this);
            graphContainer.Add(node);
        }

        /// <summary>
        /// Удалить контейнер.
        /// </summary>
        public void DeleteContainer(Container container, ContainerNode node)
        {
            if (EditorUtility.DisplayDialog("Delete Container",
                $"Are you sure you want to delete {container.containerName}?", "Yes", "No"))
            {
                manager.containers.Remove(container);
                DestroyImmediate(container, true);
                node.RemoveFromHierarchy();

                EditorUtility.SetDirty(manager);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Дублировать контейнер.
        /// </summary>
        public void DuplicateContainer(Container container)
        {
            var copy = Instantiate(container);
            copy.containerName = container.containerName + " Copy";

            AssetDatabase.AddObjectToAsset(copy, manager);
            manager.containers.Add(copy);
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();

            var node = new ContainerNode(copy, this);
            graphContainer.Add(node);
        }

        /// <summary>
        /// Обработка колёсика мыши: зум при зажатом Ctrl/Command.
        /// </summary>
        private void OnWheelEvent(WheelEvent evt)
        {
            if (evt.ctrlKey || evt.commandKey)
            {
                float oldZoom = zoomScale;
                zoomScale = Mathf.Clamp(zoomScale - evt.delta.y * ZOOM_SPEED, MIN_ZOOM, MAX_ZOOM);

                // Чтобы зум происходил вокруг курсора
                Vector2 mousePos = evt.localMousePosition;
                Vector2 graphPosBeforeZoom = (mousePos - panOffset) / oldZoom;
                panOffset = mousePos - graphPosBeforeZoom * zoomScale;

                UpdateTransform();
                evt.StopPropagation();

                // Сохраняем новые значения
                manager.editorPanOffset = panOffset;
                manager.editorZoomScale = zoomScale;
                EditorUtility.SetDirty(manager);
            }
        }

        /// <summary>
        /// Зажатие средней кнопки для панорамирования.
        /// </summary>
        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.button == (int)MouseButton.MiddleMouse)
            {
                isPanning = true;
                lastMousePos = evt.mousePosition;
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Движение мыши: панорамирование, если зажата средняя кнопка.
        /// </summary>
        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            if (isPanning)
            {
                Vector2 delta = evt.mousePosition - lastMousePos;
                lastMousePos = evt.mousePosition;
                panOffset += delta;

                UpdateTransform();
                evt.StopPropagation();

                manager.editorPanOffset = panOffset;
                manager.editorZoomScale = zoomScale;
                EditorUtility.SetDirty(manager);
            }
        }

        /// <summary>
        /// Отпускание средней кнопки — перестаём панорамировать.
        /// </summary>
        private void OnMouseUpEvent(MouseUpEvent evt)
        {
            if (isPanning && evt.button == (int)MouseButton.MiddleMouse)
            {
                isPanning = false;
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// Применяем panOffset и zoomScale к graphContainer через experimental.transform.
        /// </summary>
        private void UpdateTransform()
        {
            // experimental.transform позволяет задать позицию и масштаб 2D/3D
            var t = graphContainer.transform;
            t.position = new Vector3(panOffset.x, panOffset.y, 0f);
            t.scale = new Vector3(zoomScale, zoomScale, 1f);
        }
    }
}
