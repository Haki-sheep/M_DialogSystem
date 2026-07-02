using System.Collections.Generic;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 单条任务运行时数据
  /// </summary>
  public class QuestRuntimeState
  {
    /// <summary> 任务 </summary>
    public readonly Quest quest;

    /// <summary> 目标列表 </summary>
    public readonly List<QuestObjective> objectiveList;

    /// <summary> 进度列表 </summary>
    public readonly List<int> progressList = new();

    /// <summary> 当前状态 </summary>
    public EQuestState eQuestState = EQuestState.未激活;

    /// <summary> 接取时间 </summary>
    public float acceptedAt;

    /// <summary>
    /// 限时结束时间
    /// </summary>
    public float timeLimitEndAt;

    /// <summary>
    /// 创建运行时数据
    /// </summary>
    public QuestRuntimeState(Quest quest)
    {
      this.quest = quest;
      objectiveList = new List<QuestObjective>(quest.GetObjectives());
      for (int i = 0; i < objectiveList.Count; i++)
        progressList.Add(0);
    }

    /// <summary>
    /// 目标是否全部完成
    /// </summary>
    public bool AllDone()
    {
      for (int i = 0; i < objectiveList.Count; i++)
      {
        if (objectiveList[i] == null) continue;
        int need = objectiveList[i].count > 0 ? objectiveList[i].count : 1;
        if (progressList[i] < need) return false;
      }
      return true;
    }

    /// <summary>
    /// 重置进度
    /// </summary>
    public void ResetProgress()
    {
      for (int i = 0; i < progressList.Count; i++)
        progressList[i] = 0;
    }

    /// <summary>
    /// 写入存档数据
    /// </summary>
    public QuestSaveData ToSaveData(float remainSeconds)
    {
      return new QuestSaveData
      {
        questId = quest.QuestId,
        eQuestState = eQuestState,
        progressList = new List<int>(progressList),
        acceptedAt = acceptedAt,
        remainSeconds = remainSeconds,
      };
    }

    /// <summary>
    /// 读取存档数据
    /// </summary>
    public void ApplySaveData(QuestSaveData saveData)
    {
      eQuestState = saveData.eQuestState;
      acceptedAt = saveData.acceptedAt;

      for (int i = 0; i < progressList.Count; i++)
      {
        if (saveData.progressList == null || i >= saveData.progressList.Count)
        {
          progressList[i] = 0;
          continue;
        }

        progressList[i] = saveData.progressList[i];
      }
    }
  }
}
