#if UNITY_EDITOR
using System.Collections.Generic;
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
            ApplyTitleLayout();
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
            string header = DialogueMenuTreeUtility.BuildNodeHeader(Node);
            if (string.IsNullOrEmpty(Node.DialogText))
                return header;

            string text = DialogueMenuTreeUtility.Truncate(
                Node.DialogText,
                DialogueGraphEditorConstants.MenuNodeDialogTextMaxLength);
            return $"{header}\n{text}";
        }

        void ApplyTitleLayout()
        {
            titleContainer.style.minHeight = 44;
            titleContainer.style.height = StyleKeyword.Auto;
            titleContainer.style.whiteSpace = WhiteSpace.Normal;
        }

        public void RefreshTitle()
        {
            title = BuildTitle();
            ApplyTitleLayout();
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
            SyncChoicePorts();
        }

        /// <summary>
        /// 增量同步选项出口 不破坏已有连线
        /// </summary>
        public void SyncChoicePorts()
        {
            if (Node.ChoiceList == null || Node.ChoiceList.Count == 0)
                Node.AddChoiceNode(new DialogueOptionTransition { labelText = "选项1" });

            while (outputPorts.Count > Node.ChoiceList.Count)
            {
                var lastPort = outputPorts[outputPorts.Count - 1];
                outputContainer.Remove(lastPort);
                outputPorts.RemoveAt(outputPorts.Count - 1);
            }

            while (outputPorts.Count < Node.ChoiceList.Count)
                AppendChoiceOutputPort(outputPorts.Count);

            for (int i = 0; i < outputPorts.Count && i < Node.ChoiceList.Count; i++)
                outputPorts[i].portName = GetChoicePortLabel(i);

            RefreshExpandedState();
            RefreshPorts();
        }

        void AppendChoiceOutputPort(int index)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = GetChoicePortLabel(index);
            port.userData = index;
            outputContainer.Add(port);
            outputPorts.Add(port);
        }

        string GetChoicePortLabel(int index)
        {
            if (Node.ChoiceList != null && index < Node.ChoiceList.Count && Node.ChoiceList[index] != null)
            {
                var label = Node.ChoiceList[index].labelText;
                if (!string.IsNullOrEmpty(label))
                    return label;
            }

            return $"选项{index + 1}";
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
}
#endif
