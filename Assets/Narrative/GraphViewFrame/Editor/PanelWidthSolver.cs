#if UNITY_EDITOR
using UnityEngine;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 侧栏宽度分配 从右向左收缩
    /// </summary>
    public static class PanelWidthSolver
    {
        public struct PanelWidthState
        {
            public float Width;
            public float MinWidth;
        }

        /// <summary>
        /// 在保留画布最小宽度的前提下分配各侧栏宽度
        /// </summary>
        public static void Solve(
            float availableWidth,
            float minGraphWidth,
            float minGraphWidthRatio,
            PanelWidthState[] panels)
        {
            if (panels == null || panels.Length == 0)
                return;

            float minGraph = Mathf.Min(
                minGraphWidth,
                Mathf.Max(GraphViewFrameConstants.MinGraphPanelWidthFloor, availableWidth * minGraphWidthRatio));

            for (int i = 0; i < panels.Length; i++)
                panels[i].Width = Mathf.Max(panels[i].Width, panels[i].MinWidth);

            float sum = 0f;
            for (int i = 0; i < panels.Length; i++)
                sum += panels[i].Width;

            float overflow = sum + minGraph - availableWidth;
            for (int i = panels.Length - 1; i >= 0 && overflow > 0f; i--)
            {
                float shrink = Mathf.Min(overflow, panels[i].Width - panels[i].MinWidth);
                panels[i].Width -= shrink;
                overflow -= shrink;
            }

            for (int i = 0; i < panels.Length; i++)
                panels[i].Width = Mathf.Clamp(panels[i].Width, panels[i].MinWidth, availableWidth);
        }
    }
}
#endif
