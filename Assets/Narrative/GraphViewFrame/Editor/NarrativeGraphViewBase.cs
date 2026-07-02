#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 叙事 Graph 画布基类 网格与中键平移
    /// </summary>
    public abstract class NarrativeGraphViewBase : GraphView
    {
        const float MinorGridSpacing = 20f;
        const float MajorGridSpacing = 100f;
        const int MiddleMouseButton = 2;

        protected IMGUIContainer GridBackground { get; private set; }

        Vector2 lastPanMousePosition;
        bool isMiddleMousePanning;

        protected NarrativeGraphViewBase(string gridElementName = "narrative-graph-grid")
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground = new IMGUIContainer(DrawGridBackground) { name = gridElementName };
            GridBackground.pickingMode = PickingMode.Ignore;
            Insert(0, GridBackground);
            GridBackground.StretchToParentSize();

            style.flexGrow = 1;
            pickingMode = PickingMode.Position;
            focusable = true;

            RegisterCallback<MouseDownEvent>(_ => Focus());
            RegisterCallback<MouseDownEvent>(OnMiddleMouseDown, TrickleDown.TrickleDown);
            RegisterCallback<MouseMoveEvent>(OnMiddleMouseMove, TrickleDown.TrickleDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            RegisterCallback<WheelEvent>(_ => GridBackground.MarkDirtyRepaint());
        }

        /// <summary>
        /// 左键抬起时回调 用于同步选中
        /// </summary>
        protected virtual void OnGraphLeftMouseUp(MouseUpEvent evt) { }

        /// <summary>
        /// 左键抬起前是否跳过选中同步
        /// </summary>
        protected virtual bool ShouldIgnoreLeftMouseUp(MouseUpEvent evt) => false;

        /// <summary>
        /// 标记网格重绘
        /// </summary>
        protected void MarkGridDirty() => GridBackground.MarkDirtyRepaint();

        /// <summary>
        /// 释放画布鼠标捕获
        /// </summary>
        public void ReleaseInteractionCapture()
        {
            if (!isMiddleMousePanning)
                return;

            isMiddleMousePanning = false;
            if (this.HasMouseCapture())
                this.ReleaseMouse();
        }

        void DrawGridBackground()
        {
            var rect = new Rect(0f, 0f, resolvedStyle.width, resolvedStyle.height);
            if (rect.width <= 0f || rect.height <= 0f)
                rect = new Rect(0f, 0f, layout.width, layout.height);

            EditorGUI.DrawRect(rect, GraphViewFramePanelStyles.GraphPanelBackground);
            GraphGridDrawer.DrawGridLines(rect, viewTransform.position, viewTransform.scale.x, MinorGridSpacing, new Color(0.23f, 0.23f, 0.23f, 0.7f), 1f);
            GraphGridDrawer.DrawGridLines(rect, viewTransform.position, viewTransform.scale.x, MajorGridSpacing, new Color(0.31f, 0.31f, 0.31f, 0.9f), 1f);
        }

        void OnMiddleMouseDown(MouseDownEvent evt)
        {
            if (evt.button != MiddleMouseButton)
                return;

            isMiddleMousePanning = true;
            lastPanMousePosition = evt.mousePosition;
            this.CaptureMouse();
            Focus();
            evt.StopImmediatePropagation();
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                if (!ShouldIgnoreLeftMouseUp(evt))
                    OnGraphLeftMouseUp(evt);
                return;
            }

            if (!isMiddleMousePanning || evt.button != MiddleMouseButton)
                return;

            isMiddleMousePanning = false;
            this.ReleaseMouse();
            evt.StopImmediatePropagation();
        }

        void OnMiddleMouseMove(MouseMoveEvent evt)
        {
            if (!isMiddleMousePanning || !this.HasMouseCapture())
                return;

            Vector2 delta = evt.mousePosition - lastPanMousePosition;
            lastPanMousePosition = evt.mousePosition;
            UpdateViewTransform(viewTransform.position + new Vector3(delta.x, delta.y, 0f), viewTransform.scale);
            GridBackground.MarkDirtyRepaint();
            evt.StopImmediatePropagation();
        }
    }
}
#endif
