#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphView
    {
        /// <summary>
        /// 当前视口中心对应的画布坐标
        /// </summary>
        Vector2 GetViewCenterGraphPosition()
        {
            return contentViewContainer.WorldToLocal(worldBound.center);
        }

        void CreateNodeAtViewCenter()
        {
            if (currentGraph == null)
                return;

            var graphPosition = GetViewCenterGraphPosition();
            graphPosition -= new Vector2(DefaultNodeWidth * 0.5f, DefaultNodeHeight * 0.5f);
            CreateNodeAt(graphPosition);
        }

        void CreateNodeAt(Vector2 graphPosition)
        {
            if (currentGraph == null)
                return;

            var node = ownerWindow.CreateNodeAsset(currentGraph);
            if (node == null)
                return;

            currentGraph.AddNode(node);
            EditorUtility.SetDirty(currentGraph);

            var view = CreateNodeView(node, graphPosition);
            view.SaveLayout();
            ownerWindow.OnNodeCreated(node, currentGraph);
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
                if (node.IsOptionNode)
                {
                    evt.menu.AppendAction("添加选项出口", _ => AddChoicePort(node, view));
                    if ((node.ChoiceList?.Count ?? 0) > 1)
                        evt.menu.AppendAction("删除最后选项", _ => RemoveLastChoicePort(node));
                }
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

        void AddChoicePort(DialogueNode node, DialogueNodeView view)
        {
            if (node == null || !node.IsOptionNode)
                return;

            DialogueNodeChoiceEditorUtility.AddChoice(node);
            view.SyncChoicePorts();
        }

        void RemoveLastChoicePort(DialogueNode node)
        {
            if (node == null || !node.IsOptionNode)
                return;

            if (!DialogueNodeChoiceEditorUtility.TryRemoveLastChoice(node))
                return;

            ownerWindow.QueueGraphViewRefreshFromInspector(node);
        }

        void HighlightStartNode()
        {
            foreach (var pair in nodeViews)
            {
                if (pair.Key == currentGraph.StartNode)
                    pair.Value.MarkAsStartNode();
            }
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

        /// <summary>
        /// 清空画布
        /// </summary>
        public void ClearGraph()
        {
            currentGraph = null;
            isPopulating = true;
            try
            {
                DeleteElements(graphElements.ToList());
                nodeViews.Clear();
            }
            finally
            {
                isPopulating = false;
            }

            style.display = DisplayStyle.None;
            gridBackground.MarkDirtyRepaint();
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
    }
}
#endif
