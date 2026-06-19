#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话系统编辑器主窗口
    /// </summary>
    public partial class DialogueGraphEditorWindow : EditorWindow
    {
        internal enum ResizeTarget { None, Left, Parameters, Right }

        OdinMenuTree menuTree;

        DialogueGraphView graphView;
        VisualElement rowElement;
        VisualElement graphPanel;
        VisualElement leftSplitter;
        VisualElement parametersSplitter;
        VisualElement rightSplitter;
        IMGUIContainer leftPanelContainer;
        IMGUIContainer parametersPanelContainer;
        IMGUIContainer rightPanelContainer;

        PropertyTree inspectorTree;
        DialogueGraph lastSelectedGraph;
        DialogueTransitionHandle selectedTransition;
        object lastSyncedSelection;
        UnityEngine.Object inspectorTreeTarget;
        string renameBuffer = "";
        UnityEngine.Object renameTarget;
        Vector2 inspectorScroll;
        Vector2 parametersScroll;
        float leftPanelWidth = DialogueGraphEditorConstants.MenuPanelWidth;
        float parametersPanelWidth = DialogueGraphEditorConstants.ParametersPanelWidth;
        float rightPanelWidth = DialogueGraphEditorConstants.InspectorPanelWidth;
        float resizeStartX;
        float resizeStartWidth;
        double nextMenuLabelRefreshTime;
        ResizeTarget activeResize = ResizeTarget.None;
        bool uiBuilt;
        bool selectionHooked;
        bool renameFieldWasFocused;
        bool graphTitlesRefreshScheduled;
        bool graphSelectionUpdateScheduled;
        object pendingGraphSelection;

        readonly Dictionary<DialogueNode, DialogueGraph> nodeToGraph = new();
        readonly Dictionary<DialogueNode, string> nodeLabelCache = new();

        internal OdinMenuTree MenuTree => menuTree;
        internal OdinMenuTree MenuTreeAccessor => menuTree;
        internal DialogueGraphView GraphView => graphView;
        internal DialogueGraph LastSelectedGraph => lastSelectedGraph;
        internal DialogueTransitionHandle SelectedTransition => selectedTransition;
        internal Vector2 ParametersScroll { get => parametersScroll; set => parametersScroll = value; }
        internal Vector2 InspectorScroll { get => inspectorScroll; set => inspectorScroll = value; }
        internal string RenameBuffer { get => renameBuffer; set => renameBuffer = value; }
        internal UnityEngine.Object RenameTarget => renameTarget;
        internal PropertyTree InspectorTree => inspectorTree;
        internal UnityEngine.Object InspectorTreeTarget => inspectorTreeTarget;

        [MenuItem("Tools/MmDialogWindow")]
        static void Open()
        {
            var window = GetWindow<DialogueGraphEditorWindow>();
            window.titleContent = new GUIContent("Dialog System");
            window.minSize = new Vector2(960, 560);
            window.Show();
        }

        void OnEnable()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void CreateGUI()
        {
            uiBuilt = false;
            EnsureMenuTreeReady();
            SetupThreePanelUI();
            ResetEditorStateIfEmpty();
        }

        void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            ReleaseAllMouseCaptures();
            UnhookSelectionChanged();
            ClearInspectorTree();
            menuTree = null;
            uiBuilt = false;
        }

        void OnDestroy()
        {
            uiBuilt = false;
            selectionHooked = false;
            if (graphView != null && graphView.parent != null)
                graphView.RemoveFromHierarchy();
            ClearInspectorTree();
        }

        void OnInspectorUpdate()
        {
            UpdateWindowLayout();
            if (EditorApplication.timeSinceStartup < nextMenuLabelRefreshTime)
                return;

            nextMenuLabelRefreshTime = EditorApplication.timeSinceStartup + DialogueGraphEditorConstants.MenuLabelRefreshInterval;
            RefreshMenuLabelsIfNeeded();
        }

        void OnProjectChanged() => RequestMenuRefresh();
        void OnUndoRedo() => RequestMenuRefresh();

        void EnsureMenuTreeReady()
        {
            if (menuTree != null)
                return;

            ForceMenuTreeRebuild();
        }

        internal void ResetEditorStateIfEmpty()
        {
            if (HasAnyDialogueGraph())
                return;

            lastSelectedGraph = null;
            lastSyncedSelection = null;
            selectedTransition = null;
            DialogueEditorContext.CurrentGraph = null;
            ClearRenameTarget();
            ClearInspectorTree();
            ClearStaleSelectionInternal();
            graphView?.ClearGraph();
        }

        static bool HasAnyDialogueGraph()
        {
            return AssetDatabase.FindAssets($"t:{nameof(DialogueGraph)}").Length > 0;
        }

        internal void RequestMenuRefresh()
        {
            ForceMenuTreeRebuild();
            ResetSelectionSync();
            ResetEditorStateIfEmpty();
            Repaint();
        }

        internal void ClearInspectorTree()
        {
            inspectorTree = null;
            inspectorTreeTarget = null;
        }

        internal void SetInspectorTreeTarget(UnityEngine.Object target)
        {
            inspectorTreeTarget = target;
        }

        internal void SetInspectorTree(PropertyTree tree)
        {
            inspectorTree = tree;
        }

        internal void SetRenameTarget(UnityEngine.Object target, string name)
        {
            renameTarget = target;
            renameBuffer = name;
        }

        internal void ClearRenameTarget()
        {
            renameTarget = null;
            renameBuffer = string.Empty;
        }

        internal void SetLastSelectedGraph(DialogueGraph graph) => lastSelectedGraph = graph;

        internal void SetSelectedTransition(DialogueTransitionHandle handle) => selectedTransition = handle;

        internal void ClearLastSyncedSelection() => lastSyncedSelection = null;

        internal float GetLeftPanelWidth() => GetPanelWidth(leftPanelContainer, leftPanelWidth, DialogueGraphEditorConstants.MinMenuPanelWidth);
        internal float GetParametersPanelWidth() => GetPanelWidth(parametersPanelContainer, parametersPanelWidth, DialogueGraphEditorConstants.MinParametersPanelWidth);
        internal float GetRightPanelWidth() => GetPanelWidth(rightPanelContainer, rightPanelWidth, DialogueGraphEditorConstants.MinInspectorPanelWidth);

        float GetPanelWidth(IMGUIContainer container, float fallback, float minWidth)
        {
            float width = container?.resolvedStyle.width ?? fallback;
            return float.IsNaN(width) || width <= 0f ? fallback : Mathf.Max(width, minWidth);
        }

        internal void DrawLeftMenu(float panelWidth)
        {
            if (menuTree == null)
                return;

            menuTree.DrawMenuTree();
            menuTree.HandleKeyboardMenuNavigation();
        }

        internal static bool IsAssetAlive(UnityEngine.Object obj) => obj;
    }
}
#endif
