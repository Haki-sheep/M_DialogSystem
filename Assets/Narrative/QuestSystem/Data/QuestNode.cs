using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务流程节点
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest Node", menuName = "Quest System/Quest Node")]
    public class QuestNode : ScriptableObject
    {
        [SerializeField]
        int nodeId;

        [SerializeField]
        EQuestNodeType nodeType;

        [SerializeField]
        QuestObjectiveDef objective = new();

        [SerializeField]
        QuestFlowTransition nextTransition = new();

        public int NodeId => nodeId;
        public EQuestNodeType NodeType => nodeType;
        public QuestObjectiveDef Objective => objective;
        public QuestFlowTransition NextTransition => nextTransition;

#if UNITY_EDITOR
        #region 编辑

        public void SetNodeIdInEditor(int id) => nodeId = id;

        public void SetNodeTypeInEditor(EQuestNodeType type) => nodeType = type;

        #endregion
#endif
    }
}
