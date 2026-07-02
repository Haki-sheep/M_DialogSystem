#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Miemie.Narrative.GraphViewFrame.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务图 GraphView 画布
    /// </summary>
    public class QuestGraphView : NarrativeGraphViewBase
    {
        readonly Dictionary<QuestNode, QuestNodeView> nodeViews = new();
        QuestGraph currentGraph;

        public QuestGraph CurrentGraph => currentGraph;
        public System.Action<QuestNode> OnNodeSelected;

        public QuestGraphView() : base("quest-graph-grid")
        {
            graphViewChanged = OnGraphViewChanged;
        }

        protected override void OnGraphLeftMouseUp(MouseUpEvent evt) =>
            schedule.Execute(SyncNodeSelection);

        void SyncNodeSelection()
        {
            QuestNode selected = null;
            foreach (var item in selection)
            {
                if (item is QuestNodeView nodeView)
                {
                    selected = nodeView.Node;
                    break;
                }
            }

            OnNodeSelected?.Invoke(selected);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (currentGraph == null)
            {
                base.BuildContextualMenu(evt);
                return;
            }

            var graphPos = contentViewContainer.WorldToLocal(evt.localMousePosition);
            evt.menu.AppendAction("Start 节点", _ => CreateNode(EQuestNodeType.Start, graphPos));
            evt.menu.AppendAction("Objective 节点", _ => CreateNode(EQuestNodeType.Objective, graphPos));
            evt.menu.AppendAction("Branch 节点", _ => CreateNode(EQuestNodeType.Branch, graphPos));
            evt.menu.AppendAction("End 节点", _ => CreateNode(EQuestNodeType.End, graphPos));
            base.BuildContextualMenu(evt);
        }

        /// <summary>
        /// 刷新节点视图标题
        /// </summary>
        public void RefreshNodeView(QuestNode node)
        {
            if (node != null && nodeViews.TryGetValue(node, out var view))
                view.UpdateTitle();
        }

        #region 图加载

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter) =>
            ports.ToList().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node).ToList();

        public void LoadGraph(QuestGraph graph)
        {
            currentGraph = graph;
            DeleteElements(graphElements.ToList());

            if (graph?.NodeList == null)
                return;

            nodeViews.Clear();
            foreach (var node in graph.NodeList)
            {
                if (node == null)
                    continue;

                var view = new QuestNodeView(node);
                var pos = QuestGraphLayoutStore.GetPosition(graph, node);
                if (pos == Vector2.zero)
                    pos = new Vector2(120 + node.NodeId * 40f, 80 + node.NodeId * 30f);

                view.SetPosition(new Rect(pos, new Vector2(180, 80)));
                AddElement(view);
                nodeViews[node] = view;
            }

            RebuildEdges();
        }

        void RebuildEdges()
        {
            if (currentGraph?.NodeList == null)
                return;

            foreach (var node in currentGraph.NodeList)
            {
                if (node == null || !nodeViews.TryGetValue(node, out var fromView))
                    continue;

                var toNode = node.NextTransition?.toNode;
                if (toNode == null || !nodeViews.TryGetValue(toNode, out var toView))
                    continue;

                AddElement(fromView.OutputPort.ConnectTo(toView.InputPort));
            }
        }

        #endregion

        #region 图变更

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                    BindEdge(edge);
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is QuestNodeView nodeView)
                        RemoveNodeAsset(nodeView.Node);
                    else if (element is Edge edge)
                        UnbindEdge(edge);
                }
            }

            if (change.movedElements != null && currentGraph != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is QuestNodeView nodeView)
                    {
                        var rect = nodeView.GetPosition();
                        QuestGraphLayoutStore.SetPosition(currentGraph, nodeView.Node, rect.position);
                    }
                }
            }

            return change;
        }

        void BindEdge(Edge edge)
        {
            if (edge.output?.node is not QuestNodeView from || edge.input?.node is not QuestNodeView to)
                return;

            var so = new SerializedObject(from.Node);
            var transition = so.FindProperty("nextTransition");
            transition.FindPropertyRelative("toNode").objectReferenceValue = to.Node;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(from.Node);
        }

        void UnbindEdge(Edge edge)
        {
            if (edge.output?.node is not QuestNodeView from)
                return;

            var so = new SerializedObject(from.Node);
            var transition = so.FindProperty("nextTransition");
            transition.FindPropertyRelative("toNode").objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(from.Node);
        }

        void RemoveNodeAsset(QuestNode node)
        {
            if (currentGraph == null || node == null)
                return;

            currentGraph.NodeList.Remove(node);
            if (currentGraph.StartNode == node)
                currentGraph.SetStartNodeInEditor(null);

            EditorUtility.SetDirty(currentGraph);
            AssetDatabase.RemoveObjectFromAsset(node);
            Object.DestroyImmediate(node, true);
        }

        #endregion

        #region 节点创建

        public void CreateNode(EQuestNodeType nodeType, Vector2 graphPosition)
        {
            if (currentGraph == null)
                return;

            QuestEditorPaths.EnsureGraphAssetFolder();
            int nextId = 1;
            if (currentGraph.NodeList != null && currentGraph.NodeList.Count > 0)
                nextId = currentGraph.NodeList.Max(n => n != null ? n.NodeId : 0) + 1;

            var node = ScriptableObject.CreateInstance<QuestNode>();
            node.name = $"QuestNode_{nextId}";
            node.SetNodeIdInEditor(nextId);
            node.SetNodeTypeInEditor(nodeType);

            AssetDatabase.AddObjectToAsset(node, currentGraph);
            currentGraph.AddNode(node);

            if (currentGraph.StartNode == null && nodeType == EQuestNodeType.Start)
                currentGraph.SetStartNodeInEditor(node);

            AssetDatabase.SaveAssets();

            var view = new QuestNodeView(node);
            view.SetPosition(new Rect(graphPosition, new Vector2(180, 80)));
            AddElement(view);
            nodeViews[node] = view;
            QuestGraphLayoutStore.SetPosition(currentGraph, node, graphPosition);
            EditorUtility.SetDirty(currentGraph);
        }

        #endregion
    }
}
#endif
