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
        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (currentGraph == null || isPopulating || isRebindingEdges)
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
            var sourceView = edge.output?.node as DialogueNodeView;
            var targetView = edge.input?.node as DialogueNodeView;
            if (sourceView == null || targetView == null)
                return;

            var sourceNode = sourceView.Node;
            var targetNode = targetView.Node;

            if (sourceNode.IsOptionNode)
            {
                int portIndex = sourceView.GetOutputPortIndex(edge.output);
                EnsureChoiceCount(sourceNode, portIndex + 1);
                var choice = sourceNode.ChoiceList[portIndex];
                choice.toNode = targetNode;
                edge.userData = choice;
                sourceView.SyncChoicePorts();
            }
            else
            {
                sourceNode.SetNextNode(targetNode);
                edge.userData = "linear";
            }

            RegisterEdgeSelection(edge);
            EditorUtility.SetDirty(sourceNode);
        }

        void ApplyEdgeRemove(Edge edge)
        {
            if (edge?.userData == null)
                return;

            var sourceView = edge.output?.node as DialogueNodeView;
            if (sourceView == null)
                return;

            var sourceNode = sourceView.Node;

            if (sourceNode.IsOptionNode && edge.userData is DialogueOptionTransition optionTransition)
            {
                foreach (var item in sourceNode.ChoiceList)
                {
                    if (!ReferenceEquals(item, optionTransition))
                        continue;
                    optionTransition.toNode = null;
                    break;
                }
                sourceView.SyncChoicePorts();
            }
            else if (!sourceNode.IsOptionNode && edge.userData is string tag && tag == "linear")
            {
                sourceNode.ClearNextNode();
            }

            EditorUtility.SetDirty(sourceNode);
        }

        void BuildEdges()
        {
            foreach (var pair in nodeViews)
            {
                var node = pair.Key;
                var sourceView = pair.Value;

                if (node.IsOptionNode)
                {
                    sourceView.SyncChoicePorts();

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
                        RegisterEdgeSelection(edge);
                        AddElement(edge);
                    }
                }
                else
                {
                    var next = node.NextTransition?.toNode;
                    if (next == null)
                        continue;
                    if (!nodeViews.TryGetValue(next, out var targetView))
                        continue;

                    var edge = sourceView.OutputPort.ConnectTo(targetView.InputPort);
                    edge.userData = "linear";
                    RegisterEdgeSelection(edge);
                    AddElement(edge);
                }
            }
        }

        void RegisterEdgeSelection(Edge edge)
        {
            edge.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;

                ClearSelection();
                AddToSelection(edge);

                var handle = BuildTransitionHandle(edge);
                if (handle != null)
                    ownerWindow.SelectTransition(handle);

                evt.StopPropagation();
            });
        }

        static void EnsureChoiceCount(DialogueNode node, int count)
        {
            if (node.ChoiceList == null)
                return;

            while (node.ChoiceList.Count < count)
            {
                node.AddChoiceNode(new DialogueOptionTransition
                {
                    labelText = $"选项{node.ChoiceList.Count + 1}",
                });
            }
        }
    }
}
#endif
