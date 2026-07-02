using System;
using System.Collections.Generic;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 任务存档文件
  /// </summary>
  [Serializable]
  public class QuestSaveFile
  {
    /// <summary>
    /// 任务存档列表
    /// </summary>
    public List<QuestSaveData> questDataList = new();
  }

  /// <summary>
  /// 单条任务存档
  /// </summary>
  [Serializable]
  public class QuestSaveData
  {
    /// <summary>
    /// 任务ID
    /// </summary>
    public int questId;

    /// <summary>
    /// 任务状态
    /// </summary>
    public EQuestState eQuestState;

    /// <summary>
    /// 目标进度列表
    /// </summary>
    public List<int> progressList = new();

    /// <summary>
    /// 接取时间
    /// </summary>
    public float acceptedAt;

    /// <summary>
    /// 剩余限时
    /// </summary>
    public float remainSeconds;
  }
}
