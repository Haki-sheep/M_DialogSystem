#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 节点 Inspector 只显示台词内容 连线在画布上编辑
    /// </summary>
    static class DialogueNodeInspectorDrawer
    {
        public static void Draw(DialogueNode node, SerializedObject so)
        {
            if (node == null || so == null)
                return;

            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("nodeId"));
            EditorGUILayout.PropertyField(so.FindProperty("speakType"));
            EditorGUILayout.PropertyField(so.FindProperty("speakerName"));
            EditorGUILayout.PropertyField(so.FindProperty("dialogText"));
            EditorGUILayout.PropertyField(so.FindProperty("isOptionNode"));

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("出口连线与条件请在画布上点击连线编辑", MessageType.Info);

            so.ApplyModifiedProperties();
        }
    }
}
#endif
