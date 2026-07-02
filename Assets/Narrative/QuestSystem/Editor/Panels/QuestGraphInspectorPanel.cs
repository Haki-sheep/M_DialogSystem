#if UNITY_EDITOR
using Miemie.Narrative.GraphViewFrame.Editor;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 右侧任务图 Inspector
    /// </summary>
    static class QuestGraphInspectorPanel
    {
        #region 绘制

        public static void Draw(QuestGraphEditorWindow window)
        {
            var selectedNode = window.SelectedNode;
            var graph = window.GetActiveGraph();
            string subtitle = selectedNode != null
                ? $"[{selectedNode.NodeId}] {selectedNode.NodeType}"
                : graph != null ? graph.name : "未选中";

            if (selectedNode != null)
            {
                InspectorPanelShell.Draw("属性", subtitle, true, () => QuestNodeInspectorDrawer.Draw(window, selectedNode), null);
                return;
            }

            if (graph == null)
            {
                InspectorPanelShell.Draw("属性", subtitle, false, null, "在左侧选中任务图 或在画布上选中节点。");
                return;
            }

            InspectorPanelShell.Draw("属性", subtitle, true, () => DrawGraph(graph), null);
        }

        static void DrawGraph(QuestGraph graph)
        {
            var so = new SerializedObject(graph);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("questGraphId"));
            EditorGUILayout.PropertyField(so.FindProperty("startNode"));

            var questDefProp = so.FindProperty("questDef");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(questDefProp);
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                SyncQuestDefLink(graph, questDefProp.objectReferenceValue as QuestDef);
            }
            else
            {
                so.ApplyModifiedProperties();
            }

            EditorGUILayout.PropertyField(so.FindProperty("nodeList"), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("解析预览", EditorStyles.boldLabel);
            var objectives = QuestGraphUtility.CollectObjectives(graph);
            if (objectives.Count == 0)
                EditorGUILayout.HelpBox("图中尚无 Objective 节点", MessageType.Info);
            else
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    var o = objectives[i];
                    if (o == null)
                        continue;
                    EditorGUILayout.LabelField($"{i + 1}. {o.objectiveType} {o.targetKey} x{o.requiredCount}");
                }
            }
        }

        static void SyncQuestDefLink(QuestGraph graph, QuestDef questDef)
        {
            if (questDef == null)
                return;

            var questSo = new SerializedObject(questDef);
            questSo.FindProperty("questGraph").objectReferenceValue = graph;
            questSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(questDef);
        }

        #endregion
    }
}
#endif
