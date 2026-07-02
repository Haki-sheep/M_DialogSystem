#if UNITY_EDITOR
using System.Linq;
using Miemie.Narrative.GraphViewFrame.Editor;
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
                DefaultMenuStyle = GraphViewFramePanelStyles.CreateMenuStyle(),
            };

            GraphAssetMenuTreeUtility.AddAllAssetsByType(tree, "对话图", typeof(DialogueGraph));

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
                    string displayLabel = DialogueMenuTreeUtility.BuildNodeHeader(node);
                    string menuPath = BuildNodeMenuPath(basePath, node);
                    var menuItem = tree.Add(menuPath, node).LastOrDefault();
                    if (menuItem != null)
                        menuItem.Name = displayLabel;
                    nodeLabelCache[node] = displayLabel;
                }
            }

            tree.EnumerateTree().AddThumbnailIcons(true);
            return tree;
        }

        static string BuildNodeMenuPath(string basePath, DialogueNode node) =>
            $"{basePath}/[{node.NodeId}] {DialogueMenuTreeUtility.SanitizeMenuPath(node.name)}";

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

                string newLabel = DialogueMenuTreeUtility.BuildNodeHeader(node);
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
