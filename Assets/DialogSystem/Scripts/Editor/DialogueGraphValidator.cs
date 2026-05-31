#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图校验工具
    /// </summary>
    public static class DialogueGraphValidator
    {
        public static void Validate(DialogueGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("未选中任何 DialogueGraph");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== 校验 {graph.name} ===");

            if (graph.StartNode == null)
                sb.AppendLine("[错误] startNode 未设置");

            if (graph.NodeList == null || graph.NodeList.Count == 0)
                sb.AppendLine("[警告] nodeList 为空");

            var inGraph = new HashSet<DialogueNode>();
            if (graph.NodeList != null)
            {
                foreach (var node in graph.NodeList)
                {
                    if (node == null)
                    {
                        sb.AppendLine("[错误] nodeList 中有空引用");
                        continue;
                    }
                    inGraph.Add(node);
                }
            }

            if (graph.StartNode != null && !inGraph.Contains(graph.StartNode))
                sb.AppendLine("[警告] startNode 不在 nodeList 中");

            if (graph.NodeList != null)
            {
                foreach (var node in graph.NodeList)
                {
                    if (node == null)
                        continue;
                    ValidateNode(node, inGraph, sb);
                }
            }

            sb.AppendLine("=== 结束 ===");
            Debug.Log(sb.ToString());
        }

        static void ValidateNode(DialogueNode node, HashSet<DialogueNode> inGraph, StringBuilder sb)
        {
            if (node.IsOptionNode)
            {
                if (node.ChoiceList == null || node.ChoiceList.Count == 0)
                    sb.AppendLine($"[错误] 选项节点 [{node.NodeId}] choiceList 为空");
                else
                    ValidateChoices(node, inGraph, sb);
            }
            else
            {
                if (node.LinkList == null || node.LinkList.Count == 0)
                    sb.AppendLine($"[提示] 节点 [{node.NodeId}] 无出口（可能是结局）");
                else
                    ValidateLinks(node, inGraph, sb);
            }
        }

        static void ValidateLinks(DialogueNode node, HashSet<DialogueNode> inGraph, StringBuilder sb)
        {
            foreach (var link in node.LinkList)
            {
                if (link?.toNode == null)
                    sb.AppendLine($"[错误] [{node.NodeId}] 存在空 toNode 的连线");
                else if (!inGraph.Contains(link.toNode))
                    sb.AppendLine($"[警告] [{node.NodeId}] → {link.toNode.name} 不在本图 nodeList");
            }
        }

        static void ValidateChoices(DialogueNode node, HashSet<DialogueNode> inGraph, StringBuilder sb)
        {
            foreach (var choice in node.ChoiceList)
            {
                if (choice?.toNode == null)
                    sb.AppendLine($"[错误] [{node.NodeId}] 选项「{choice?.labelText}」无 toNode");
                else if (!inGraph.Contains(choice.toNode))
                    sb.AppendLine($"[警告] 选项 → {choice.toNode.name} 不在本图 nodeList");
            }
        }
    }
}
#endif
