#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务图节点布局存储
    /// </summary>
    static class QuestGraphLayoutStore
    {
        [Serializable]
        class LayoutEntry
        {
            public QuestGraph graph;
            public QuestNode node;
            public Vector2 position;
        }

        class QuestGraphLayoutAsset : ScriptableObject
        {
            public List<LayoutEntry> entryList = new();
        }

        static QuestGraphLayoutAsset database;

        static QuestGraphLayoutAsset Database
        {
            get
            {
                if (database != null)
                    return database;

                database = AssetDatabase.LoadAssetAtPath<QuestGraphLayoutAsset>(QuestEditorPaths.LayoutAssetPath);
                if (database != null)
                    return database;

                QuestEditorPaths.EnsureGraphAssetFolder();
                database = ScriptableObject.CreateInstance<QuestGraphLayoutAsset>();
                AssetDatabase.CreateAsset(database, QuestEditorPaths.LayoutAssetPath);
                AssetDatabase.SaveAssets();
                return database;
            }
        }

        #region 读写

        public static Vector2 GetPosition(QuestGraph graph, QuestNode node)
        {
            if (graph == null || node == null)
                return Vector2.zero;

            foreach (var entry in Database.entryList)
            {
                if (entry.graph == graph && entry.node == node)
                    return entry.position;
            }

            return Vector2.zero;
        }

        public static void SetPosition(QuestGraph graph, QuestNode node, Vector2 position)
        {
            if (graph == null || node == null)
                return;

            foreach (var entry in Database.entryList)
            {
                if (entry.graph == graph && entry.node == node)
                {
                    entry.position = position;
                    EditorUtility.SetDirty(Database);
                    return;
                }
            }

            Database.entryList.Add(new LayoutEntry { graph = graph, node = node, position = position });
            EditorUtility.SetDirty(Database);
        }

        #endregion
    }
}
#endif
