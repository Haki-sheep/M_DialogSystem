#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("新建对话图", EditorStyles.toolbarButton, GUILayout.Width(80)))
                CreateNewDialogueGraph();

            var targetGraph = GetActiveGraph();
            if (targetGraph != null)
            {
                if (GUILayout.Button("校验本图", EditorStyles.toolbarButton, GUILayout.Width(72)))
                    DialogueGraphValidator.Validate(targetGraph);

                if (GUILayout.Button("导出 JSON", EditorStyles.toolbarButton, GUILayout.Width(72)))
                    DialogueGraphJsonIO.ExportWithSaveDialog(targetGraph);
            }

            if (GUILayout.Button("导入 JSON", EditorStyles.toolbarButton, GUILayout.Width(72))
                && DialogueGraphJsonIO.ImportWithOpenDialog(targetGraph, out var importedGraph))
            {
                RefreshAfterExternalChange(importedGraph);
            }

            if (Application.isPlaying && lastSelectedGraph != null)
            {
                if (GUILayout.Button("播放当前图", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    var runner = Object.FindObjectOfType<DialogueRunner>();
                    if (runner != null)
                        runner.PlayGraph(lastSelectedGraph);
                    else
                        Debug.LogWarning("场景中未找到 DialogueRunner");
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void CreateNewDialogueGraph()
        {
            DialogueEditorPaths.EnsureGraphAssetFolder();

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DialogueEditorPaths.GraphAssetPath}/New Dialogue Graph.asset");
            var graph = CreateInstance<DialogueGraph>();
            AssetDatabase.CreateAsset(graph, assetPath);

            string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var so = new SerializedObject(graph);
            so.FindProperty("graphId").intValue = GetNextGraphId();
            so.FindProperty("graphName").stringValue = assetName;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            Selection.activeObject = graph;
            RefreshAfterExternalChange(graph);
        }

        static int GetNextGraphId()
        {
            int maxId = 0;
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(DialogueGraph)}"))
            {
                var graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(AssetDatabase.GUIDToAssetPath(guid));
                if (graph != null && graph.GraphId > maxId)
                    maxId = graph.GraphId;
            }

            return maxId + 1;
        }
    }
}
#endif
