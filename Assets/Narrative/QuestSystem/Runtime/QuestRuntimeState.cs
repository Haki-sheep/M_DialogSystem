using System.Collections.Generic;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 单任务运行时状态
    /// </summary>
    public class QuestRuntimeState
    {
        public QuestDef Def { get; }
        public EQuestState State { get; set; }
        public IReadOnlyList<QuestObjectiveDef> ObjectiveList { get; }
        public List<int> ProgressList { get; } = new();

        public QuestRuntimeState(QuestDef def)
        {
            Def = def;
            State = EQuestState.Inactive;

            var objectives = QuestGraphUtility.ResolveObjectives(def);
            ObjectiveList = objectives;

            for (int i = 0; i < objectives.Count; i++)
                ProgressList.Add(0);
        }

        #region 查询

        /// <summary>
        /// 是否所有目标已达成
        /// </summary>
        public bool IsAllObjectiveDone()
        {
            if (ObjectiveList == null || ObjectiveList.Count == 0)
                return true;

            for (int i = 0; i < ObjectiveList.Count; i++)
            {
                var objective = ObjectiveList[i];
                if (objective == null)
                    continue;

                int required = objective.requiredCount > 0 ? objective.requiredCount : 1;
                if (ProgressList[i] < required)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取目标显示文案
        /// </summary>
        public string GetObjectiveLabel(int index)
        {
            if (ObjectiveList == null || index < 0 || index >= ObjectiveList.Count)
                return string.Empty;

            var objective = ObjectiveList[index];
            if (objective == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(objective.description))
                return objective.description;

            return objective.objectiveType switch
            {
                EQuestObjectiveType.Kill => $"击杀 {objective.targetKey}",
                EQuestObjectiveType.Collect => $"收集 {objective.targetKey}",
                EQuestObjectiveType.ReachZone => $"到达 {objective.targetKey}",
                EQuestObjectiveType.DialogueComplete => $"完成对话 {objective.targetId}",
                _ => objective.objectiveType.ToString(),
            };
        }

        #endregion
    }
}
