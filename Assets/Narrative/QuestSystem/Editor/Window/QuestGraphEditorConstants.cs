#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务编辑器窗口布局常量
    /// </summary>
    static class QuestGraphEditorConstants
    {
        public const float MenuPanelWidth = GraphViewFrameConstants.DefaultMenuPanelWidth;
        public const float InspectorPanelWidth = GraphViewFrameConstants.DefaultInspectorPanelWidth;
        public const float MinMenuPanelWidth = GraphViewFrameConstants.MinMenuPanelWidth;
        public const float MinInspectorPanelWidth = GraphViewFrameConstants.MinInspectorPanelWidth;
        public const float MinGraphPanelWidth = 240f;
    }
}
#endif
