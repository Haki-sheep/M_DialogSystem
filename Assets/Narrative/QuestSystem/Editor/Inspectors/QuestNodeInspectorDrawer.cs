#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务节点右侧 Inspector
    /// </summary>
    static class QuestNodeInspectorDrawer
    {
        #region 绘制

        public static void Draw(QuestGraphEditorWindow window, QuestNode node)
        {
            if (node == null)
                return;

            EditorGUILayout.LabelField("节点", EditorStyles.boldLabel);

            var so = new SerializedObject(node);
            so.Update();

            var nodeIdProp = so.FindProperty("nodeId");
            var nodeTypeProp = so.FindProperty("nodeType");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(nodeIdProp);
            EditorGUILayout.PropertyField(nodeTypeProp);
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                window.GraphView?.RefreshNodeView(node);
            }

            var type = (EQuestNodeType)nodeTypeProp.enumValueIndex;
            switch (type)
            {
                case EQuestNodeType.Start:
                    DrawStartNode(window, node);
                    break;
                case EQuestNodeType.Objective:
                    if (DrawObjective(so))
                        window.GraphView?.RefreshNodeView(node);
                    break;
                case EQuestNodeType.Branch:
                    EditorGUILayout.HelpBox("分支节点暂未实现 仅作占位", MessageType.Info);
                    break;
                case EQuestNodeType.End:
                    break;
            }

            so.ApplyModifiedProperties();
        }

        static void DrawStartNode(QuestGraphEditorWindow window, QuestNode node)
        {
            var graph = window.GetActiveGraph();
            if (graph == null)
                return;

            bool isStart = graph.StartNode == node;
            EditorGUILayout.LabelField(isStart ? "当前为入口节点" : "非入口节点");

            if (!isStart && GUILayout.Button("设为入口"))
            {
                Undo.RecordObject(graph, "Set Quest Start Node");
                graph.SetStartNodeInEditor(node);
                EditorUtility.SetDirty(graph);
            }
        }

        static bool DrawObjective(SerializedObject so)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("目标", EditorStyles.boldLabel);

            var objectiveProp = so.FindProperty("objective");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(objectiveProp.FindPropertyRelative("objectiveType"));
            EditorGUILayout.PropertyField(objectiveProp.FindPropertyRelative("targetId"));
            EditorGUILayout.PropertyField(objectiveProp.FindPropertyRelative("targetKey"));
            EditorGUILayout.PropertyField(objectiveProp.FindPropertyRelative("requiredCount"));
            EditorGUILayout.PropertyField(objectiveProp.FindPropertyRelative("description"));
            return EditorGUI.EndChangeCheck();
        }

        #endregion
    }
}
#endif
