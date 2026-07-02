#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 侧栏 IMGUI 容器工厂
    /// </summary>
    public static class SidePanelFactory
    {
        /// <summary>
        /// 创建带主题的侧栏容器
        /// </summary>
        public static IMGUIContainer Create(
            string name,
            Action draw,
            float width,
            bool isInspector,
            Action<VisualElement> extraStyle = null)
        {
            var container = new IMGUIContainer(draw) { name = name };
            container.style.width = width;
            container.style.flexShrink = 0;
            container.style.flexGrow = 0;
            container.style.alignSelf = Align.Stretch;
            container.focusable = true;
            container.pickingMode = PickingMode.Position;
            ApplyTheme(container, isInspector);
            extraStyle?.Invoke(container);
            return container;
        }

        static void ApplyTheme(VisualElement panel, bool isInspector)
        {
            panel.style.backgroundColor = isInspector
                ? GraphViewFramePanelStyles.InspectorBackground
                : GraphViewFramePanelStyles.PanelBackground;

            if (isInspector)
            {
                panel.style.borderLeftWidth = 1;
                panel.style.borderLeftColor = GraphViewFramePanelStyles.BorderColor;
            }
            else
            {
                panel.style.borderRightWidth = 1;
                panel.style.borderRightColor = GraphViewFramePanelStyles.BorderColor;
            }
        }
    }
}
#endif
