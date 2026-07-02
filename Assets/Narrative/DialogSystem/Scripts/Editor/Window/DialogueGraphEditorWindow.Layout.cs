#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        GraphViewEditorLayout editorLayout;

        void SetupThreePanelUI()
        {
            uiBuilt = true;
            editorLayout = new GraphViewEditorLayout(this, "dialog-editor-body");
            editorLayout.ConfigureGraphPanel(
                DialogueGraphEditorConstants.MinGraphPanelWidth,
                GraphViewFrameConstants.GraphMinWidthRatioDialog);
            editorLayout.BuildToolbar(DrawToolbar, "dialog-toolbar");

            editorLayout.AddLeftColumn(
                "dialog-left-panel",
                () => DialogueGraphLeftPanel.Draw(this),
                DialogueGraphEditorConstants.MenuPanelWidth,
                DialogueGraphEditorConstants.MinMenuPanelWidth,
                GraphViewFrameConstants.MenuMinWidthRatio);

            editorLayout.AddLeftColumn(
                "dialog-variables-panel",
                () => DialogueGraphVariablesPanel.Draw(this),
                DialogueGraphEditorConstants.VariablesPanelWidth,
                DialogueGraphEditorConstants.MinVariablesPanelWidth,
                GraphViewFrameConstants.VariablesMinWidthRatio,
                panel => panel.style.borderRightWidth = 0);

            editorLayout.AddGraphPanel();

            graphView = new DialogueGraphView(this);
            editorLayout.MountGraphView(graphView);

            editorLayout.AddRightColumn(
                "dialog-right-panel",
                () => DialogueGraphInspectorPanel.Draw(this),
                DialogueGraphEditorConstants.InspectorPanelWidth,
                DialogueGraphEditorConstants.MinInspectorPanelWidth,
                GraphViewFrameConstants.InspectorMinWidthRatio);

            rowElement = editorLayout.Row;
            graphPanel = editorLayout.GraphPanel;
            leftPanelContainer = editorLayout.LeftColumns[0].Container;
            variablesPanelContainer = editorLayout.LeftColumns[1].Container;
            rightPanelContainer = editorLayout.RightColumn.Container;

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

        void UpdateWindowLayout() => editorLayout?.UpdateLayout();

        void ReleaseAllMouseCaptures() => editorLayout?.ReleaseAllCaptures();
    }
}
#endif
