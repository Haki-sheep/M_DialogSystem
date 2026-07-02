#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Miemie.Narrative.GraphViewFrame.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图 GraphView 画布
    /// </summary>
    public partial class DialogueGraphView : NarrativeGraphViewBase
    {
        internal const float DefaultNodeWidth = 220f;
        internal const float DefaultNodeHeight = 120f;

        readonly DialogueGraphEditorWindow ownerWindow;
        readonly Dictionary<DialogueNode, DialogueNodeView> nodeViews = new Dictionary<DialogueNode, DialogueNodeView>();

        DialogueGraph currentGraph;
        bool isPopulating;
        bool suppressSelectionBroadcast;
        bool isRebindingEdges;

        public DialogueGraph CurrentGraph => currentGraph;

        public DialogueGraphView(DialogueGraphEditorWindow ownerWindow) : base("dialog-graph-grid")
        {
            this.ownerWindow = ownerWindow;
            graphViewChanged = OnGraphViewChanged;
        }

        protected override void OnGraphLeftMouseUp(MouseUpEvent evt)
        {
            if (FindPort(evt.target as VisualElement) != null)
                return;

            schedule.Execute(SyncGraphSelectionToTree);
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

            if (edge.userData is DialogueTransition choiceTransition)
                handle.choiceTransition = choiceTransition;
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
    }
}
#endif
