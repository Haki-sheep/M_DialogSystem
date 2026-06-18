#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        public DialogueGraph FindGraphForNode(DialogueNode node)
        {
            if (node == null)
                return null;

            if (nodeToGraph.TryGetValue(node, out var graph))
                return graph;

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(DialogueGraph)}"))
            {
                var g = AssetDatabase.LoadAssetAtPath<DialogueGraph>(AssetDatabase.GUIDToAssetPath(guid));
                if (g?.NodeList != null && g.NodeList.Contains(node))
                    return g;
            }

            return null;
        }

        public DialogueNode CreateNodeAsset(DialogueGraph graph)
        {
            DialogueEditorPaths.EnsureGraphAssetFolder();

            var node = CreateInstance<DialogueNode>();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DialogueEditorPaths.GraphAssetPath}/New Dialog Node.asset");
            AssetDatabase.CreateAsset(node, assetPath);
            AssetDatabase.SaveAssets();

            if (graph.NodeList != null && graph.NodeList.Count > 0)
            {
                int maxId = graph.NodeList.Where(n => n != null).Select(n => n.NodeId).DefaultIfEmpty(0).Max();
                var so = new SerializedObject(node);
                so.FindProperty("nodeId").intValue = maxId + 1;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return node;
        }

        internal void CommitAssetRename(Object selected)
        {
            if (selected == null || RenameBuffer == selected.name)
                return;

            if (DialogueGraphAssetRenameUtility.Apply(selected, RenameBuffer, ref renameBuffer))
                RefreshAfterExternalChange(selected);
        }

        internal void RefreshAfterExternalChange(Object target)
        {
            if (target == null)
                return;

            SetRenameTarget(target, target.name);
            ClearInspectorTree();
            RequestMenuRefresh();
            EditorApplication.delayCall += () => SelectObjectInTree(target);
        }

        internal void TryDeleteSelectedAsset(Object selected)
        {
            if (!IsAssetAlive(selected))
                return;

            selectedTransition = null;
            ClearInspectorTree();

            if (selected is DialogueGraph graph)
            {
                if (!DialogueGraphAssetDeleter.TryDeleteGraph(graph))
                    return;

                lastSelectedGraph = null;
                lastSyncedSelection = null;
                ClearRenameTarget();
                ClearStaleSelectionInternal();
                RequestMenuRefresh();
                graphView?.ClearGraph();
                return;
            }

            if (selected is DialogueNode node && node)
            {
                var parentGraph = FindGraphForNode(node);
                if (!DialogueGraphAssetDeleter.TryDeleteNode(node))
                    return;

                lastSyncedSelection = null;
                ClearRenameTarget();
                RequestMenuRefresh();

                if (parentGraph != null)
                {
                    lastSelectedGraph = parentGraph;
                    EditorApplication.delayCall += () =>
                    {
                        SelectObjectInTree(parentGraph);
                        graphView?.RefreshCurrentGraph(preserveView: true);
                    };
                }
            }
        }
    }
}
#endif
