#if UNITY_EDITOR
namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 叙事 Graph 编辑器公共布局常量
    /// </summary>
    public static class GraphViewFrameConstants
    {
        public const float SplitterWidth = 5f;
        public const float ToolbarHeight = 22f;
        public const float MinGraphPanelWidth = 260f;
        public const float MinGraphPanelWidthFloor = 140f;
        public const float MinRowHeight = 100f;

        public const float DefaultMenuPanelWidth = 250f;
        public const float DefaultVariablesPanelWidth = 220f;
        public const float DefaultInspectorPanelWidth = 340f;

        public const float MinMenuPanelWidth = 180f;
        public const float MinVariablesPanelWidth = 160f;
        public const float MinInspectorPanelWidth = 260f;

        public const float MenuMinWidthRatio = 0.28f;
        public const float VariablesMinWidthRatio = 0.18f;
        public const float InspectorMinWidthRatio = 0.32f;
        public const float GraphMinWidthRatioDialog = 0.35f;
        public const float GraphMinWidthRatioQuest = 0.4f;
    }
}
#endif
