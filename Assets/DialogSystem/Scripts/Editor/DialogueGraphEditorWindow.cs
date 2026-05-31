#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话系统编辑器主窗口
    /// </summary>
    public class DialogueGraphEditorWindow : OdinMenuEditorWindow
    {
        enum ResizeTarget { None, Left, Right }

        const float MenuPanelWidth = 250f;
        const float InspectorPanelWidth = 340f;
        const float MinMenuPanelWidth = 180f;
        const float MinInspectorPanelWidth = 260f;
        const float MinGraphPanelWidth = 260f;
        const float SplitterWidth = 5f;
        const float ToolbarHeight = 22f;
        const double MenuLabelRefreshInterval = 0.5d;
        const string RenameFieldControl = "dialog-asset-rename-field";

        DialogueGraphView graphView;
        VisualElement rowElement;
        VisualElement graphPanel;
        VisualElement leftSplitter;
        VisualElement rightSplitter;
        IMGUIContainer leftPanelContainer;
        IMGUIContainer rightPanelContainer;
        PropertyTree inspectorTree;
        DialogueGraph lastSelectedGraph;
        object lastSyncedSelection;
        UnityEngine.Object inspectorTreeTarget;
        string renameBuffer = "";
        UnityEngine.Object renameTarget;
        Vector2 inspectorScroll;
        float leftPanelWidth = MenuPanelWidth;
        float rightPanelWidth = InspectorPanelWidth;
        float resizeStartX;
        float resizeStartWidth;
        double nextMenuLabelRefreshTime;
        ResizeTarget activeResize = ResizeTarget.None;
        bool uiBuilt;
        bool odinInitialized;
        bool selectionHooked;
        bool renameFieldWasFocused;
        bool graphTitlesRefreshScheduled;
        bool graphSelectionUpdateScheduled;
        object pendingGraphSelection;

        readonly Dictionary<DialogueNode, DialogueGraph> nodeToGraph = new();
        readonly Dictionary<DialogueNode, string> nodeLabelCache = new();

        [MenuItem("Tools/Dialog System")]
        static void Open()
        {
            var window = GetWindow<DialogueGraphEditorWindow>();
            window.titleContent = new GUIContent("Dialog System");
            window.minSize = new Vector2(960, 560);
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            SetupThreePanelUI();
        }

        protected override void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            UnhookSelectionChanged();
            ClearInspectorTree();
            odinInitialized = false;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            uiBuilt = false;
            odinInitialized = false;
            selectionHooked = false;
            if (graphView != null && graphView.parent != null)
                graphView.RemoveFromHierarchy();
            ClearInspectorTree();
            base.OnDestroy();
        }

        [System.Obsolete]
        protected override void OnGUI()
        {
            EnsureOdinInitialized();
            UpdateWindowLayout();
        }

        protected override void OnImGUI() { }

        void OnInspectorUpdate()
        {
            UpdateWindowLayout();
            if (EditorApplication.timeSinceStartup < nextMenuLabelRefreshTime)
                return;

            nextMenuLabelRefreshTime = EditorApplication.timeSinceStartup + MenuLabelRefreshInterval;
            RefreshMenuLabelsIfNeeded();
        }

        void OnProjectChanged() => RequestMenuRefresh();
        void OnUndoRedo() => RequestMenuRefresh();

        void EnsureOdinInitialized()
        {
            if (odinInitialized)
                return;

            Initialize();
            odinInitialized = true;
            TryHookSelectionChanged();

            if (!uiBuilt || rowElement?.parent == null)
                SetupThreePanelUI();
        }

        void RequestMenuRefresh()
        {
            UnhookSelectionChanged();
            ForceMenuTreeRebuild();
            TryHookSelectionChanged();
            ResetSelectionSync();
            Repaint();
        }

        // 构建三栏 UIToolkit 布局
        void SetupThreePanelUI()
        {
            rootVisualElement.Clear();
            uiBuilt = true;

            rootVisualElement.style.position = Position.Relative;
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.flexShrink = 0;
            rootVisualElement.pickingMode = PickingMode.Ignore;

            var toolbar = new IMGUIContainer(DrawToolbar) { name = "dialog-toolbar" };
            toolbar.style.height = ToolbarHeight;
            toolbar.style.flexShrink = 0;
            toolbar.style.flexGrow = 0;
            toolbar.focusable = true;
            toolbar.pickingMode = PickingMode.Position;
            rootVisualElement.Add(toolbar);

            rowElement = new VisualElement { name = "dialog-editor-body" };
            rowElement.style.flexDirection = FlexDirection.Row;
            rowElement.style.flexGrow = 1;
            rowElement.style.flexShrink = 1;
            rowElement.style.flexBasis = 0;
            rowElement.style.alignItems = Align.Stretch;
            rowElement.style.overflow = Overflow.Hidden;
            rowElement.pickingMode = PickingMode.Position;
            rootVisualElement.Add(rowElement);

            leftPanelContainer = CreatePanelContainer("dialog-left-panel", DrawLeftPanel, isLeft: true);
            rowElement.Add(leftPanelContainer);

            leftSplitter = CreateSplitter("dialog-left-splitter", ResizeTarget.Left);
            rowElement.Add(leftSplitter);

            graphPanel = new VisualElement { name = "dialog-graph-panel" };
            graphPanel.style.flexGrow = 1;
            graphPanel.style.flexShrink = 1;
            graphPanel.style.flexBasis = 0;
            graphPanel.style.minWidth = MinGraphPanelWidth;
            graphPanel.style.alignSelf = Align.Stretch;
            graphPanel.style.position = Position.Relative;
            graphPanel.style.overflow = Overflow.Hidden;
            graphPanel.pickingMode = PickingMode.Position;
            graphPanel.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            rowElement.Add(graphPanel);

            graphView = new DialogueGraphView(this);
            graphView.style.position = Position.Absolute;
            graphView.style.left = graphView.style.right = graphView.style.top = graphView.style.bottom = 0;
            graphView.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            graphPanel.Add(graphView);
            graphView.StretchToParentSize();

            rightSplitter = CreateSplitter("dialog-right-splitter", ResizeTarget.Right);
            rowElement.Add(rightSplitter);

            rightPanelContainer = CreatePanelContainer("dialog-right-panel", DrawRightPanel, isLeft: false);
            rowElement.Add(rightPanelContainer);

            UpdateWindowLayout();
            ResetSelectionSync();
            EditorApplication.delayCall += () =>
            {
                TryHookSelectionChanged();
                UpdateWindowLayout();
                SyncSelectionToGraphView();
            };
        }

        IMGUIContainer CreatePanelContainer(string name, System.Action draw, bool isLeft)
        {
            var container = new IMGUIContainer(draw) { name = name };
            container.style.width = isLeft ? leftPanelWidth : rightPanelWidth;
            container.style.flexShrink = 0;
            container.style.flexGrow = 0;
            container.style.alignSelf = Align.Stretch;
            container.focusable = true;
            container.pickingMode = PickingMode.Position;
            StyleSidePanel(container, isLeft);
            return container;
        }

        VisualElement CreateSplitter(string name, ResizeTarget target)
        {
            var splitter = new VisualElement { name = name, userData = target };
            splitter.style.width = SplitterWidth;
            splitter.style.flexShrink = 0;
            splitter.style.flexGrow = 0;
            splitter.style.alignSelf = Align.Stretch;
            splitter.style.backgroundColor = DialogueEditorPanelStyles.SplitterNormal;
            splitter.pickingMode = PickingMode.Position;
            splitter.RegisterCallback<MouseDownEvent>(evt => BeginResize(target, evt, splitter));
            splitter.RegisterCallback<MouseMoveEvent>(evt => Resize(evt, splitter));
            splitter.RegisterCallback<MouseUpEvent>(_ => EndResize(splitter));
            splitter.RegisterCallback<MouseEnterEvent>(_ => splitter.style.backgroundColor = DialogueEditorPanelStyles.SplitterHover);
            splitter.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!splitter.HasMouseCapture())
                    splitter.style.backgroundColor = DialogueEditorPanelStyles.SplitterNormal;
            });
            return splitter;
        }

        static void StyleSidePanel(VisualElement panel, bool isLeft)
        {
            panel.style.backgroundColor = isLeft
                ? DialogueEditorPanelStyles.PanelBackground
                : DialogueEditorPanelStyles.InspectorBackground;

            if (isLeft)
            {
                panel.style.borderRightWidth = 1;
                panel.style.borderRightColor = DialogueEditorPanelStyles.BorderColor;
            }
            else
            {
                panel.style.borderLeftWidth = 1;
                panel.style.borderLeftColor = DialogueEditorPanelStyles.BorderColor;
            }
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

        void OnMenuSelectionChanged(SelectionChangedType _) => RefreshFromMenuSelection();

        // 以左侧树选中项为准同步画布与 Inspector
        void RefreshFromMenuSelection()
        {
            var selectedObject = MenuTree?.Selection?.SelectedValue as UnityEngine.Object;

            if (!ReferenceEquals(inspectorTreeTarget, selectedObject))
                ClearInspectorTree();

            if (selectedObject != null)
            {
                if (!ReferenceEquals(renameTarget, selectedObject))
                {
                    renameTarget = selectedObject;
                    renameBuffer = selectedObject.name;
                }
            }
            else
            {
                renameTarget = null;
                renameBuffer = string.Empty;
            }

            SyncSelectionToGraphView();
            Repaint();
        }

        void UpdateWindowLayout()
        {
            if (!uiBuilt || rowElement == null || graphView == null)
                return;

            float rowHeight = GetRowHeight();
            rootVisualElement.style.width = position.width;
            rootVisualElement.style.height = position.height;
            rowElement.style.height = rowElement.style.minHeight = rowHeight;
            leftPanelContainer.style.height = rowHeight;
            rightPanelContainer.style.height = rowHeight;
            graphPanel.style.height = rowHeight;
            leftSplitter.style.height = rowHeight;
            rightSplitter.style.height = rowHeight;
            ApplyPanelWidths(position.width);
        }

        void ApplyPanelWidths(float windowWidth)
        {
            float availableWidth = Mathf.Max(1f, windowWidth - SplitterWidth * 2f);
            float minGraphWidth = Mathf.Min(MinGraphPanelWidth, Mathf.Max(140f, availableWidth * 0.35f));
            float minLeftWidth = Mathf.Min(MinMenuPanelWidth, availableWidth * 0.28f);
            float minRightWidth = Mathf.Min(MinInspectorPanelWidth, availableWidth * 0.32f);

            leftPanelWidth = Mathf.Max(leftPanelWidth, minLeftWidth);
            rightPanelWidth = Mathf.Max(rightPanelWidth, minRightWidth);

            float overflow = leftPanelWidth + rightPanelWidth + minGraphWidth - availableWidth;
            if (overflow > 0f)
            {
                rightPanelWidth -= Mathf.Min(overflow, rightPanelWidth - minRightWidth);
                overflow = leftPanelWidth + rightPanelWidth + minGraphWidth - availableWidth;
            }

            if (overflow > 0f)
                leftPanelWidth -= Mathf.Min(overflow, leftPanelWidth - minLeftWidth);

            leftPanelWidth = Mathf.Clamp(leftPanelWidth, minLeftWidth, Mathf.Max(minLeftWidth, availableWidth - rightPanelWidth - minGraphWidth));
            rightPanelWidth = Mathf.Clamp(rightPanelWidth, minRightWidth, Mathf.Max(minRightWidth, availableWidth - leftPanelWidth - minGraphWidth));

            leftPanelContainer.style.width = leftPanelWidth;
            rightPanelContainer.style.width = rightPanelWidth;
        }

        void BeginResize(ResizeTarget target, MouseDownEvent evt, VisualElement splitter)
        {
            if (evt.button != 0)
                return;

            activeResize = target;
            resizeStartX = evt.mousePosition.x;
            resizeStartWidth = target == ResizeTarget.Left ? leftPanelWidth : rightPanelWidth;
            splitter.CaptureMouse();
            evt.StopPropagation();
        }

        void Resize(MouseMoveEvent evt, VisualElement splitter)
        {
            if (activeResize == ResizeTarget.None || !splitter.HasMouseCapture())
                return;

            float delta = evt.mousePosition.x - resizeStartX;
            if (activeResize == ResizeTarget.Left)
                leftPanelWidth = resizeStartWidth + delta;
            else
                rightPanelWidth = resizeStartWidth - delta;

            ApplyPanelWidths(position.width);
            evt.StopPropagation();
        }

        void EndResize(VisualElement splitter)
        {
            if (activeResize == ResizeTarget.None)
                return;

            activeResize = ResizeTarget.None;
            splitter.ReleaseMouse();
        }

        void EnsureMenuTree()
        {
            if (MenuTree != null)
                return;

            ForceMenuTreeRebuild();
            UnhookSelectionChanged();
            TryHookSelectionChanged();
        }

        void DrawLeftPanel()
        {
            float panelWidth = GetLeftPanelWidth();
            EditorGUILayout.BeginVertical(GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
            EnsureMenuTree();
            MenuWidth = panelWidth;
            DrawMenu();
            EditorGUILayout.EndVertical();
        }

        void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(GetRightPanelWidth()), GUILayout.ExpandHeight(true));
            EnsureMenuTree();

            var selected = MenuTree?.Selection?.SelectedValue as UnityEngine.Object;
            DialogueEditorPanelStyles.DrawPanelHeader("属性", selected != null ? GetSelectionSubtitle(selected) : "未选中");

            if (selected == null)
            {
                DialogueEditorPanelStyles.BeginPaddedContent();
                DialogueEditorPanelStyles.DrawEmptyHint("在左侧选中 Graph 或节点以编辑属性。");
                DialogueEditorPanelStyles.EndPaddedContent();
                EditorGUILayout.EndVertical();
                return;
            }

            DialogueEditorPanelStyles.BeginPaddedContent();
            DrawSelectedObjectInspector(selected);
            DialogueEditorPanelStyles.EndPaddedContent();
            EditorGUILayout.EndVertical();
        }

        static string GetSelectionSubtitle(UnityEngine.Object selected)
        {
            if (selected is DialogueGraph)
                return "Dialogue Graph";
            if (selected is DialogueNode node)
                return $"Dialogue Node  ·  [{node.NodeId}] {node.SpeakerName}";
            return selected.name;
        }

        float GetRowHeight() => Mathf.Max(position.height - ToolbarHeight, 100f);

        float GetPanelWidth(IMGUIContainer container, float fallback, float minWidth)
        {
            float width = container?.resolvedStyle.width ?? fallback;
            return float.IsNaN(width) || width <= 0f ? fallback : Mathf.Max(width, minWidth);
        }

        float GetLeftPanelWidth() => GetPanelWidth(leftPanelContainer, leftPanelWidth, MinMenuPanelWidth);
        float GetRightPanelWidth() => GetPanelWidth(rightPanelContainer, rightPanelWidth, MinInspectorPanelWidth);

        void DrawSelectedObjectInspector(UnityEngine.Object selected)
        {
            if (inspectorTreeTarget != selected || inspectorTree == null)
            {
                inspectorTreeTarget = selected;
                inspectorTree = PropertyTree.Create(new SerializedObject(selected));
                inspectorScroll = Vector2.zero;
            }

            if (renameTarget != selected)
            {
                renameTarget = selected;
                renameBuffer = selected.name;
            }

            DrawAssetRenameField(selected);
            EditorGUILayout.Space(4);

            inspectorScroll = EditorGUILayout.BeginScrollView(inspectorScroll);
            EditorGUI.BeginChangeCheck();
            inspectorTree.Draw(false);
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndScrollView();

            if (!changed)
                return;

            inspectorTree.ApplyChanges();
            EditorUtility.SetDirty(selected);
            QueueGraphViewRefreshFromInspector(selected);
            RequestMenuLabelRefreshOnly();
        }

        void ClearInspectorTree()
        {
            inspectorTree = null;
            inspectorTreeTarget = null;
        }

        void QueueGraphViewRefreshFromInspector(UnityEngine.Object selected)
        {
            if (graphView == null)
                return;

            graphView.schedule.Execute(() =>
            {
                graphView.RefreshCurrentGraph(preserveView: true);
                if (selected is DialogueNode node)
                    graphView.SelectNode(node);
            });
        }

        void DrawAssetRenameField(UnityEngine.Object selected)
        {
            GUI.SetNextControlName(RenameFieldControl);
            renameBuffer = EditorGUILayout.TextField("名称", renameBuffer);

            var evt = Event.current;
            if (evt.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == RenameFieldControl)
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    CommitAssetRename(selected);
                    GUI.FocusControl(null);
                    evt.Use();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    renameBuffer = selected.name;
                    GUI.FocusControl(null);
                    evt.Use();
                }
            }

            if (evt.type == EventType.Repaint)
            {
                bool focused = GUI.GetNameOfFocusedControl() == RenameFieldControl;
                if (renameFieldWasFocused && !focused)
                    CommitAssetRename(selected);
                renameFieldWasFocused = focused;
            }
        }

        void CommitAssetRename(UnityEngine.Object selected)
        {
            if (selected == null || renameBuffer == selected.name)
                return;

            if (ApplyAssetRename(selected, renameBuffer, ref renameBuffer))
                RefreshAfterExternalChange(selected);
        }

        void RefreshAfterExternalChange(UnityEngine.Object target)
        {
            if (target == null)
                return;

            renameTarget = target;
            renameBuffer = target.name;
            ClearInspectorTree();
            RequestMenuRefresh();
            EditorApplication.delayCall += () => SelectObjectInTree(target);
        }

        void RequestMenuLabelRefreshOnly()
        {
            RefreshMenuLabelsIfNeeded();
            QueueGraphViewRefreshTitles();
            Repaint();
        }

        static bool ApplyAssetRename(UnityEngine.Object asset, string newName, ref string renameBuffer)
        {
            if (asset == null || string.IsNullOrWhiteSpace(newName) || newName == asset.name)
                return false;

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return false;

            string error = AssetDatabase.RenameAsset(path, newName.Trim());
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(error);
                renameBuffer = asset.name;
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }

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

        protected override OdinMenuTree BuildMenuTree()
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
                    tree.Add($"{basePath}/{SanitizeMenuPath(label)}", node);
                }
            }

            tree.EnumerateTree().AddThumbnailIcons(true);
            return tree;
        }

        static string BuildNodeLabel(DialogueNode node) =>
            $"[{node.NodeId}] {node.SpeakerName}: {Truncate(node.DialogText, 12)}";

        DialogueGraph GetActiveGraph()
        {
            var selected = MenuTree?.Selection?.SelectedValue;
            return selected as DialogueGraph ?? lastSelectedGraph;
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("新建对话图", EditorStyles.toolbarButton, GUILayout.Width(80)))
                CreateNewDialogueGraph();

            var targetGraph = GetActiveGraph();
            if (targetGraph != null)
            {
                if (GUILayout.Button("校验本图", EditorStyles.toolbarButton, GUILayout.Width(72)))
                    DialogueGraphValidator.Validate(targetGraph);

                if (GUILayout.Button("导出 JSON", EditorStyles.toolbarButton, GUILayout.Width(72)))
                    DialogueGraphJsonIO.ExportWithSaveDialog(targetGraph);
            }

            if (GUILayout.Button("导入 JSON", EditorStyles.toolbarButton, GUILayout.Width(72))
                && DialogueGraphJsonIO.ImportWithOpenDialog(targetGraph, out var importedGraph))
            {
                RefreshAfterExternalChange(importedGraph);
            }

            if (Application.isPlaying && lastSelectedGraph != null)
            {
                if (GUILayout.Button("播放当前图", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    var runner = FindObjectOfType<DialogueRunner>();
                    if (runner != null)
                        runner.PlayGraph(lastSelectedGraph);
                    else
                        Debug.LogWarning("场景中未找到 DialogueRunner");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void SyncSelectionToGraphView()
        {
            if (graphView == null || MenuTree?.Selection == null)
                return;

            var selected = MenuTree.Selection.SelectedValue;
            if (ReferenceEquals(selected, lastSyncedSelection))
                return;

            lastSyncedSelection = selected;

            if (selected is DialogueGraph graph)
            {
                lastSelectedGraph = graph;
                DialogueEditorContext.CurrentGraph = graph;
            }
            else if (selected is DialogueNode node)
            {
                lastSelectedGraph = FindGraphForNode(node);
                DialogueEditorContext.CurrentGraph = lastSelectedGraph;
            }

            QueueGraphViewSelectionUpdate(selected);
        }

        void QueueGraphViewSelectionUpdate(object selected)
        {
            pendingGraphSelection = selected;
            if (graphView == null || graphSelectionUpdateScheduled)
                return;

            graphSelectionUpdateScheduled = true;
            graphView.schedule.Execute(() =>
            {
                graphSelectionUpdateScheduled = false;
                graphView?.ApplySelection(pendingGraphSelection);
            });
        }

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

        // 画布点击后反向同步到左侧树
        public void SelectObjectInTree(UnityEngine.Object target)
        {
            if (MenuTree == null || target == null)
                return;

            foreach (var item in MenuTree.EnumerateTree())
            {
                if (!ReferenceEquals(item.Value, target))
                    continue;

                if (!ReferenceEquals(MenuTree.Selection.SelectedValue, target))
                {
                    item.Select();
                    MenuTree.Selection.Clear();
                    MenuTree.Selection.Add(item);
                }

                RefreshFromMenuSelection();
                return;
            }
        }

        public void ResetSelectionSync() => lastSyncedSelection = null;

        void CreateNewDialogueGraph()
        {
            DialogueEditorPaths.EnsureGraphAssetFolder();

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DialogueEditorPaths.GraphAssetPath}/New Dialogue Graph.asset");
            var graph = CreateInstance<DialogueGraph>();
            AssetDatabase.CreateAsset(graph, assetPath);

            string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var so = new SerializedObject(graph);
            so.FindProperty("graphId").intValue = GetNextGraphId();
            so.FindProperty("graphName").stringValue = assetName;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            Selection.activeObject = graph;
            RefreshAfterExternalChange(graph);
        }

        static int GetNextGraphId()
        {
            int maxId = 0;
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(DialogueGraph)}"))
            {
                var graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(AssetDatabase.GUIDToAssetPath(guid));
                if (graph != null && graph.GraphId > maxId)
                    maxId = graph.GraphId;
            }

            return maxId + 1;
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

        static string SanitizeMenuPath(string path) => path.Replace("/", "／");

        static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";
            return text.Length <= max ? text : text.Substring(0, max) + "…";
        }
    }
}
#endif
