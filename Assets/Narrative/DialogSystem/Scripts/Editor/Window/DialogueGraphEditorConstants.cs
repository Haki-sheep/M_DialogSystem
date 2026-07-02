#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话编辑器窗口布局常量
    /// </summary>
    static class DialogueGraphEditorConstants
    {
        public const float MenuPanelWidth = GraphViewFrameConstants.DefaultMenuPanelWidth;
        public const float VariablesPanelWidth = GraphViewFrameConstants.DefaultVariablesPanelWidth;
        public const float MinVariablesPanelWidth = GraphViewFrameConstants.MinVariablesPanelWidth;
        public const float InspectorPanelWidth = GraphViewFrameConstants.DefaultInspectorPanelWidth;
        public const float MinMenuPanelWidth = GraphViewFrameConstants.MinMenuPanelWidth;
        public const float MinInspectorPanelWidth = GraphViewFrameConstants.MinInspectorPanelWidth;
        public const float MinGraphPanelWidth = GraphViewFrameConstants.MinGraphPanelWidth;

        public const double MenuLabelRefreshInterval = 0.5d;
        public const int MenuNodeDialogTextMaxLength = 16;
        public const string RenameFieldControl = "dialog-asset-rename-field";
    }
}
#endif
