#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 可拖拽分栏条
    /// </summary>
    public sealed class PanelSplitter
    {
        readonly Action<float> onResizeDelta;
        float lastMouseX;

        public VisualElement Element { get; }

        public PanelSplitter(string name, Action<float> onResizeDelta)
        {
            this.onResizeDelta = onResizeDelta;
            Element = new VisualElement { name = name };
            Element.style.width = GraphViewFrameConstants.SplitterWidth;
            Element.style.flexShrink = 0;
            Element.style.flexGrow = 0;
            Element.style.alignSelf = Align.Stretch;
            Element.style.backgroundColor = GraphViewFramePanelStyles.SplitterNormal;
            Element.pickingMode = PickingMode.Position;

            Element.RegisterCallback<MouseDownEvent>(OnMouseDown);
            Element.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            Element.RegisterCallback<MouseUpEvent>(OnMouseUp);
            Element.RegisterCallback<MouseEnterEvent>(_ => Element.style.backgroundColor = GraphViewFramePanelStyles.SplitterHover);
            Element.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (!Element.HasMouseCapture())
                    Element.style.backgroundColor = GraphViewFramePanelStyles.SplitterNormal;
            });
        }

        public void ReleaseCapture()
        {
            if (Element.HasMouseCapture())
                Element.ReleaseMouse();
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
                return;

            lastMouseX = evt.mousePosition.x;
            Element.CaptureMouse();
            evt.StopPropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!Element.HasMouseCapture())
                return;

            float delta = evt.mousePosition.x - lastMouseX;
            lastMouseX = evt.mousePosition.x;
            onResizeDelta?.Invoke(delta);
            evt.StopPropagation();
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            if (!Element.HasMouseCapture())
                return;

            Element.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}
#endif
