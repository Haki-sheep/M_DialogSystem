#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 叙事 Graph 编辑器三栏布局构建器
    /// 左栏可多个 中间画布 右栏 Inspector
    /// </summary>
    public sealed class GraphViewEditorLayout
    {
        public sealed class ColumnBinding
        {
            public string Name { get; internal set; }
            public IMGUIContainer Container { get; internal set; }
            public float Width { get; set; }
            public float MinWidth { get; internal set; }
            public float MinWidthRatio { get; internal set; }
            public bool IsInspector { get; internal set; }
        }

        readonly EditorWindow window;
        readonly VisualElement root;
        readonly List<ColumnBinding> leftColumns = new();
        readonly List<PanelSplitter> splitters = new();
        readonly List<VisualElement> rowHeightTargets = new();

        VisualElement row;
        VisualElement graphPanel;
        ColumnBinding rightColumn;
        PanelSplitter rightSplitter;
        NarrativeGraphViewBase mountedGraphView;

        float minGraphWidth = GraphViewFrameConstants.MinGraphPanelWidth;
        float minGraphWidthRatio = GraphViewFrameConstants.GraphMinWidthRatioDialog;

        public VisualElement Row => row;
        public VisualElement GraphPanel => graphPanel;
        public IReadOnlyList<ColumnBinding> LeftColumns => leftColumns;
        public ColumnBinding RightColumn => rightColumn;

        public GraphViewEditorLayout(EditorWindow window, string rowElementName)
        {
            this.window = window;
            root = window.rootVisualElement;
            row = new VisualElement { name = rowElementName };
        }

        /// <summary>
        /// 初始化根布局与工具栏
        /// </summary>
        public void BuildToolbar(Action drawToolbar, string toolbarName = "graph-frame-toolbar")
        {
            root.Clear();
            root.style.position = Position.Relative;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            root.style.flexShrink = 0;
            root.pickingMode = PickingMode.Position;

            var toolbar = new IMGUIContainer(drawToolbar) { name = toolbarName };
            toolbar.style.height = GraphViewFrameConstants.ToolbarHeight;
            toolbar.style.flexShrink = 0;
            toolbar.style.flexGrow = 0;
            toolbar.focusable = true;
            toolbar.pickingMode = PickingMode.Position;
            root.Add(toolbar);

            row.style.flexDirection = FlexDirection.Row;
            row.style.flexGrow = 1;
            row.style.flexShrink = 1;
            row.style.flexBasis = 0;
            row.style.alignItems = Align.Stretch;
            row.style.overflow = Overflow.Hidden;
            row.pickingMode = PickingMode.Position;
            root.Add(row);
        }

        /// <summary>
        /// 配置画布最小宽度策略
        /// </summary>
        public void ConfigureGraphPanel(float minWidth, float minWidthRatio)
        {
            minGraphWidth = minWidth;
            minGraphWidthRatio = minWidthRatio;
        }

        /// <summary>
        /// 在画布左侧追加一栏
        /// </summary>
        public ColumnBinding AddLeftColumn(
            string name,
            Action draw,
            float defaultWidth,
            float minWidth,
            float minWidthRatio = 0f,
            Action<VisualElement> extraStyle = null)
        {
            var column = new ColumnBinding
            {
                Name = name,
                Width = defaultWidth,
                MinWidth = minWidth,
                MinWidthRatio = minWidthRatio,
                IsInspector = false,
            };

            column.Container = SidePanelFactory.Create(name, draw, defaultWidth, isInspector: false, extraStyle);
            row.Add(column.Container);
            rowHeightTargets.Add(column.Container);

            var splitter = new PanelSplitter($"{name}-splitter", delta => column.Width += delta);
            splitters.Add(splitter);
            row.Add(splitter.Element);
            rowHeightTargets.Add(splitter.Element);

            leftColumns.Add(column);
            return column;
        }

        /// <summary>
        /// 添加中间 Graph 容器
        /// </summary>
        public VisualElement AddGraphPanel()
        {
            graphPanel = new VisualElement { name = "graph-frame-graph-panel" };
            graphPanel.style.flexGrow = 1;
            graphPanel.style.flexShrink = 1;
            graphPanel.style.flexBasis = 0;
            graphPanel.style.minWidth = minGraphWidth;
            graphPanel.style.alignSelf = Align.Stretch;
            graphPanel.style.position = Position.Relative;
            graphPanel.style.overflow = Overflow.Hidden;
            graphPanel.pickingMode = PickingMode.Position;
            graphPanel.style.backgroundColor = GraphViewFramePanelStyles.GraphPanelBackground;
            row.Add(graphPanel);
            rowHeightTargets.Add(graphPanel);
            return graphPanel;
        }

        /// <summary>
        /// 挂载 GraphView 到画布容器
        /// </summary>
        public void MountGraphView(NarrativeGraphViewBase graphView)
        {
            mountedGraphView = graphView;
            graphView.style.position = Position.Absolute;
            graphView.style.left = graphView.style.right = graphView.style.top = graphView.style.bottom = 0;
            graphPanel.Add(graphView);
            graphView.StretchToParentSize();
        }

        /// <summary>
        /// 追加右侧 Inspector 栏
        /// </summary>
        public ColumnBinding AddRightColumn(
            string name,
            Action draw,
            float defaultWidth,
            float minWidth,
            float minWidthRatio = 0f)
        {
            rightSplitter = new PanelSplitter($"{name}-splitter", delta =>
            {
                if (rightColumn != null)
                    rightColumn.Width -= delta;
            });
            splitters.Add(rightSplitter);
            row.Add(rightSplitter.Element);
            rowHeightTargets.Add(rightSplitter.Element);

            rightColumn = new ColumnBinding
            {
                Name = name,
                Width = defaultWidth,
                MinWidth = minWidth,
                MinWidthRatio = minWidthRatio,
                IsInspector = true,
            };
            rightColumn.Container = SidePanelFactory.Create(name, draw, defaultWidth, isInspector: true);
            row.Add(rightColumn.Container);
            rowHeightTargets.Add(rightColumn.Container);
            return rightColumn;
        }

        /// <summary>
        /// 窗口尺寸变化时刷新布局
        /// </summary>
        public void UpdateLayout()
        {
            if (row == null)
                return;

            float rowHeight = Mathf.Max(
                window.position.height - GraphViewFrameConstants.ToolbarHeight,
                GraphViewFrameConstants.MinRowHeight);

            root.style.width = window.position.width;
            root.style.height = window.position.height;
            row.style.height = row.style.minHeight = rowHeight;

            foreach (var target in rowHeightTargets)
                target.style.height = rowHeight;

            ApplyPanelWidths(window.position.width);
        }

        /// <summary>
        /// 释放分栏与画布鼠标捕获
        /// </summary>
        public void ReleaseAllCaptures()
        {
            foreach (var splitter in splitters)
                splitter.ReleaseCapture();

            mountedGraphView?.ReleaseInteractionCapture();
        }

        /// <summary>
        /// 解析侧栏实际渲染宽度
        /// </summary>
        public static float ResolvePanelWidth(IMGUIContainer container, float fallback, float minWidth)
        {
            float width = container?.resolvedStyle.width ?? fallback;
            return float.IsNaN(width) || width <= 0f ? fallback : Mathf.Max(width, minWidth);
        }

        /// <summary>
        /// 解析列绑定宽度
        /// </summary>
        public float ResolveColumnWidth(ColumnBinding column)
        {
            if (column?.Container == null)
                return column?.Width ?? 0f;

            return ResolvePanelWidth(column.Container, column.Width, column.MinWidth);
        }

        void ApplyPanelWidths(float windowWidth)
        {
            float splitterTotal = GraphViewFrameConstants.SplitterWidth * splitters.Count;
            float availableWidth = Mathf.Max(1f, windowWidth - splitterTotal);

            int panelCount = leftColumns.Count + (rightColumn != null ? 1 : 0);
            if (panelCount == 0)
                return;

            var states = new PanelWidthSolver.PanelWidthState[panelCount];
            int index = 0;

            for (int i = 0; i < leftColumns.Count; i++)
            {
                var column = leftColumns[i];
                states[index++] = new PanelWidthSolver.PanelWidthState
                {
                    Width = column.Width,
                    MinWidth = ResolveEffectiveMinWidth(column, availableWidth),
                };
            }

            if (rightColumn != null)
            {
                states[index] = new PanelWidthSolver.PanelWidthState
                {
                    Width = rightColumn.Width,
                    MinWidth = ResolveEffectiveMinWidth(rightColumn, availableWidth),
                };
            }

            PanelWidthSolver.Solve(availableWidth, minGraphWidth, minGraphWidthRatio, states);

            index = 0;
            for (int i = 0; i < leftColumns.Count; i++)
            {
                leftColumns[i].Width = states[index].Width;
                leftColumns[i].Container.style.width = states[index].Width;
                index++;
            }

            if (rightColumn != null)
            {
                rightColumn.Width = states[index].Width;
                rightColumn.Container.style.width = states[index].Width;
            }
        }

        static float ResolveEffectiveMinWidth(ColumnBinding column, float availableWidth)
        {
            if (column.MinWidthRatio <= 0f)
                return column.MinWidth;

            return Mathf.Min(column.MinWidth, availableWidth * column.MinWidthRatio);
        }
    }
}
#endif
