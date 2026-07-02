using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务流程节点
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest Node", menuName = "Quest System/Quest Node")]
    public class QuestNode : ScriptableObject
    {
        /// <summary> 节点ID </summary>
        [SerializeField]
        private int nodeId;

        /// <summary> 节点类型 </summary>
        [SerializeField]
        private EQuestNodeType nodeType;

        /// <summary> 任务目标定义 </summary>
        [SerializeField]
        private QuestObjectiveDef objective = new();

        /// <summary> 任务流程跳转 </summary>
        [SerializeField]
        private QuestFlowTransition nextTransition = new();

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
