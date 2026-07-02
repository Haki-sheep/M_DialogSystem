using System;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 任务目标定义
  /// </summary>
  [Serializable]
  public class QuestObjective
  {
    /// <summary> 任务目标类型 </summary>
    public EQuestObjectiveType type;
    /// <summary> 目标ID </summary>
    public int targetId;
    /// <summary> 匹配Key </summary>
    public string targetKey;
    /// <summary> 目标数量 </summary>
    public int count = 1;
    /// <summary> 目标描述 </summary>
    public string description;
  }
}
