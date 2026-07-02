using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务定义
    /// 有 QuestGraph 时以图内 Objective 节点为准
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
    public class QuestDef : ScriptableObject
    {
        [SerializeField]
        int questId;

        [SerializeField]
        string questTitle;

        [SerializeField, TextArea]
        string questDescription;

        [SerializeField]
        QuestGraph questGraph;

        [SerializeField]
        List<int> prerequisiteQuestIdList = new();

        [SerializeField]
        bool autoAccept;

        [SerializeField]
        List<QuestObjectiveDef> fallbackObjectiveList = new();

        public int QuestId => questId;
        public string QuestTitle => questTitle;
        public string QuestDescription => questDescription;
        public QuestGraph QuestGraph => questGraph;
        public bool AutoAccept => autoAccept;
        public IReadOnlyList<int> PrerequisiteQuestIdList => prerequisiteQuestIdList;
        public IReadOnlyList<QuestObjectiveDef> FallbackObjectiveList => fallbackObjectiveList;
    }
}
