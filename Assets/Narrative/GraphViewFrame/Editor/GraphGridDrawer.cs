#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// Graph 画布网格绘制
    /// </summary>
    public static class GraphGridDrawer
    {
        /// <summary>
        /// 按当前视口变换绘制网格线
        /// </summary>
        public static void DrawGridLines(Rect rect, Vector3 viewOffset, float viewScale, float baseSpacing, Color color, float lineWidth)
        {
            float scale = Mathf.Max(0.05f, viewScale);
            float spacing = baseSpacing * scale;
            if (spacing < 4f)
                return;

            float xStart = viewOffset.x % spacing;
            float yStart = viewOffset.y % spacing;

            if (xStart > 0f)
                xStart -= spacing;
            if (yStart > 0f)
                yStart -= spacing;

            for (float x = xStart; x < rect.width; x += spacing)
                EditorGUI.DrawRect(new Rect(x, 0f, lineWidth, rect.height), color);

            for (float y = yStart; y < rect.height; y += spacing)
                EditorGUI.DrawRect(new Rect(0f, y, rect.width, lineWidth), color);
        }
    }
}
#endif
