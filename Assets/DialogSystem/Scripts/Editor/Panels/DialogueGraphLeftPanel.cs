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
    /// Variables 变量面板
    /// </summary>
    static class DialogueGraphVariablesPanel
    {
        public static void Draw(DialogueGraphEditorWindow window)
        {
            float panelWidth = window.GetVariablesPanelWidth();
            EditorGUILayout.BeginVertical(GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
            DialogueEditorPanelStyles.DrawPanelHeader("Variables", window.GetActiveGraph()?.name ?? "未选中图");
            DialogueEditorPanelStyles.BeginPaddedContent();

            var scroll = window.VariablesScroll;
            DialogueGraphVariablesDrawer.Draw(window.GetActiveGraph(), ref scroll, panelWidth);
            window.VariablesScroll = scroll;

            DialogueEditorPanelStyles.EndPaddedContent();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
