using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务定义
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
    public class QuestDef : ScriptableObject
    {
        /// <summary> 任务ID </summary>
        [SerializeField]
        private int questId;

        /// <summary> 任务标题 </summary>
        [SerializeField]
        private string questTitle;

        /// <summary> 任务描述 </summary>
        [SerializeField, TextArea]
        private string questDescription;

        /// <summary> 所以属任务图 </summary>
        [SerializeField]
        private QuestGraph questGraph;

        /// <summary> 前置任务ID列表 </summary>
        [SerializeField]
        private List<int> prerequisiteQuestIdList = new();

        /// <summary> 是否自动接受 </summary>
        [SerializeField]
        private bool autoAccept;

        /// <summary> 回退目标列表 </summary>
        [SerializeField]
        private List<QuestObjectiveDef> fallbackObjectiveList = new();

        public int QuestId => questId;
        public string QuestTitle => questTitle;
        public string QuestDescription => questDescription;
        public QuestGraph QuestGraph => questGraph;
        public bool AutoAccept => autoAccept;
        public IReadOnlyList<int> PrerequisiteQuestIdList => prerequisiteQuestIdList;
        public IReadOnlyList<QuestObjectiveDef> FallbackObjectiveList => fallbackObjectiveList;
    }
}
