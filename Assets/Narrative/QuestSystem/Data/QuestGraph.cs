using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务流程图
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest Graph", menuName = "Quest System/Quest Graph")]
    public class QuestGraph : ScriptableObject
    {
        [SerializeField]
        int questGraphId;

        [SerializeField]
        QuestDef questDef;

        [SerializeField]
        QuestNode startNode;

        [SerializeField]
        List<QuestNode> nodeList = new();

        public int QuestGraphId => questGraphId;
        public QuestDef QuestDef => questDef;
        public QuestNode StartNode => startNode;
        public List<QuestNode> NodeList => nodeList;

        #region 编辑

        public void AddNode(QuestNode node)
        {
            if (nodeList == null)
                nodeList = new List<QuestNode>();
            nodeList.Add(node);
        }

#if UNITY_EDITOR
        public void SetStartNodeInEditor(QuestNode node) => startNode = node;
#endif

        #endregion
    }
}
