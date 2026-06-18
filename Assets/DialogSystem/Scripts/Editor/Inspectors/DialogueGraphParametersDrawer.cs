#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图 Parameters 面板绘制
    /// </summary>
    static class DialogueGraphParametersDrawer
    {
        public static void Draw(DialogueGraph graph, ref Vector2 scroll, float panelWidth)
        {
            if (graph == null)
            {
                EditorGUILayout.LabelField("选中一张对话图以编辑 Parameters", EditorStyles.wordWrappedLabel);
                return;
            }

            var so = new SerializedObject(graph);
            var parametersProp = so.FindProperty("parameters");

            scroll = EditorGUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(panelWidth - 16f));
            if (parametersProp != null)
            {
                for (int i = 0; i < parametersProp.arraySize; i++)
                {
                    var element = parametersProp.GetArrayElementAtIndex(i);
                    DrawParameterBlock(element, parametersProp, i);
                    EditorGUILayout.Space(4);
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ 添加参数"))
                ShowAddParameterMenu(graph, parametersProp);

            so.ApplyModifiedProperties();
        }

        static void DrawParameterBlock(SerializedProperty element, SerializedProperty listProp, int index)
        {
            var nameProp = element.FindPropertyRelative("name");
            var typeProp = element.FindPropertyRelative("parameterType");
            var floatProp = element.FindPropertyRelative("defaultFloat");
            var intProp = element.FindPropertyRelative("defaultInt");
            var boolProp = element.FindPropertyRelative("defaultBool");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("名称", GUILayout.Width(36));
            nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
            if (GUILayout.Button("-", GUILayout.Width(22)))
            {
                listProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            var newType = (E_DialogueParameterType)EditorGUILayout.EnumPopup("类型", (E_DialogueParameterType)typeProp.enumValueIndex);
            typeProp.enumValueIndex = (int)newType;

            switch (newType)
            {
                case E_DialogueParameterType.Float:
                    floatProp.floatValue = EditorGUILayout.FloatField("默认值", floatProp.floatValue);
                    break;
                case E_DialogueParameterType.Int:
                    intProp.intValue = EditorGUILayout.IntField("默认值", intProp.intValue);
                    break;
                case E_DialogueParameterType.Bool:
                    boolProp.boolValue = EditorGUILayout.Toggle("默认值", boolProp.boolValue);
                    break;
                case E_DialogueParameterType.Trigger:
                    EditorGUILayout.LabelField("默认值", "运行时 SetTrigger 触发");
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        static void ShowAddParameterMenu(DialogueGraph graph, SerializedProperty parametersProp)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Float"), false, () => AddParameter(graph, parametersProp, E_DialogueParameterType.Float));
            menu.AddItem(new GUIContent("Int"), false, () => AddParameter(graph, parametersProp, E_DialogueParameterType.Int));
            menu.AddItem(new GUIContent("Bool"), false, () => AddParameter(graph, parametersProp, E_DialogueParameterType.Bool));
            menu.AddItem(new GUIContent("Trigger"), false, () => AddParameter(graph, parametersProp, E_DialogueParameterType.Trigger));
            menu.ShowAsContext();
        }

        static void AddParameter(DialogueGraph graph, SerializedProperty parametersProp, E_DialogueParameterType type)
        {
            if (parametersProp == null)
                return;

            var so = parametersProp.serializedObject;
            int index = parametersProp.arraySize;
            parametersProp.InsertArrayElementAtIndex(index);

            var element = parametersProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = GenerateUniqueName(graph, type);
            element.FindPropertyRelative("parameterType").enumValueIndex = (int)type;
            element.FindPropertyRelative("defaultFloat").floatValue = 0f;
            element.FindPropertyRelative("defaultInt").intValue = 0;
            element.FindPropertyRelative("defaultBool").boolValue = false;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(graph);
        }

        static string GenerateUniqueName(DialogueGraph graph, E_DialogueParameterType type)
        {
            string prefix = type switch
            {
                E_DialogueParameterType.Float => "New Float",
                E_DialogueParameterType.Int => "New Int",
                E_DialogueParameterType.Bool => "New Bool",
                E_DialogueParameterType.Trigger => "New Trigger",
                _ => "New Param",
            };

            var used = new HashSet<string>();
            if (graph.Parameters != null)
            {
                foreach (var param in graph.Parameters)
                {
                    if (param != null && !string.IsNullOrEmpty(param.name))
                        used.Add(param.name);
                }
            }

            string candidate = prefix;
            int suffix = 0;
            while (used.Contains(candidate))
            {
                suffix++;
                candidate = $"{prefix} {suffix}";
            }

            return candidate;
        }

        /// <summary>
        /// 获取参数名下拉
        /// </summary>
        public static string DrawParameterPopup(DialogueGraph graph, string currentKey)
        {
            if (graph?.Parameters == null || graph.Parameters.Count == 0)
                return EditorGUILayout.TextField("参数", currentKey);

            var names = new List<string> { "(无)" };
            int selected = 0;

            for (int i = 0; i < graph.Parameters.Count; i++)
            {
                var param = graph.Parameters[i];
                if (param == null || string.IsNullOrEmpty(param.name))
                    continue;

                names.Add(param.name);
                if (param.name == currentKey)
                    selected = names.Count - 1;
            }

            int newIndex = EditorGUILayout.Popup("参数", selected, names.ToArray());
            return newIndex <= 0 ? string.Empty : names[newIndex];
        }
    }
}
#endif
