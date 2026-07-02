#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Miemie.DialogSystem.Quest.Editor
{
    partial class QuestGraphEditorWindow
    {
        #region 菜单树

        void EnsureMenuTree()
        {
            if (menuTree != null)
                return;

            ForceMenuTreeRebuild();
        }

        void ForceMenuTreeRebuild()
        {
            menuTree = BuildMenuTree();
            menuTree.Selection.SelectionChanged += _ => SyncGraphSelection();
        }

        OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false)
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    DefaultMenuStyle = GraphViewFramePanelStyles.CreateMenuStyle(),
                },
            };

            GraphAssetMenuTreeUtility.AddAllAssetsByType(tree, "任务图", typeof(QuestGraph));
            tree.EnumerateTree().AddThumbnailIcons(true);
            return tree;
        }

        void SyncGraphSelection()
        {
            lastSelectedGraph = menuTree?.Selection?.SelectedValue as QuestGraph;
            selectedNode = null;
            graphView?.LoadGraph(lastSelectedGraph);
            Repaint();
        }

        #endregion
    }
}
#endif
