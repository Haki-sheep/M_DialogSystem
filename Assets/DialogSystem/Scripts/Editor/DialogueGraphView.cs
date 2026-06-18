#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 单个对话节点的 GraphView 视图
    /// </summary>
    public class DialogueNodeView : Node
    {
        readonly DialogueGraph graph;
        readonly List<Port> outputPorts = new List<Port>();
        bool hasSavedLayout;
        Vector2 savedLayoutPosition;

        public DialogueNode Node { get; }

        public Port InputPort { get; private set; }

        public DialogueNodeView(DialogueNode node, DialogueGraph graph)
        {
            Node = node;
            this.graph = graph;

            title = BuildTitle();
            viewDataKey = node.GetInstanceID().ToString();
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            if (node.IsOptionNode)
                RebuildChoicePorts();
            else
                AddSingleOutputPort("Out");

            RefreshExpandedState();
            RefreshPorts();

        }

        string BuildTitle()
        {
            string text = Node.DialogText;
            if (!string.IsNullOrEmpty(text) && text.Length > 16)
                text = text.Substring(0, 16) + "…";
            return $"[{Node.NodeId}] {Node.SpeakerName}\n{text}";
        }

        public void RefreshTitle()
        {
            title = BuildTitle();
        }

        void AddSingleOutputPort(string portName)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = portName;
            outputContainer.Add(port);
            outputPorts.Add(port);
        }

        public void RebuildChoicePorts()
        {
            outputContainer.Clear();
            outputPorts.Clear();

            int count = Mathf.Max(1, Node.ChoiceList?.Count ?? 0);
            for (int i = 0; i < count; i++)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                string label = "新选项";
                if (Node.ChoiceList != null && i < Node.ChoiceList.Count && Node.ChoiceList[i] != null)
                    label = string.IsNullOrEmpty(Node.ChoiceList[i].labelText) ? $"选项{i + 1}" : Node.ChoiceList[i].labelText;
                port.portName = label;
                port.userData = i;
                outputContainer.Add(port);
                outputPorts.Add(port);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port OutputPort => outputPorts.Count > 0 ? outputPorts[0] : null;

        public Port GetOutputPort(int index)
        {
            if (index < 0 || index >= outputPorts.Count)
                return null;
            return outputPorts[index];
        }

        public int GetOutputPortIndex(Port port) => outputPorts.IndexOf(port);

        public void MarkAsStartNode()
        {
            titleContainer.style.borderTopWidth = 3;
            titleContainer.style.borderTopColor = new Color(0.2f, 0.85f, 0.35f);
        }

        public void SaveLayout()
        {
            if (graph == null || Node == null)
                return;

            Vector2 position = GetPosition().position;
            if (hasSavedLayout && Vector2.SqrMagnitude(position - savedLayoutPosition) < 0.01f)
                return;

            hasSavedLayout = true;
            savedLayoutPosition = position;
            DialogueGraphLayoutStore.SetPosition(graph, Node, position);
        }
    }

    /// <summary>
    /// 对话图 GraphView 画布
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        const float DefaultNodeWidth = 220f;
        const float DefaultNodeHeight = 120f;
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

        public DialogueGraph CurrentGraph => currentGraph;

        public DialogueGraphView(DialogueGraphEditorWindow ownerWindow)
        {
            this.ownerWindow = ownerWindow;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new IMGUIContainer(DrawGridBackground)
            {
                name = "dialog-graph-grid"
            };
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

        // 画布选中变化后同步左侧树
        void SyncGraphSelectionToTree()
        {
            if (suppressSelectionBroadcast || isPopulating)
                return;

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
                var mousePos = contentViewContainer.WorldToLocal(evt.originalMousePosition);
                evt.menu.AppendAction("创建对话节点", _ => CreateNodeAtMouse(mousePos));
                evt.menu.AppendSeparator();
            }

            base.BuildContextualMenu(evt);
        }

        void CreateNodeAtMouse(Vector2 localPosition)
        {
            if (currentGraph == null)
                return;

            var node = ownerWindow.CreateNodeAsset(currentGraph);
            if (node == null)
                return;

            currentGraph.AddNode(node);
            EditorUtility.SetDirty(currentGraph);

            var view = CreateNodeView(node, localPosition);
            view.SaveLayout();
            EditorApplication.delayCall += () =>
            {
                ownerWindow.ForceMenuTreeRebuild();
                ownerWindow.ResetSelectionSync();
            };
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (currentGraph == null || isPopulating)
                return change;

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                    ApplyEdgeCreate(edge);
            }

            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is DialogueNodeView nodeView)
                        nodeView.SaveLayout();
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                        ApplyEdgeRemove(edge);
                    else if (element is DialogueNodeView nodeView)
                        RemoveNodeFromGraph(nodeView);
                }
            }

            return change;
        }

        void ApplyEdgeCreate(Edge edge)
        {
            var sourceView = edge.output.node as DialogueNodeView;
            var targetView = edge.input.node as DialogueNodeView;
            if (sourceView == null || targetView == null)
                return;

            var sourceNode = sourceView.Node;
            var targetNode = targetView.Node;

            if (sourceNode.IsOptionNode)
            {
                int portIndex = sourceView.GetOutputPortIndex(edge.output);
                EnsureChoiceCount(sourceNode, portIndex + 1);
                sourceNode.ChoiceList[portIndex].toNode = targetNode;
            }
            else
            {
                sourceNode.AddLink(new DialogueLink
                {
                    toNode = targetNode,
                    condition = new DialogueCondition()
                });
            }

            EditorUtility.SetDirty(sourceNode);
        }

        void ApplyEdgeRemove(Edge edge)
        {
            var sourceView = edge.output.node as DialogueNodeView;
            var targetView = edge.input.node as DialogueNodeView;
            if (sourceView == null || targetView == null)
                return;

            var sourceNode = sourceView.Node;
            var targetNode = targetView.Node;

            if (sourceNode.IsOptionNode)
            {
                int portIndex = sourceView.GetOutputPortIndex(edge.output);
                if (portIndex >= 0 && portIndex < sourceNode.ChoiceList.Count)
                    sourceNode.ChoiceList[portIndex].toNode = null;
            }
            else if (sourceNode.LinkList != null)
            {
                sourceNode.LinkList.RemoveAll(link => link != null && link.toNode == targetNode);
            }

            EditorUtility.SetDirty(sourceNode);
        }

        void RemoveNodeFromGraph(DialogueNodeView nodeView)
        {
            if (currentGraph == null || nodeView?.Node == null)
                return;

            currentGraph.RemoveNode(nodeView.Node);
            DialogueGraphLayoutStore.RemoveNode(currentGraph, nodeView.Node);
            nodeViews.Remove(nodeView.Node);
            EditorUtility.SetDirty(currentGraph);
            ownerWindow.ForceMenuTreeRebuild();
            ownerWindow.ResetSelectionSync();
        }

        static void EnsureChoiceCount(DialogueNode node, int count)
        {
            if (node.ChoiceList == null)
                return;

            while (node.ChoiceList.Count < count)
            {
                node.AddChoice(new DialogueChoice
                {
                    labelText = $"选项{node.ChoiceList.Count + 1}",
                    condition = new DialogueCondition()
                });
            }
        }

        public void Populate(DialogueGraph graph, bool focusStartNode = false)
        {
            currentGraph = graph;
            isPopulating = true;
            try
            {
                DeleteElements(graphElements.ToList());
                nodeViews.Clear();

                if (graph?.NodeList == null)
                    return;

                int index = 0;
                foreach (var node in graph.NodeList)
                {
                    if (node == null)
                        continue;

                    var layout = DialogueGraphLayoutStore.GetPosition(graph, node);
                    if (layout == Vector2.zero)
                        layout = new Vector2(260f * index, 80f * (index % 4));

                    CreateNodeView(node, layout);
                    index++;
                }

                BuildEdges();
                HighlightStartNode();
                gridBackground.MarkDirtyRepaint();

                if (focusStartNode)
                    FocusStartNodeOrGraph();
            }
            finally
            {
                isPopulating = false;
            }
        }

        DialogueNodeView CreateNodeView(DialogueNode node, Vector2 position)
        {
            var view = new DialogueNodeView(node, currentGraph);
            view.SetPosition(new Rect(position, new Vector2(DefaultNodeWidth, DefaultNodeHeight)));
            AddElement(view);
            nodeViews[node] = view;

            view.capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable;
            view.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("设为起点", _ => SetStartNode(node));
                evt.menu.AppendAction("聚焦到左侧树", _ => ownerWindow.SelectObjectInTree(node));
            });

            return view;
        }

        void SetStartNode(DialogueNode node)
        {
            if (currentGraph == null || node == null)
                return;

            currentGraph.SetStartNodeInEditorWindow(node);
            EditorUtility.SetDirty(currentGraph);
            HighlightStartNode();
        }

        void HighlightStartNode()
        {
            foreach (var pair in nodeViews)
            {
                if (pair.Key == currentGraph.StartNode)
                    pair.Value.MarkAsStartNode();
            }
        }

        void BuildEdges()
        {
            foreach (var pair in nodeViews)
            {
                var node = pair.Key;
                var sourceView = pair.Value;

                if (node.IsOptionNode)
                {
                    sourceView.RebuildChoicePorts();
                    if (node.ChoiceList == null)
                        continue;

                    for (int i = 0; i < node.ChoiceList.Count; i++)
                    {
                        var choice = node.ChoiceList[i];
                        if (choice?.toNode == null)
                            continue;
                        if (!nodeViews.TryGetValue(choice.toNode, out var targetView))
                            continue;

                        var outPort = sourceView.GetOutputPort(i);
                        if (outPort == null)
                            continue;

                        var edge = outPort.ConnectTo(targetView.InputPort);
                        edge.userData = choice;
                        AddElement(edge);
                    }
                }
                else
                {
                    if (node.LinkList == null)
                        continue;

                    foreach (var link in node.LinkList)
                    {
                        if (link?.toNode == null)
                            continue;
                        if (!nodeViews.TryGetValue(link.toNode, out var targetView))
                            continue;

                        var edge = sourceView.OutputPort.ConnectTo(targetView.InputPort);
                        edge.userData = link;
                        AddElement(edge);
                    }
                }
            }
        }

        public void SelectNode(DialogueNode node)
        {
            if (node == null || !nodeViews.TryGetValue(node, out var view))
                return;

            RunWithoutSelectionBroadcast(() =>
            {
                ClearSelection();
                AddToSelection(view);
            });
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

        public void FocusNode(DialogueNode node)
        {
            if (node == null || !nodeViews.TryGetValue(node, out var view))
                return;

            SelectNode(node);
            schedule.Execute(() =>
            {
                FrameSelection();
                gridBackground.MarkDirtyRepaint();
            });
        }

        void FocusStartNodeOrGraph()
        {
            if (currentGraph?.StartNode != null && nodeViews.ContainsKey(currentGraph.StartNode))
            {
                FocusNode(currentGraph.StartNode);
                return;
            }

            FrameCurrentGraph();
        }

        public void FrameCurrentGraph()
        {
            if (nodeViews.Count == 0)
                return;

            schedule.Execute(() =>
            {
                FrameAll();
                gridBackground.MarkDirtyRepaint();
            });
        }

        public void RefreshCurrentGraph(bool preserveView)
        {
            if (currentGraph == null)
                return;

            var graph = currentGraph;
            var position = viewTransform.position;
            var scale = viewTransform.scale;

            Populate(graph);

            if (preserveView)
            {
                schedule.Execute(() =>
                {
                    UpdateViewTransform(position, scale);
                    gridBackground.MarkDirtyRepaint();
                });
            }
        }

        public void ApplySelection(object selected)
        {
            if (selected is DialogueGraph graph)
            {
                if (currentGraph != graph)
                    Populate(graph, focusStartNode: true);
                style.display = DisplayStyle.Flex;
                return;
            }

            if (selected is DialogueNode node)
            {
                var graphForNode = ownerWindow.FindGraphForNode(node);
                if (graphForNode == null)
                    return;

                if (currentGraph != graphForNode)
                    Populate(graphForNode, focusStartNode: true);

                SelectNode(node);
                style.display = DisplayStyle.Flex;
                return;
            }

            style.display = DisplayStyle.None;
        }

        public void RefreshNodeTitles()
        {
            foreach (var view in nodeViews.Values)
                view?.RefreshTitle();
        }
    }
}
#endif
