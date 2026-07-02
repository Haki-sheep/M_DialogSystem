#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图与节点资产删除
    /// </summary>
    static class DialogueGraphAssetDeleter
    {
        /// <summary>
        /// 删除整张对话图及其全部节点
        /// </summary>
        public static bool TryDeleteGraph(DialogueGraph graph)
        {
            if (!graph)
                return false;

            string graphName = graph.name;
            int nodeCount = graph.NodeList?.Count ?? 0;
            if (!EditorUtility.DisplayDialog(
                    "删除对话图",
                    $"确定删除「{graphName}」及其 {nodeCount} 个节点吗\n此操作不可撤销",
                    "删除",
                    "取消"))
                return false;

            var nodes = graph.NodeList != null ? new List<DialogueNode>(graph.NodeList) : new List<DialogueNode>();
            foreach (var node in nodes)
                DeleteNodeInternal(node, graph, clearReferences: false);

            DialogueGraphLayoutStore.RemoveGraph(graph);

            string graphPath = AssetDatabase.GetAssetPath(graph);
            if (!string.IsNullOrEmpty(graphPath))
                AssetDatabase.DeleteAsset(graphPath);

            AssetDatabase.SaveAssets();
            Debug.Log($"已删除对话图: {graphName}");
            return true;
        }

        /// <summary>
        /// 删除单个节点资产
        /// </summary>
        public static bool TryDeleteNode(DialogueNode node)
        {
            if (!node)
                return false;

            string nodeName = node.name;
            int nodeId = node.NodeId;
            var graph = FindGraphForNode(node);
            if (!EditorUtility.DisplayDialog(
                    "删除节点",
                    $"确定删除节点「{nodeName}」[{nodeId}] 吗\n此操作不可撤销",
                    "删除",
                    "取消"))
                return false;

            DeleteNodeInternal(node, graph, clearReferences: true);
            AssetDatabase.SaveAssets();
            Debug.Log($"已删除节点: {nodeName}");
            return true;
        }

        static void DeleteNodeInternal(DialogueNode node, DialogueGraph graph, bool clearReferences)
        {
            if (node == null)
                return;

            if (graph != null)
            {
                if (clearReferences)
                    ClearReferencesToNode(graph, node);

                if (graph.StartNode == node)
                    graph.SetStartNodeInEditorWindow(null);

                graph.RemoveNode(node);
                DialogueGraphLayoutStore.RemoveNode(graph, node);
                EditorUtility.SetDirty(graph);
            }

            string path = AssetDatabase.GetAssetPath(node);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);
        }

        static void ClearReferencesToNode(DialogueGraph graph, DialogueNode target)
        {
            if (graph?.NodeList == null)
                return;

            foreach (var node in graph.NodeList)
            {
                if (node == null || node == target)
                    continue;

                if (node.NextTransition?.toNode == target)
                    node.ClearNextNode();

                if (node.ChoiceList == null)
                    continue;

                foreach (var choice in node.ChoiceList)
                {
                    if (choice != null && choice.toNode == target)
                        choice.toNode = null;
                }

                EditorUtility.SetDirty(node);
            }
        }

        static DialogueGraph FindGraphForNode(DialogueNode node)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(DialogueGraph)}"))
            {
                var graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(AssetDatabase.GUIDToAssetPath(guid));
                if (graph?.NodeList != null && graph.NodeList.Contains(node))
                    return graph;
            }

            return null;
        }
    }
}
#endif
