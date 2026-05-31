using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    [Serializable]
    public class NodeLayoutEntry
    {
        public DialogueNode node;
        public Vector2 position;
    }

    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Dialog System/Dialogue Graph")]
    public class DialogueGraph : ScriptableObject
    {
        #region 字段
        [SerializeField] private int graphId;
        [SerializeField] private string graphName;
        [SerializeField] private DialogueNode startNode;
        [SerializeField] private List<DialogueNode> nodeList = new List<DialogueNode>();
        [HideInInspector][SerializeField] 
        private List<NodeLayoutEntry> nodeLayouts = new List<NodeLayoutEntry>();
        #endregion

        #region 属性
        public int GraphId => graphId;
        public string GraphName => graphName;
        public DialogueNode StartNode => startNode;
        public List<DialogueNode> NodeList => nodeList;
        public List<NodeLayoutEntry> NodeLayouts => nodeLayouts;
        #endregion

        #region 方法
        public void AddNode(DialogueNode node)
        {
            if (nodeList is null)
                nodeList = new List<DialogueNode>();
            nodeList.Add(node);
        }

        public void RemoveNode(DialogueNode node)
        {
            if (nodeList is null)
            {
                Debug.LogError("Node list is null, please add node first");
                return;
            }
            nodeList.Remove(node);
            RemoveLayout(node);
        }

        public Vector2 GetLayout(DialogueNode node)
        {
            if (node == null || nodeLayouts == null)
                return Vector2.zero;

            foreach (var entry in nodeLayouts)
            {
                if (entry.node == node)
                    return entry.position;
            }

            return Vector2.zero;
        }

        public void SetLayout(DialogueNode node, Vector2 position)
        {
            if (node == null)
                return;

            if (nodeLayouts == null)
                nodeLayouts = new List<NodeLayoutEntry>();

            foreach (var entry in nodeLayouts)
            {
                if (entry.node == node)
                {
                    entry.position = position;
                    return;
                }
            }

            nodeLayouts.Add(new NodeLayoutEntry { node = node, position = position });
        }

        void RemoveLayout(DialogueNode node)
        {
            if (nodeLayouts == null)
                return;

            nodeLayouts.RemoveAll(e => e.node == node);
        }

#if UNITY_EDITOR
        public void SetStartNode(DialogueNode node)
        {
            startNode = node;
        }
#endif
        #endregion
    }
}
