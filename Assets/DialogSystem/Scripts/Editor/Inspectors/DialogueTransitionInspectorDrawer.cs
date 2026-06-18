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

            EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(handle.Title, EditorStyles.miniLabel);
            EditorGUILayout.HelpBox("条件挂在这条连线上 运行时按 Parameters 判断是否通过", MessageType.None);
            EditorGUILayout.Space(4);

            var sourceSo = new SerializedObject(handle.sourceNode);
            sourceSo.Update();

            if (handle.IsChoice)
                DrawChoiceTransition(handle, sourceSo);
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

            DrawConditionsBlock(handle.graph, transitionProp.FindPropertyRelative("conditions"), "Conditions");
        }

        static void DrawChoiceTransition(DialogueTransitionHandle handle, SerializedObject sourceSo)
        {
            var choiceListProp = sourceSo.FindProperty("choiceList");
            int choiceIndex = FindChoiceIndex(handle.sourceNode, handle.choice);
            if (choiceIndex < 0)
            {
                EditorGUILayout.HelpBox("找不到对应选项数据", MessageType.Warning);
                return;
            }

            var choiceProp = choiceListProp.GetArrayElementAtIndex(choiceIndex);
            EnsureSerializedConditions(choiceProp);
            var labelProp = choiceProp.FindPropertyRelative("labelText");
            labelProp.stringValue = EditorGUILayout.TextField("选项文本", labelProp.stringValue);
            EditorGUILayout.Space(4);
            DrawConditionsBlock(handle.graph, choiceProp.FindPropertyRelative("conditions"), "Conditions");
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
                if (!DialogueCondition.IsAnimatorStyle(currentType))
                {
                    currentType = DialogueCondition.MigrateToAnimatorStyle(currentType, boolProp.boolValue);
                    typeProp.enumValueIndex = (int)currentType;
                }

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

        static int FindChoiceIndex(DialogueNode node, DialogueChoice choice)
        {
            if (node?.ChoiceList == null || choice == null)
                return -1;

            for (int i = 0; i < node.ChoiceList.Count; i++)
            {
                if (ReferenceEquals(node.ChoiceList[i], choice))
                    return i;
            }

            return -1;
        }

        static void EnsureSerializedConditions(SerializedProperty connectionProp)
        {
            if (connectionProp == null)
                return;

            var conditionsProp = connectionProp.FindPropertyRelative("conditions");
            var legacyProp = connectionProp.FindPropertyRelative("condition");
            if (conditionsProp == null || legacyProp == null || conditionsProp.arraySize > 0)
                return;

            var legacyType = (E_Condition)legacyProp.FindPropertyRelative("e_Condition").enumValueIndex;
            if (legacyType == E_Condition.None)
                return;

            conditionsProp.InsertArrayElementAtIndex(0);
            CopyConditionProperties(legacyProp, conditionsProp.GetArrayElementAtIndex(0));
        }

        static void CopyConditionProperties(SerializedProperty from, SerializedProperty to)
        {
            to.FindPropertyRelative("e_Condition").enumValueIndex = from.FindPropertyRelative("e_Condition").enumValueIndex;
            to.FindPropertyRelative("key").stringValue = from.FindPropertyRelative("key").stringValue;
            to.FindPropertyRelative("targetBool").boolValue = from.FindPropertyRelative("targetBool").boolValue;
            to.FindPropertyRelative("targetFloat").floatValue = from.FindPropertyRelative("targetFloat").floatValue;
            to.FindPropertyRelative("targetInt").intValue = from.FindPropertyRelative("targetInt").intValue;
        }
    }
}
#endif
