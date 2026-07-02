#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 单张对话图在编辑器中的节点布局
    /// </summary>
    [System.Serializable]
    class GraphLayoutEntry
    {
        public DialogueGraph graph;
        public List<NodeLayoutEntry> layouts = new();
    }

    /// <summary>
    /// 节点在 GraphView 画布上的坐标
    /// </summary>
    [System.Serializable]
    class NodeLayoutEntry
    {
        public DialogueNode node;
        public Vector2 position;
    }

    /// <summary>
    /// 所有对话图的编辑器布局数据库
    /// </summary>
    class DialogueGraphLayoutDatabase : ScriptableObject
    {
        public List<GraphLayoutEntry> graphs = new();
    }

    /// <summary>
    /// 读写 GraphView 节点布局 与运行时逻辑无关
    /// </summary>
    static class DialogueGraphLayoutStore
    {
        const string DatabasePath = DialogueEditorPaths.LayoutAssetPath;

        static DialogueGraphLayoutDatabase database;

        public static Vector2 GetPosition(DialogueGraph graph, DialogueNode node)
        {
            if (graph == null || node == null)
                return Vector2.zero;

            var entry = GetGraphEntry(graph, create: false);
            if (entry == null)
                return Vector2.zero;

            foreach (var layout in entry.layouts)
            {
                if (layout.node == node)
                    return layout.position;
            }

            return Vector2.zero;
        }

        public static void SetPosition(DialogueGraph graph, DialogueNode node, Vector2 position)
        {
            if (graph == null || node == null)
                return;

            var entry = GetGraphEntry(graph, create: true);
            foreach (var layout in entry.layouts)
            {
                if (layout.node != node)
                    continue;

                layout.position = position;
                SaveDatabase();
                return;
            }

            entry.layouts.Add(new NodeLayoutEntry { node = node, position = position });
            SaveDatabase();
        }

        public static void RemoveNode(DialogueGraph graph, DialogueNode node)
        {
            if (graph == null || node == null)
                return;

            var entry = GetGraphEntry(graph, create: false);
            if (entry == null)
                return;

            entry.layouts.RemoveAll(e => e.node == node);
            SaveDatabase();
        }

        /// <summary>
        /// 删除整张图的布局数据
        /// </summary>
        public static void RemoveGraph(DialogueGraph graph)
        {
            if (graph == null)
                return;

            var db = GetDatabase();
            db.graphs.RemoveAll(e => e.graph == graph);
            SaveDatabase();
        }

        public static void ReplaceGraphLayouts(DialogueGraph graph, IEnumerable<(DialogueNode node, Vector2 position)> layouts)
        {
            if (graph == null)
                return;

            var entry = GetGraphEntry(graph, create: true);
            entry.layouts.Clear();

            if (layouts != null)
            {
                foreach (var (node, position) in layouts)
                {
                    if (node == null)
                        continue;

                    entry.layouts.Add(new NodeLayoutEntry { node = node, position = position });
                }
            }

            SaveDatabase();
        }

        static GraphLayoutEntry GetGraphEntry(DialogueGraph graph, bool create)
        {
            var db = GetDatabase();
            foreach (var entry in db.graphs)
            {
                if (entry.graph == graph)
                    return entry;
            }

            if (!create)
                return null;

            var created = new GraphLayoutEntry { graph = graph };
            db.graphs.Add(created);
            SaveDatabase();
            return created;
        }

        static DialogueGraphLayoutDatabase GetDatabase()
        {
            if (database != null)
                return database;

            database = AssetDatabase.LoadAssetAtPath<DialogueGraphLayoutDatabase>(DatabasePath);
            if (database != null)
                return database;

            DialogueEditorPaths.EnsureGraphAssetFolder();
            database = ScriptableObject.CreateInstance<DialogueGraphLayoutDatabase>();
            AssetDatabase.CreateAsset(database, DatabasePath);
            AssetDatabase.SaveAssets();
            return database;
        }

        static void SaveDatabase()
        {
            if (database == null)
                return;

            EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// 布局库是否有未保存修改
        /// </summary>
        public static bool IsDatabaseDirty()
        {
            if (database != null)
                return EditorUtility.IsDirty(database);

            var db = AssetDatabase.LoadAssetAtPath<DialogueGraphLayoutDatabase>(DatabasePath);
            return db != null && EditorUtility.IsDirty(db);
        }
    }
}
#endif
