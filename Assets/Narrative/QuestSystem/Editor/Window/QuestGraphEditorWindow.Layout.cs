#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.DialogSystem.Quest.Editor
{
    partial class QuestGraphEditorWindow
    {
        GraphViewEditorLayout editorLayout;

        void SetupUI()
        {
            uiBuilt = true;
            editorLayout = new GraphViewEditorLayout(this, "quest-editor-body");
            editorLayout.ConfigureGraphPanel(
                QuestGraphEditorConstants.MinGraphPanelWidth,
                GraphViewFrameConstants.GraphMinWidthRatioQuest);
            editorLayout.BuildToolbar(DrawToolbar, "quest-toolbar");

            editorLayout.AddLeftColumn(
                "quest-left-panel",
                DrawLeftPanel,
                QuestGraphEditorConstants.MenuPanelWidth,
                QuestGraphEditorConstants.MinMenuPanelWidth,
                GraphViewFrameConstants.MenuMinWidthRatio);

            editorLayout.AddGraphPanel();

            graphView = new QuestGraphView();
            graphView.OnNodeSelected = node =>
            {
                selectedNode = node;
                Repaint();
            };
            editorLayout.MountGraphView(graphView);

            editorLayout.AddRightColumn(
                "quest-right-panel",
                () => QuestGraphInspectorPanel.Draw(this),
                QuestGraphEditorConstants.InspectorPanelWidth,
                QuestGraphEditorConstants.MinInspectorPanelWidth,
                GraphViewFrameConstants.InspectorMinWidthRatio);

            rowElement = editorLayout.Row;
            graphPanel = editorLayout.GraphPanel;

            UpdateWindowLayout();
            SyncGraphSelection();
        }

        void UpdateWindowLayout() => editorLayout?.UpdateLayout();

        void ReleaseAllMouseCaptures() => editorLayout?.ReleaseAllCaptures();

        float GetPanelWidth(IMGUIContainer container, float fallback, float minWidth) =>
            GraphViewEditorLayout.ResolvePanelWidth(container, fallback, minWidth);
    }
}
#endif
