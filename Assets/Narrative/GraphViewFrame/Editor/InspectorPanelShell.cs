#if UNITY_EDITOR
using System;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 右侧 Inspector 通用外壳
    /// </summary>
    public static class InspectorPanelShell
    {
        /// <summary>
        /// 绘制带空态的 Inspector 面板
        /// </summary>
        public static void Draw(
            string title,
            string subtitle,
            bool hasContent,
            Action drawContent,
            string emptyHint)
        {
            GraphViewFramePanelStyles.DrawPanelHeader(title, subtitle);

            if (!hasContent)
            {
                GraphViewFramePanelStyles.BeginPaddedContent();
                GraphViewFramePanelStyles.DrawEmptyHint(emptyHint);
                GraphViewFramePanelStyles.EndPaddedContent();
                return;
            }

            GraphViewFramePanelStyles.BeginPaddedContent();
            drawContent?.Invoke();
            GraphViewFramePanelStyles.EndPaddedContent();
        }

        /// <summary>
        /// 绘制固定有内容的区块
        /// </summary>
        public static void DrawSection(string title, string subtitle, Action drawContent)
        {
            GraphViewFramePanelStyles.DrawPanelHeader(title, subtitle);
            GraphViewFramePanelStyles.BeginPaddedContent();
            drawContent?.Invoke();
            GraphViewFramePanelStyles.EndPaddedContent();
        }
    }
}
#endif
