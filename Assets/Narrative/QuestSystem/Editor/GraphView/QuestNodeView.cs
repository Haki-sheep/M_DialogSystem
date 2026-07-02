#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务节点 GraphView 视图
    /// </summary>
    public class QuestNodeView : Node
    {
        public QuestNode Node { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public QuestNodeView(QuestNode node)
        {
            Node = node;
            UpdateTitle();
            viewDataKey = node.GetInstanceID().ToString();
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable;
            ApplyNodeColor(node.NodeType);

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>
        /// 刷新节点标题
        /// </summary>
        public void UpdateTitle()
        {
            title = $"[{Node.NodeId}] {Node.NodeType}";
            if (Node.NodeType == EQuestNodeType.Objective && Node.Objective != null)
            {
                string hint = string.IsNullOrEmpty(Node.Objective.targetKey)
                    ? Node.Objective.objectiveType.ToString()
                    : Node.Objective.targetKey;
                title += $"\n{hint}";
            }
        }

        void ApplyNodeColor(EQuestNodeType type)
        {
            titleContainer.style.backgroundColor = type switch
            {
                EQuestNodeType.Start => new Color(0.2f, 0.55f, 0.35f),
                EQuestNodeType.Objective => new Color(0.25f, 0.4f, 0.65f),
                EQuestNodeType.Branch => new Color(0.55f, 0.45f, 0.2f),
                EQuestNodeType.End => new Color(0.55f, 0.25f, 0.25f),
                _ => new Color(0.3f, 0.3f, 0.3f),
            };
        }
    }
}
#endif
