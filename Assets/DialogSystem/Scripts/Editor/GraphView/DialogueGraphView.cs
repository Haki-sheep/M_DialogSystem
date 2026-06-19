#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图 GraphView 画布
    /// </summary>
    public partial class DialogueGraphView : GraphView
    {
        internal const float DefaultNodeWidth = 220f;
        internal const float DefaultNodeHeight = 120f;
        const float MinorGridSpacing = 20f;
        const float MajorGridSpacing = 100f;
        const int MiddleMouseButton = 2;

        readonly DialogueGraphEditorWindow ownerWindow;
        readonly Dictionary<DialogueNode, DialogueNodeView> nodeViews = new Dictionary<DialogueNode, DialogueNodeView>();
        readonly IMGUIContainer gridBackground;

        DialogueGraph currentGraph;
        Vector2 lastPanMousePosition;
        bool isPopulating;
        bool isMiddleMousePanning;
        bool suppressSelectionBroadcast;
        bool isRebindingEdges;

        public DialogueGraph CurrentGraph => currentGraph;

        public DialogueGraphView(DialogueGraphEditorWindow ownerWindow)
        {
            this.ownerWindow = ownerWindow;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new IMGUIContainer(DrawGridBackground) { name = "dialog-graph-grid" };
            gridBackground.pickingMode = PickingMode.Ignore;
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            graphViewChanged = OnGraphViewChanged;

            style.flexGrow = 1;
            pickingMode = PickingMode.Position;
            focusable = true;
            RegisterCallback<MouseDownEvent>(_ => Focus());
            RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            RegisterCallback<WheelEvent>(_ => gridBackground.MarkDirtyRepaint());
        }

        void DrawGridBackground()
        {
            var rect = new Rect(0f, 0f, resolvedStyle.width, resolvedStyle.height);
            if (rect.width <= 0f || rect.height <= 0f)
                rect = new Rect(0f, 0f, layout.width, layout.height);

            EditorGUI.DrawRect(rect, new Color(0.14f, 0.14f, 0.14f));
            DrawGridLines(rect, MinorGridSpacing, new Color(0.23f, 0.23f, 0.23f, 0.7f), 1f);
            DrawGridLines(rect, MajorGridSpacing, new Color(0.31f, 0.31f, 0.31f, 0.9f), 1f);
        }

        void DrawGridLines(Rect rect, float baseSpacing, Color color, float lineWidth)
        {
            float scale = Mathf.Max(0.05f, viewTransform.scale.x);
            float spacing = baseSpacing * scale;
            if (spacing < 4f)
                return;

            Vector3 offset = viewTransform.position;
            float xStart = offset.x % spacing;
            float yStart = offset.y % spacing;

            if (xStart > 0f)
                xStart -= spacing;
            if (yStart > 0f)
                yStart -= spacing;

            for (float x = xStart; x < rect.width; x += spacing)
                EditorGUI.DrawRect(new Rect(x, 0f, lineWidth, rect.height), color);

            for (float y = yStart; y < rect.height; y += spacing)
                EditorGUI.DrawRect(new Rect(0f, y, rect.width, lineWidth), color);
        }

        void SyncGraphSelectionToTree()
        {
            if (suppressSelectionBroadcast || isPopulating)
                return;

            foreach (var item in selection)
            {
                if (item is Edge edge)
                {
                    var handle = BuildTransitionHandle(edge);
                    if (handle != null)
                    {
                        ownerWindow.SelectTransition(handle);
                        return;
                    }
                }
            }

            foreach (var item in selection)
            {
                if (item is DialogueNodeView nodeView)
                {
                    ownerWindow.SelectObjectInTree(nodeView.Node);
                    return;
                }
            }

            if (!selection.Any() && currentGraph != null)
                ownerWindow.SelectObjectInTree(currentGraph);
        }

        internal DialogueTransitionHandle BuildTransitionHandle(Edge edge)
        {
            var sourceView = edge.output?.node as DialogueNodeView;
            var targetView = edge.input?.node as DialogueNodeView;
            if (sourceView == null)
                return null;

            var handle = new DialogueTransitionHandle
            {
                graph = currentGraph,
                sourceNode = sourceView.Node,
                targetNode = targetView?.Node,
            };

            if (edge.userData is DialogueOptionTransition optionTransition)
                handle.optionTransition = optionTransition;
            else if (!sourceView.Node.IsOptionNode)
                handle.targetNode = targetView?.Node;
            else
                return null;

            return handle;
        }

        static Port FindPort(VisualElement element)
        {
            while (element != null)
            {
                if (element is Port port)
                    return port;
                element = element.parent;
            }

            return null;
        }

        void OnMouseDown(MouseDownEvent evt)
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
                if (FindPort(evt.target as VisualElement) != null)
                    return;

                schedule.Execute(SyncGraphSelectionToTree);
                return;
            }

            if (!isMiddleMousePanning || evt.button != MiddleMouseButton)
                return;

            isMiddleMousePanning = false;
            this.ReleaseMouse();
            evt.StopImmediatePropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isMiddleMousePanning || !this.HasMouseCapture())
                return;

            Vector2 delta = evt.mousePosition - lastPanMousePosition;
            lastPanMousePosition = evt.mousePosition;
            UpdateViewTransform(viewTransform.position + new Vector3(delta.x, delta.y, 0f), viewTransform.scale);
            gridBackground.MarkDirtyRepaint();
            evt.StopImmediatePropagation();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (currentGraph != null)
            {
                evt.menu.AppendAction("创建对话节点", _ => CreateNodeAtViewCenter());
                evt.menu.AppendSeparator();
            }

            base.BuildContextualMenu(evt);
        }

        public void RefreshNodeTitles()
        {
            foreach (var view in nodeViews.Values)
                view?.RefreshTitle();
        }

        void RunWithoutSelectionBroadcast(System.Action action)
        {
            suppressSelectionBroadcast = true;
            try
            {
                action();
            }
            finally
            {
                schedule.Execute(() => suppressSelectionBroadcast = false);
            }
        }

        /// <summary>
        /// 释放画布鼠标捕获 避免编辑器全局无响应
        /// </summary>
        internal void ReleaseInteractionCapture()
        {
            if (!isMiddleMousePanning)
                return;

            isMiddleMousePanning = false;
            if (this.HasMouseCapture())
                this.ReleaseMouse();
        }
    }
}
#endif
