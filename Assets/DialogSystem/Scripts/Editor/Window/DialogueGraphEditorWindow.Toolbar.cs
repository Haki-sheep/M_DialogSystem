#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    partial class DialogueGraphEditorWindow
    {
        void DrawToolbar()
        {
            HandleSaveShortcut();

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

            GUILayout.FlexibleSpace();
            if (HasUnsavedChanges())
                GUILayout.Label("*", UnsavedMarkStyle, GUILayout.Width(14));

            EditorGUILayout.EndHorizontal();
        }

        static GUIStyle unsavedMarkStyle;

        static GUIStyle UnsavedMarkStyle
        {
            get
            {
                if (unsavedMarkStyle == null)
                {
                    unsavedMarkStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 16,
                        alignment = TextAnchor.MiddleCenter,
                    };
                    unsavedMarkStyle.normal.textColor = new Color(1f, 0.82f, 0.2f);
                }

                return unsavedMarkStyle;
            }
        }

        void HandleSaveShortcut()
        {
            var evt = Event.current;
            if (evt.type != EventType.KeyDown || !evt.control || evt.keyCode != KeyCode.S)
                return;

            if (!SaveActiveGraph())
                return;

            evt.Use();
            Repaint();
        }

        /// <summary>
        /// 当前图或布局是否有未保存修改
        /// </summary>
        bool HasUnsavedChanges()
        {
            var graph = GetActiveGraph();
            if (graph == null)
                return DialogueGraphLayoutStore.IsDatabaseDirty();

            if (EditorUtility.IsDirty(graph))
                return true;

            if (graph.NodeList != null)
            {
                foreach (var node in graph.NodeList)
                {
                    if (node != null && EditorUtility.IsDirty(node))
                        return true;
                }
            }

            return DialogueGraphLayoutStore.IsDatabaseDirty();
        }

        /// <summary>
        /// 保存当前对话图相关资产
        /// </summary>
        bool SaveActiveGraph()
        {
            if (!HasUnsavedChanges())
                return false;

            AssetDatabase.SaveAssets();
            Repaint();
            return true;
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
