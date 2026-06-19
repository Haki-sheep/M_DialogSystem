#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 连线 Transition Inspector 绘制
    /// </summary>
    static class DialogueTransitionInspectorDrawer
    {
        public static void Draw(DialogueTransitionHandle handle)
        {
            if (handle?.sourceNode == null)
            {
                EditorGUILayout.LabelField("未选中连线");
                return;
            }

            if (handle.IsOptionTransition)
            {
                EditorGUILayout.LabelField("Option Transition", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(handle.Title, EditorStyles.miniLabel);
                EditorGUILayout.HelpBox("选项节点的出口 选项文本为运行时按钮文案 条件不满足时该按钮不显示", MessageType.None);
            }
            else
            {
                EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(handle.Title, EditorStyles.miniLabel);
                EditorGUILayout.HelpBox("普通节点的 Out 出口 按空格前进时检查条件 不满足则无法跳转", MessageType.None);
            }

            EditorGUILayout.Space(4);

            var sourceSo = new SerializedObject(handle.sourceNode);
            sourceSo.Update();

            if (handle.IsOptionTransition)
                DrawOptionTransition(handle, sourceSo);
            else
                DrawLinearTransition(handle, sourceSo);

            sourceSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(handle.sourceNode);
        }

        static void DrawLinearTransition(DialogueTransitionHandle handle, SerializedObject sourceSo)
        {
            var transitionProp = sourceSo.FindProperty("nextTransition");
            if (transitionProp == null)
            {
                EditorGUILayout.HelpBox("找不到 nextTransition", MessageType.Warning);
                return;
            }

            DrawConditionsBlock(handle.graph, transitionProp.FindPropertyRelative("conditionList"), "Conditions");
        }

        static void DrawOptionTransition(DialogueTransitionHandle handle, SerializedObject sourceSo)
        {
            var choiceListProp = sourceSo.FindProperty("choiceList");
            int choiceIndex = FindOptionTransitionIndex(handle.sourceNode, handle.optionTransition);
            if (choiceIndex < 0)
            {
                EditorGUILayout.HelpBox("找不到对应选项跳转数据", MessageType.Warning);
                return;
            }

            var optionProp = choiceListProp.GetArrayElementAtIndex(choiceIndex);
            var labelProp = optionProp.FindPropertyRelative("labelText");
            labelProp.stringValue = EditorGUILayout.TextField("选项文本", labelProp.stringValue);
            EditorGUILayout.Space(4);
            DrawConditionsBlock(handle.graph, optionProp.FindPropertyRelative("conditionList"), "Conditions");
        }

        static void DrawConditionsBlock(DialogueGraph graph, SerializedProperty conditionsProp, string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (conditionsProp == null)
                return;

            for (int i = 0; i < conditionsProp.arraySize; i++)
            {
                var conditionProp = conditionsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawConditionRow(graph, conditionProp);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    conditionsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("+ 添加条件"))
                AddCondition(graph, conditionsProp);
        }

        static void DrawConditionRow(DialogueGraph graph, SerializedProperty conditionProp)
        {
            var keyProp = conditionProp.FindPropertyRelative("key");
            var typeProp = conditionProp.FindPropertyRelative("e_Condition");
            var boolProp = conditionProp.FindPropertyRelative("targetBool");
            var floatProp = conditionProp.FindPropertyRelative("targetFloat");
            var intProp = conditionProp.FindPropertyRelative("targetInt");

            string newKey = DialogueGraphParametersDrawer.DrawParameterPopup(graph, keyProp.stringValue);
            keyProp.stringValue = newKey;

            var paramDef = graph?.FindParameter(newKey);
            if (paramDef == null)
            {
                typeProp.enumValueIndex = (int)(E_Condition)EditorGUILayout.EnumPopup("条件", (E_Condition)typeProp.enumValueIndex);
            }
            else
            {
                var options = DialogueCondition.GetConditionOptions(paramDef.parameterType);
                var currentType = (E_Condition)typeProp.enumValueIndex;

                int currentIndex = System.Array.IndexOf(options, currentType);
                if (currentIndex < 0)
                {
                    typeProp.enumValueIndex = (int)options[0];
                    currentIndex = 0;
                }

                string[] labels = System.Array.ConvertAll(options, DialogueCondition.GetDisplayLabel);
                int picked = EditorGUILayout.Popup("条件", currentIndex, labels);
                typeProp.enumValueIndex = (int)options[picked];
            }

            var conditionType = (E_Condition)typeProp.enumValueIndex;
            if (DialogueCondition.IsFloatThresholdType(conditionType))
                floatProp.floatValue = EditorGUILayout.FloatField("阈值", floatProp.floatValue);
            else if (DialogueCondition.IsIntThresholdType(conditionType))
                intProp.intValue = EditorGUILayout.IntField("阈值", intProp.intValue);
        }

        static void AddCondition(DialogueGraph graph, SerializedProperty conditionsProp)
        {
            int index = conditionsProp.arraySize;
            conditionsProp.InsertArrayElementAtIndex(index);
            var conditionProp = conditionsProp.GetArrayElementAtIndex(index);

            string defaultKey = graph?.Parameters != null && graph.Parameters.Count > 0
                ? graph.Parameters[0]?.name ?? string.Empty
                : string.Empty;

            conditionProp.FindPropertyRelative("key").stringValue = defaultKey;
            var paramDef = graph?.FindParameter(defaultKey);
            var defaultType = paramDef != null
                ? DialogueCondition.GetConditionOptions(paramDef.parameterType)[0]
                : E_Condition.None;
            conditionProp.FindPropertyRelative("e_Condition").enumValueIndex = (int)defaultType;
        }

        static int FindOptionTransitionIndex(DialogueNode node, DialogueOptionTransition optionTransition)
        {
            if (node?.ChoiceList == null || optionTransition == null)
                return -1;

            for (int i = 0; i < node.ChoiceList.Count; i++)
            {
                if (ReferenceEquals(node.ChoiceList[i], optionTransition))
                    return i;
            }

            return -1;
        }
    }
}
#endif
