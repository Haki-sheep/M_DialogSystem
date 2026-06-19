#if UNITY_EDITOR
namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 编辑器窗口布局常量
    /// </summary>
    static class DialogueGraphEditorConstants
    {
        public const float MenuPanelWidth = 250f;
        public const float ParametersPanelWidth = 220f;
        public const float MinParametersPanelWidth = 160f;
        public const float InspectorPanelWidth = 340f;
        public const float MinMenuPanelWidth = 180f;
        public const float MinInspectorPanelWidth = 260f;
        public const float MinGraphPanelWidth = 260f;
        public const float SplitterWidth = 5f;
        public const float ToolbarHeight = 22f;
        public const double MenuLabelRefreshInterval = 0.5d;
        public const int MenuNodeDialogTextMaxLength = 16;
        public const string RenameFieldControl = "dialog-asset-rename-field";
    }
}
#endif
