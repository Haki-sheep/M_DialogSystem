#if UNITY_EDITOR
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        internal void EnsureMenuTreeInternal()
        {
            if (menuTree != null)
                return;

            ForceMenuTreeRebuild();
        }

        internal void ForceMenuTreeRebuild()
        {
            suppressMenuSelectionRefresh = true;
            try
            {
                UnhookSelectionChanged();
                menuTree = BuildMenuTree();
                TryHookSelectionChanged();
            }
            finally
            {
                suppressMenuSelectionRefresh = false;
            }
        }

        internal DialogueGraph GetActiveGraph()
        {
            var selected = MenuTree?.Selection?.SelectedValue;
            if (selected is DialogueGraph graph && IsAssetAlive(graph))
                return graph;

            return IsAssetAlive(lastSelectedGraph) ? lastSelectedGraph : null;
        }

        void TryHookSelectionChanged()
        {
            if (selectionHooked || MenuTree?.Selection == null)
                return;

            MenuTree.Selection.SelectionChanged += OnMenuSelectionChanged;
            selectionHooked = true;
        }

        void UnhookSelectionChanged()
        {
            if (!selectionHooked || MenuTree?.Selection == null)
                return;

            MenuTree.Selection.SelectionChanged -= OnMenuSelectionChanged;
            selectionHooked = false;
        }

        bool suppressMenuSelectionRefresh;

        void OnMenuSelectionChanged(SelectionChangedType _)
        {
            if (suppressMenuSelectionRefresh)
                return;

            RefreshFromMenuSelection();
        }

        void RefreshFromMenuSelection()
        {
            selectedTransition = null;
            var selectedObject = MenuTree?.Selection?.SelectedValue as UnityEngine.Object;

            if (!ReferenceEquals(inspectorTreeTarget, selectedObject))
                ClearInspectorTree();

            if (selectedObject != null)
            {
                if (!ReferenceEquals(renameTarget, selectedObject))
                    SetRenameTarget(selectedObject, selectedObject.name);
            }
            else
            {
                ClearRenameTarget();
            }

            SyncSelectionToGraphView();
            Repaint();
        }

        OdinMenuTree BuildMenuTree()
        {
            nodeToGraph.Clear();
            nodeLabelCache.Clear();

            var tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                Config = { DrawSearchToolbar = true },
                DefaultMenuStyle = DialogueEditorPanelStyles.CreateMenuStyle(),
            };

            tree.AddAllAssetsAtPath("对话图", DialogueEditorPaths.GraphAssetPath, typeof(DialogueGraph), includeSubDirectories: true);

            foreach (var item in tree.EnumerateTree().Where(i => i.Value is DialogueGraph).ToList())
            {
                var graph = (DialogueGraph)item.Value;
                if (graph.NodeList == null)
                    continue;

                string basePath = item.GetFullPath();
                foreach (var node in graph.NodeList)
                {
                    if (node == null)
                        continue;

                    nodeToGraph[node] = graph;
                    string label = BuildNodeLabel(node);
                    nodeLabelCache[node] = label;
                    tree.Add($"{basePath}/{DialogueMenuTreeUtility.SanitizeMenuPath(label)}", node);
                }
            }

            tree.EnumerateTree().AddThumbnailIcons(true);
            return tree;
        }

        static string BuildNodeLabel(DialogueNode node) =>
            $"[{node.NodeId}] {node.SpeakerName}: {DialogueMenuTreeUtility.Truncate(node.DialogText, 12)}";

        void RefreshMenuLabelsIfNeeded()
        {
            if (MenuTree == null)
                return;

            bool dirty = false;
            foreach (var item in MenuTree.EnumerateTree())
            {
                if (item.Value is DialogueGraph graph)
                {
                    if (item.Name != graph.name)
                    {
                        item.Name = graph.name;
                        dirty = true;
                    }
                    continue;
                }

                if (item.Value is not DialogueNode node)
                    continue;

                if (!IsAssetAlive(node))
                    continue;

                string newLabel = BuildNodeLabel(node);
                if (!nodeLabelCache.TryGetValue(node, out var oldLabel) || oldLabel != newLabel)
                {
                    nodeLabelCache[node] = newLabel;
                    item.Name = newLabel;
                    dirty = true;
                }
            }

            if (dirty)
            {
                MenuTree.MarkDirty();
                QueueGraphViewRefreshTitles();
            }
        }

        void QueueGraphViewRefreshTitles()
        {
            if (graphView == null || graphTitlesRefreshScheduled)
                return;

            graphTitlesRefreshScheduled = true;
            graphView.schedule.Execute(() =>
            {
                graphTitlesRefreshScheduled = false;
                graphView?.RefreshNodeTitles();
            });
        }

        internal void RequestMenuLabelRefreshOnly()
        {
            RefreshMenuLabelsIfNeeded();
            QueueGraphViewRefreshTitles();
            Repaint();
        }
    }
}
#endif
