#if UNITY_EDITOR
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 画布上选中的连线句柄
    /// </summary>
    public class DialogueTransitionHandle
    {
        public DialogueGraph graph;
        public DialogueNode sourceNode;
        public DialogueNode targetNode;
        public DialogueChoice choice;

        public bool IsChoice => choice != null;

        public string Title
        {
            get
            {
                string from = sourceNode != null ? $"[{sourceNode.NodeId}]" : "?";
                string to = targetNode != null ? $"[{targetNode.NodeId}]" : "?";
                return IsChoice ? $"{from} 选项 → {to}" : $"{from} → {to}";
            }
        }
    }
}
#endif
