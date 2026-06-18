#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 右侧 Inspector 面板
    /// </summary>
    static class DialogueGraphInspectorPanel
    {
        public static void Draw(DialogueGraphEditorWindow window)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(window.GetRightPanelWidth()), GUILayout.ExpandHeight(true));
            window.EnsureMenuTreeInternal();

            if (window.SelectedTransition != null)
            {
                DrawTransitionInspector(window);
                EditorGUILayout.EndVertical();
                return;
            }

            var selected = window.MenuTreeAccessor?.Selection?.SelectedValue as Object;
            if (!DialogueGraphEditorWindow.IsAssetAlive(selected))
            {
                window.ClearStaleSelectionInternal();
                selected = null;
            }

            DialogueEditorPanelStyles.DrawPanelHeader("属性", selected != null ? GetSelectionSubtitle(selected) : "未选中");

            if (!DialogueGraphEditorWindow.IsAssetAlive(selected))
            {
                DialogueEditorPanelStyles.BeginPaddedContent();
                DialogueEditorPanelStyles.DrawEmptyHint("在左侧选中 Graph 或节点以编辑属性。");
                DialogueEditorPanelStyles.EndPaddedContent();
                EditorGUILayout.EndVertical();
                return;
            }

            DialogueEditorPanelStyles.BeginPaddedContent();
            DrawSelectedObjectInspector(window, selected);
            DialogueEditorPanelStyles.EndPaddedContent();
            EditorGUILayout.EndVertical();
        }

        static void DrawTransitionInspector(DialogueGraphEditorWindow window)
        {
            DialogueEditorPanelStyles.DrawPanelHeader("属性", "Transition");
            DialogueEditorPanelStyles.BeginPaddedContent();
            DialogueTransitionInspectorDrawer.Draw(window.SelectedTransition);
            DialogueEditorPanelStyles.EndPaddedContent();
        }

        static string GetSelectionSubtitle(Object selected)
        {
            if (!DialogueGraphEditorWindow.IsAssetAlive(selected))
                return "未选中";

            if (selected is DialogueGraph)
                return "Dialogue Graph";
            if (selected is DialogueNode node)
                return $"Dialogue Node  ·  [{node.NodeId}] {node.SpeakerName}";
            return selected.name;
        }

        static void DrawSelectedObjectInspector(DialogueGraphEditorWindow window, Object selected)
        {
            if (!DialogueGraphEditorWindow.IsAssetAlive(selected))
                return;

            if (window.RenameTarget != selected)
                window.SetRenameTarget(selected, selected.name);

            DrawAssetRenameField(window, selected);
            EditorGUILayout.Space(4);

            if (selected is DialogueNode node && node)
            {
                DrawNodeInspector(window, node, selected);
                return;
            }

            DrawGraphInspector(window, selected);
        }

        static void DrawNodeInspector(DialogueGraphEditorWindow window, DialogueNode node, Object selected)
        {
            var so = new SerializedObject(node);
            var scroll = window.InspectorScroll;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUI.BeginChangeCheck();
            DialogueNodeInspectorDrawer.Draw(node, so);
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndScrollView();
            window.InspectorScroll = scroll;

                if (node.IsOptionNode)
                    DrawChoiceButtons(window, node);

            DrawDeleteButton(window, selected);

            if (!changed)
                return;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(node);
            window.QueueGraphViewRefreshFromInspector(node);
            window.RequestMenuLabelRefreshOnly();
        }

        static void DrawGraphInspector(DialogueGraphEditorWindow window, Object selected)
        {
            if (window.InspectorTreeTarget != selected || window.InspectorTree == null)
            {
                window.SetInspectorTreeTarget(selected);
                window.SetInspectorTree(PropertyTree.Create(new SerializedObject(selected)));
                window.InspectorScroll = Vector2.zero;
            }

            var scroll = window.InspectorScroll;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUI.BeginChangeCheck();
            window.InspectorTree.Draw(false);
            bool treeChanged = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndScrollView();
            window.InspectorScroll = scroll;

            DrawDeleteButton(window, selected);

            if (!treeChanged)
                return;

            window.InspectorTree.ApplyChanges();
            EditorUtility.SetDirty(selected);
            window.QueueGraphViewRefreshFromInspector(selected);
            window.RequestMenuLabelRefreshOnly();
        }

        static void DrawDeleteButton(DialogueGraphEditorWindow window, Object selected)
        {
            if (!DialogueGraphEditorWindow.IsAssetAlive(selected))
                return;

            if (selected is not DialogueGraph and not DialogueNode)
                return;

            EditorGUILayout.Space(12);

            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.85f, 0.35f, 0.35f);

            string label = selected is DialogueGraph ? "删除对话图" : "删除节点";
            if (GUILayout.Button(label, GUILayout.Height(28)))
                window.TryDeleteSelectedAsset(selected);

            GUI.backgroundColor = prevColor;
        }

        static void DrawChoiceButtons(DialogueGraphEditorWindow window, DialogueNode node)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加选项", GUILayout.Height(24)))
            {
                DialogueNodeChoiceEditorUtility.AddChoice(node);
                window.QueueGraphViewRefreshFromInspector(node);
            }

            EditorGUI.BeginDisabledGroup((node.ChoiceList?.Count ?? 0) <= 1);
            if (GUILayout.Button("删除最后选项", GUILayout.Height(24))
                && DialogueNodeChoiceEditorUtility.TryRemoveLastChoice(node))
            {
                window.QueueGraphViewRefreshFromInspector(node);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        static void DrawAssetRenameField(DialogueGraphEditorWindow window, Object selected)
        {
            GUI.SetNextControlName(DialogueGraphEditorConstants.RenameFieldControl);
            window.RenameBuffer = EditorGUILayout.TextField("名称", window.RenameBuffer);

            var evt = Event.current;
            if (evt.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == DialogueGraphEditorConstants.RenameFieldControl)
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    window.CommitAssetRename(selected);
                    GUI.FocusControl(null);
                    evt.Use();
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    window.RenameBuffer = selected.name;
                    GUI.FocusControl(null);
                    evt.Use();
                }
            }

            if (evt.type == EventType.Repaint)
            {
                bool focused = GUI.GetNameOfFocusedControl() == DialogueGraphEditorConstants.RenameFieldControl;
                if (window.RenameFieldWasFocused && !focused)
                    window.CommitAssetRename(selected);
                window.RenameFieldWasFocused = focused;
            }
        }
    }
}
#endif
