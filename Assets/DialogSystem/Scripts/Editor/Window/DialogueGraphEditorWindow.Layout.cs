#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        void SetupThreePanelUI()
        {
            rootVisualElement.Clear();
            uiBuilt = true;

            rootVisualElement.style.position = Position.Relative;
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.flexShrink = 0;
            rootVisualElement.pickingMode = PickingMode.Position;

            var toolbar = new IMGUIContainer(DrawToolbar) { name = "dialog-toolbar" };
            toolbar.style.height = DialogueGraphEditorConstants.ToolbarHeight;
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

            leftPanelContainer = CreatePanelContainer("dialog-left-panel", DialogueGraphLeftPanel.Draw, isLeft: true);
            rowElement.Add(leftPanelContainer);

            leftSplitter = CreateSplitter("dialog-left-splitter", ResizeTarget.Left);
            rowElement.Add(leftSplitter);

            variablesPanelContainer = CreatePanelContainer("dialog-variables-panel", DialogueGraphVariablesPanel.Draw, isLeft: true);
            variablesPanelContainer.style.borderRightWidth = 0;
            rowElement.Add(variablesPanelContainer);

            variablesSplitter = CreateSplitter("dialog-variables-splitter", ResizeTarget.Variables);
            rowElement.Add(variablesSplitter);

            graphPanel = new VisualElement { name = "dialog-graph-panel" };
            graphPanel.style.flexGrow = 1;
            graphPanel.style.flexShrink = 1;
            graphPanel.style.flexBasis = 0;
            graphPanel.style.minWidth = DialogueGraphEditorConstants.MinGraphPanelWidth;
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

            rightPanelContainer = CreatePanelContainer("dialog-right-panel", DialogueGraphInspectorPanel.Draw, isLeft: false);
            rowElement.Add(rightPanelContainer);

            UpdateWindowLayout();
            ResetSelectionSync();
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null)
                    return;

                TryHookSelectionChanged();
                UpdateWindowLayout();
                ResetEditorStateIfEmpty();
                SyncSelectionToGraphView();
            };
        }

        IMGUIContainer CreatePanelContainer(string name, System.Action<DialogueGraphEditorWindow> draw, bool isLeft)
        {
            var container = new IMGUIContainer(() => draw(this)) { name = name };
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
            splitter.style.width = DialogueGraphEditorConstants.SplitterWidth;
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

        void UpdateWindowLayout()
        {
            if (!uiBuilt || rowElement == null || graphView == null)
                return;

            float rowHeight = GetRowHeight();
            rootVisualElement.style.width = position.width;
            rootVisualElement.style.height = position.height;
            rowElement.style.height = rowElement.style.minHeight = rowHeight;
            leftPanelContainer.style.height = rowHeight;
            variablesPanelContainer.style.height = rowHeight;
            rightPanelContainer.style.height = rowHeight;
            graphPanel.style.height = rowHeight;
            leftSplitter.style.height = rowHeight;
            variablesSplitter.style.height = rowHeight;
            rightSplitter.style.height = rowHeight;
            ApplyPanelWidths(position.width);
        }

        void ApplyPanelWidths(float windowWidth)
        {
            float splitterTotal = DialogueGraphEditorConstants.SplitterWidth * 3f;
            float availableWidth = Mathf.Max(1f, windowWidth - splitterTotal);
            float minGraphWidth = Mathf.Min(DialogueGraphEditorConstants.MinGraphPanelWidth, Mathf.Max(140f, availableWidth * 0.35f));
            float minLeftWidth = Mathf.Min(DialogueGraphEditorConstants.MinMenuPanelWidth, availableWidth * 0.28f);
            float minVariablesWidth = Mathf.Min(DialogueGraphEditorConstants.MinVariablesPanelWidth, availableWidth * 0.18f);
            float minRightWidth = Mathf.Min(DialogueGraphEditorConstants.MinInspectorPanelWidth, availableWidth * 0.32f);

            leftPanelWidth = Mathf.Max(leftPanelWidth, minLeftWidth);
            variablesPanelWidth = Mathf.Max(variablesPanelWidth, minVariablesWidth);
            rightPanelWidth = Mathf.Max(rightPanelWidth, minRightWidth);

            float overflow = leftPanelWidth + variablesPanelWidth + rightPanelWidth + minGraphWidth - availableWidth;
            if (overflow > 0f)
            {
                rightPanelWidth -= Mathf.Min(overflow, rightPanelWidth - minRightWidth);
                overflow = leftPanelWidth + variablesPanelWidth + rightPanelWidth + minGraphWidth - availableWidth;
            }

            if (overflow > 0f)
            {
                variablesPanelWidth -= Mathf.Min(overflow, variablesPanelWidth - minVariablesWidth);
                overflow = leftPanelWidth + variablesPanelWidth + rightPanelWidth + minGraphWidth - availableWidth;
            }

            if (overflow > 0f)
                leftPanelWidth -= Mathf.Min(overflow, leftPanelWidth - minLeftWidth);

            leftPanelWidth = Mathf.Clamp(leftPanelWidth, minLeftWidth, availableWidth);
            variablesPanelWidth = Mathf.Clamp(variablesPanelWidth, minVariablesWidth, availableWidth);
            rightPanelWidth = Mathf.Clamp(rightPanelWidth, minRightWidth, availableWidth);

            leftPanelContainer.style.width = leftPanelWidth;
            variablesPanelContainer.style.width = variablesPanelWidth;
            rightPanelContainer.style.width = rightPanelWidth;
        }

        void BeginResize(ResizeTarget target, MouseDownEvent evt, VisualElement splitter)
        {
            if (evt.button != 0)
                return;

            activeResize = target;
            resizeStartX = evt.mousePosition.x;
            resizeStartWidth = target switch
            {
                ResizeTarget.Left => leftPanelWidth,
                ResizeTarget.Variables => variablesPanelWidth,
                _ => rightPanelWidth,
            };
            splitter.CaptureMouse();
            evt.StopPropagation();
        }

        void Resize(MouseMoveEvent evt, VisualElement splitter)
        {
            if (activeResize == ResizeTarget.None || !splitter.HasMouseCapture())
                return;

            float delta = evt.mousePosition.x - resizeStartX;
            switch (activeResize)
            {
                case ResizeTarget.Left:
                    leftPanelWidth = resizeStartWidth + delta;
                    break;
                case ResizeTarget.Variables:
                    variablesPanelWidth = resizeStartWidth + delta;
                    break;
                default:
                    rightPanelWidth = resizeStartWidth - delta;
                    break;
            }

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

        void ReleaseAllMouseCaptures()
        {
            activeResize = ResizeTarget.None;
            leftSplitter?.ReleaseMouse();
            variablesSplitter?.ReleaseMouse();
            rightSplitter?.ReleaseMouse();
            graphView?.ReleaseInteractionCapture();
        }

        float GetRowHeight() => Mathf.Max(position.height - DialogueGraphEditorConstants.ToolbarHeight, 100f);
    }
}
#endif
