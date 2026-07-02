#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务流程图编辑器主窗口
    /// </summary>
    public partial class QuestGraphEditorWindow : EditorWindow
    {
        OdinMenuTree menuTree;
        QuestGraphView graphView;
        VisualElement rowElement;
        VisualElement graphPanel;
        QuestGraph lastSelectedGraph;
        QuestNode selectedNode;
        bool uiBuilt;

        #region 入口

        [MenuItem("Tools/MmQuestWindow")]
        static void Open()
        {
            var window = GetWindow<QuestGraphEditorWindow>();
            window.titleContent = new GUIContent("Quest System");
            window.minSize = new Vector2(960, 560);
            window.Show();
        }

        void CreateGUI()
        {
            uiBuilt = false;
            EnsureMenuTree();
            SetupUI();
        }

        void OnEnable() => EditorApplication.projectChanged += OnProjectChanged;

        void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            ReleaseAllMouseCaptures();
            menuTree = null;
            uiBuilt = false;
        }

        void OnProjectChanged()
        {
            menuTree = null;
            EnsureMenuTree();
            Repaint();
        }

        void OnInspectorUpdate() => UpdateWindowLayout();

        internal QuestGraph GetActiveGraph()
        {
            var selected = menuTree?.Selection?.SelectedValue as QuestGraph;
            return selected != null ? selected : lastSelectedGraph;
        }

        internal QuestNode SelectedNode => selectedNode;
        internal QuestGraphView GraphView => graphView;

        internal float GetInspectorPanelWidth()
        {
            if (editorLayout?.RightColumn == null)
                return QuestGraphEditorConstants.InspectorPanelWidth;

            return editorLayout.ResolveColumnWidth(editorLayout.RightColumn);
        }

        #endregion

        #region 工具栏与左栏

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("新建任务图", EditorStyles.toolbarButton))
                CreateQuestGraph();

            if (graphView != null && graphView.CurrentGraph != null)
            {
                if (GUILayout.Button("Start", EditorStyles.toolbarButton))
                    graphView.CreateNode(EQuestNodeType.Start, new Vector2(120, 80));
                if (GUILayout.Button("Objective", EditorStyles.toolbarButton))
                    graphView.CreateNode(EQuestNodeType.Objective, new Vector2(280, 80));
                if (GUILayout.Button("Branch", EditorStyles.toolbarButton))
                    graphView.CreateNode(EQuestNodeType.Branch, new Vector2(440, 80));
                if (GUILayout.Button("End", EditorStyles.toolbarButton))
                    graphView.CreateNode(EQuestNodeType.End, new Vector2(600, 80));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DrawLeftPanel()
        {
            GraphViewFramePanelStyles.DrawPanelHeader("任务图", GetActiveGraph()?.name ?? "未选中");
            GraphViewFramePanelStyles.BeginPaddedContent();

            if (menuTree != null)
            {
                menuTree.DrawMenuTree();
                menuTree.HandleKeyboardMenuNavigation();
            }

            GraphViewFramePanelStyles.EndPaddedContent();
        }

        #endregion

        #region 资产

        void CreateQuestGraph()
        {
            QuestEditorPaths.EnsureGraphAssetFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{QuestEditorPaths.GraphAssetPath}/New Quest Graph.asset");
            var graph = CreateInstance<QuestGraph>();
            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            menuTree = null;
            ForceMenuTreeRebuild();
            Repaint();
        }

        #endregion
    }
}
#endif
