#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 左侧 Odin 资源树面板
    /// </summary>
    static class DialogueGraphLeftPanel
    {
        public static void Draw(DialogueGraphEditorWindow window)
        {
            float panelWidth = window.GetLeftPanelWidth();
            EditorGUILayout.BeginVertical(GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
            window.EnsureMenuTreeInternal();
            window.DrawLeftMenu(panelWidth);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Parameters 参数面板
    /// </summary>
    static class DialogueGraphParametersPanel
    {
        public static void Draw(DialogueGraphEditorWindow window)
        {
            float panelWidth = window.GetParametersPanelWidth();
            EditorGUILayout.BeginVertical(GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
            DialogueEditorPanelStyles.DrawPanelHeader("Parameters", window.GetActiveGraph()?.name ?? "未选中图");
            DialogueEditorPanelStyles.BeginPaddedContent();

            var scroll = window.ParametersScroll;
            DialogueGraphParametersDrawer.Draw(window.GetActiveGraph(), ref scroll, panelWidth);
            window.ParametersScroll = scroll;

            DialogueEditorPanelStyles.EndPaddedContent();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
