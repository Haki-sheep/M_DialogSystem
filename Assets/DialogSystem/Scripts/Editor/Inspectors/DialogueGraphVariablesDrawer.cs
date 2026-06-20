#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图 Variables 面板绘制
    /// </summary>
    static class DialogueGraphVariablesDrawer
    {
        public static void Draw(DialogueGraph graph, ref Vector2 scroll, float panelWidth)
        {
            if (graph == null)
            {
                EditorGUILayout.LabelField("选中一张对话图以编辑 Variables", EditorStyles.wordWrappedLabel);
                return;
            }

            var so = new SerializedObject(graph);
            var variableListProp = so.FindProperty("variableList");

            scroll = EditorGUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(panelWidth - 16f));
            if (variableListProp != null)
            {
                for (int i = 0; i < variableListProp.arraySize; i++)
                {
                    var element = variableListProp.GetArrayElementAtIndex(i);
                    DrawVariableBlock(element, variableListProp, i);
                    EditorGUILayout.Space(4);
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ 添加变量"))
                ShowAddVariableMenu(graph, variableListProp);

            so.ApplyModifiedProperties();
        }

        static void DrawVariableBlock(SerializedProperty element, SerializedProperty listProp, int index)
        {
            var nameProp = element.FindPropertyRelative("name");
            var typeProp = element.FindPropertyRelative("variableType");
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

            var newType = (EDialogueVariableType)EditorGUILayout.EnumPopup("类型", (EDialogueVariableType)typeProp.enumValueIndex);
            typeProp.enumValueIndex = (int)newType;

            switch (newType)
            {
                case EDialogueVariableType.Float:
                    floatProp.floatValue = EditorGUILayout.FloatField("默认值", floatProp.floatValue);
                    break;
                case EDialogueVariableType.Int:
                    intProp.intValue = EditorGUILayout.IntField("默认值", intProp.intValue);
                    break;
                case EDialogueVariableType.Bool:
                    boolProp.boolValue = EditorGUILayout.Toggle("默认值", boolProp.boolValue);
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        static void ShowAddVariableMenu(DialogueGraph graph, SerializedProperty variableListProp)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Float"), false, () => AddVariable(graph, variableListProp, EDialogueVariableType.Float));
            menu.AddItem(new GUIContent("Int"), false, () => AddVariable(graph, variableListProp, EDialogueVariableType.Int));
            menu.AddItem(new GUIContent("Bool"), false, () => AddVariable(graph, variableListProp, EDialogueVariableType.Bool));
            menu.ShowAsContext();
        }

        static void AddVariable(DialogueGraph graph, SerializedProperty variableListProp, EDialogueVariableType type)
        {
            if (variableListProp == null)
                return;

            var so = variableListProp.serializedObject;
            int index = variableListProp.arraySize;
            variableListProp.InsertArrayElementAtIndex(index);

            var element = variableListProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = GenerateUniqueName(graph, type);
            element.FindPropertyRelative("variableType").enumValueIndex = (int)type;
            element.FindPropertyRelative("defaultFloat").floatValue = 0f;
            element.FindPropertyRelative("defaultInt").intValue = 0;
            element.FindPropertyRelative("defaultBool").boolValue = false;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(graph);
        }

        static string GenerateUniqueName(DialogueGraph graph, EDialogueVariableType type)
        {
            string prefix = type switch
            {
                EDialogueVariableType.Float => "New Float",
                EDialogueVariableType.Int => "New Int",
                EDialogueVariableType.Bool => "New Bool",
                _ => "New Variable",
            };

            var used = new HashSet<string>();
            if (graph.Variables != null)
            {
                foreach (var def in graph.Variables)
                {
                    if (def != null && !string.IsNullOrEmpty(def.name))
                        used.Add(def.name);
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
        /// 变量名下拉
        /// </summary>
        public static string DrawVariablePopup(DialogueGraph graph, string currentName)
        {
            if (graph?.Variables == null || graph.Variables.Count == 0)
                return EditorGUILayout.TextField("变量", currentName);

            var names = new List<string> { "(无)" };
            int selected = 0;

            for (int i = 0; i < graph.Variables.Count; i++)
            {
                var def = graph.Variables[i];
                if (def == null || string.IsNullOrEmpty(def.name))
                    continue;

                names.Add(def.name);
                if (def.name == currentName)
                    selected = names.Count - 1;
            }

            int newIndex = EditorGUILayout.Popup("变量", selected, names.ToArray());
            return newIndex <= 0 ? string.Empty : names[newIndex];
        }
    }
}
#endif
