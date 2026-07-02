#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 叙事 Graph 编辑器 IMGUI 与 UIElements 主题
    /// </summary>
    public static class GraphViewFramePanelStyles
    {
        static bool stylesBuilt;
        static bool builtForProSkin;

        static GUIStyle panelTitle;
        static GUIStyle panelSubtitle;
        static GUIStyle sectionLabel;
        static GUIStyle emptyHint;

        public static Color PanelBackground => EditorGUIUtility.isProSkin
            ? new Color(0.198f, 0.198f, 0.198f)
            : new Color(0.765f, 0.765f, 0.765f);

        public static Color InspectorBackground => EditorGUIUtility.isProSkin
            ? new Color(0.205f, 0.205f, 0.205f)
            : new Color(0.785f, 0.785f, 0.785f);

        public static Color HeaderBackground => EditorGUIUtility.isProSkin
            ? new Color(0.255f, 0.255f, 0.255f)
            : new Color(0.835f, 0.835f, 0.835f);

        public static Color BorderColor => EditorGUIUtility.isProSkin
            ? new Color(0.08f, 0.08f, 0.08f, 0.85f)
            : new Color(0.45f, 0.45f, 0.45f, 0.55f);

        public static Color AccentColor => EditorGUIUtility.isProSkin
            ? new Color(0.28f, 0.56f, 0.95f)
            : new Color(0.16f, 0.44f, 0.86f);

        public static Color SplitterNormal => EditorGUIUtility.isProSkin
            ? new Color(0.12f, 0.12f, 0.12f)
            : new Color(0.62f, 0.62f, 0.62f);

        public static Color SplitterHover => EditorGUIUtility.isProSkin
            ? new Color(0.28f, 0.56f, 0.95f, 0.55f)
            : new Color(0.16f, 0.44f, 0.86f, 0.45f);

        public static Color GraphPanelBackground => new Color(0.14f, 0.14f, 0.14f);

        /// <summary>
        /// Odin 左侧资源树菜单样式
        /// </summary>
        public static OdinMenuStyle CreateMenuStyle()
        {
            bool pro = EditorGUIUtility.isProSkin;
            return new OdinMenuStyle
            {
                Height = 26,
                Offset = 6,
                IndentAmount = 14,
                IconSize = 16,
                IconPadding = 6,
                IconOffset = 0,
                NotSelectedIconAlpha = pro ? 0.55f : 0.75f,
                Borders = true,
                BorderPadding = 4,
                BorderAlpha = pro ? 0.12f : 0.2f,
                DrawFoldoutTriangle = true,
                TriangleSize = 8,
                TrianglePadding = 4,
                SelectedColorDarkSkin = new Color(0.27f, 0.52f, 0.92f, 0.42f),
                SelectedInactiveColorDarkSkin = new Color(0.27f, 0.52f, 0.92f, 0.22f),
                SelectedColorLightSkin = new Color(0.24f, 0.49f, 0.90f, 0.32f),
                SelectedInactiveColorLightSkin = new Color(0.24f, 0.49f, 0.90f, 0.18f),
            };
        }

        /// <summary>
        /// 侧栏顶栏标题
        /// </summary>
        public static void DrawPanelHeader(string title, string subtitle = null)
        {
            EnsureBuilt();

            float height = string.IsNullOrEmpty(subtitle) ? 30f : 40f;
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            EditorGUI.DrawRect(rect, HeaderBackground);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), AccentColor);

            var titleRect = new Rect(rect.x + 10f, rect.y + (string.IsNullOrEmpty(subtitle) ? 7f : 5f), rect.width - 20f, 18f);
            GUI.Label(titleRect, title, panelTitle);

            if (!string.IsNullOrEmpty(subtitle))
                GUI.Label(new Rect(rect.x + 10f, rect.y + 22f, rect.width - 20f, 14f), subtitle, panelSubtitle);
        }

        public static void BeginPaddedContent()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(8f);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }

        public static void EndPaddedContent()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(8f);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawEmptyHint(string message)
        {
            EnsureBuilt();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(emptyHint, GUILayout.MaxWidth(220f));
            EditorGUILayout.LabelField("未选中对象", sectionLabel);
            GUILayout.Space(4f);
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        static void EnsureBuilt()
        {
            if (stylesBuilt && builtForProSkin == EditorGUIUtility.isProSkin)
                return;

            stylesBuilt = true;
            builtForProSkin = EditorGUIUtility.isProSkin;

            panelTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
            panelSubtitle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, clipping = TextClipping.Clip };
            sectionLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.25f, 0.25f, 0.25f) },
            };
            emptyHint = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 12, 12),
                alignment = TextAnchor.MiddleCenter,
            };
        }
    }
}
#endif
