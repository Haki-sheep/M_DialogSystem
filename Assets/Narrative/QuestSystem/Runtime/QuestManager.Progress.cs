using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  public partial class QuestManager
  {
    /// <summary>
    /// 创建运行时数据
    /// </summary>
    private void BuildRuntimeState()
    {
      stateDict.Clear();
      activeQuestList.Clear();

      foreach (var quest in questList)
      {
        if (quest == null) continue;

        if (stateDict.ContainsKey(quest.QuestId))
        {
          Debug.LogWarning($"[Quest] 任务ID重复 {quest.QuestId} 请检查 {quest.name}");
          continue;
        }

        stateDict[quest.QuestId] = new QuestRuntimeState(quest);
        if (stateDict[quest.QuestId].objectiveList.Count == 0)
          Debug.LogWarning($"[Quest] {quest.Title} 无目标 请在 objectiveList 添加步骤");
      }

      if (stateDict.Count == 0)
        Debug.LogWarning("[Quest] questList 为空 请拖入 TestKillSlimeQuest");
    }

    /// <summary>
    /// 接受系统派发任务
    /// </summary>
    private void AcceptSystemQuests()
    {
      foreach (var runtime in stateDict.Values)
      {
        if (runtime.eQuestState == EQuestState.可用 && runtime.quest.AcceptMode == EQuestAcceptMode.系统派发)
          Accept(runtime.quest.QuestId);
      }
    }

    /// <summary>
    /// 刷新可接任务
    /// </summary>
    private void RefreshAvailable()
    {
      foreach (var runtime in stateDict.Values)
      {
        if (runtime.eQuestState != EQuestState.未激活) continue;

        bool ready = true;
        foreach (int preId in runtime.quest.PrerequisiteIdList)
        {
          if (GetState(preId) != EQuestState.提交)
          {
            ready = false;
            break;
          }
        }

        if (ready) runtime.eQuestState = EQuestState.可用;
      }
    }

    /// <summary>
    /// 按Key推进执行中任务
    /// </summary>
    private void AdvanceByKey(EQuestObjectiveType type, string targetKey, int delta)
    {
      AdvanceMatchedObjectives(type, delta, objective => IsKeyMatch(objective, targetKey));
    }

    /// <summary>
    /// 按对话事件推进执行中任务
    /// </summary>
    private void AdvanceDialogue(DialogueGraph graph, string eventKey, int delta)
    {
      AdvanceMatchedObjectives(EQuestObjectiveType.对话, delta, objective => IsDialogueMatch(objective, graph, eventKey));
    }

    /// <summary>
    /// 推进匹配的任务目标
    /// </summary>
    private void AdvanceMatchedObjectives(EQuestObjectiveType type, int delta, System.Func<QuestObjective, bool> isMatch)
    {
      for (int q = 0; q < activeQuestList.Count; q++)
      {
        var runtime = activeQuestList[q];
        if (runtime.eQuestState != EQuestState.执行中) continue;

        for (int i = 0; i < runtime.objectiveList.Count; i++)
        {
          var objective = runtime.objectiveList[i];
          if (objective == null) continue;
          if (objective.type != type) continue;
          if (!isMatch(objective)) continue;

          int need = objective.count > 0 ? objective.count : 1;
          if (runtime.progressList[i] >= need) continue;

          int currentCount = Mathf.Min(runtime.progressList[i] + delta, need);
          runtime.progressList[i] = currentCount;
          NotifyProgressChanged(runtime, i, currentCount, need);
        }
      }
    }

    /// <summary>
    /// 判断Key是否匹配
    /// </summary>
    private static bool IsKeyMatch(QuestObjective objective, string targetKey)
    {
      return !string.IsNullOrEmpty(objective.targetKey) && objective.targetKey == targetKey;
    }

    /// <summary>
    /// 判断对话事件是否匹配
    /// </summary>
    private static bool IsDialogueMatch(QuestObjective objective, DialogueGraph graph, string eventKey)
    {
      return objective.dialogueGraph == graph && objective.dialogueEventKey == eventKey;
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    private void CompleteQuest(QuestRuntimeState runtime)
    {
      if (!runtime.AllDone()) return;
      if (runtime.eQuestState != EQuestState.执行中) return;

      CancelTimeLimit(runtime.quest.QuestId);
      RemoveActive(runtime);
      runtime.eQuestState = EQuestState.提交;

      Debug.Log($"[Quest] 提交 {runtime.quest.Title} (id={runtime.quest.QuestId})");
      NotifyCompleted(runtime);
      RefreshAvailable();
    }

    /// <summary>
    /// 移出执行列表
    /// </summary>
    private void RemoveActive(QuestRuntimeState runtime)
    {
      activeQuestList.Remove(runtime);
    }

    /// <summary>
    /// 通知接受任务
    /// </summary>
    private void NotifyAccepted(QuestRuntimeState runtime)
    {
      var context = CreateContext(runtime, -1, 0, 0);
      runtime.quest.OnAccepted(context);
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestAccepted, runtime.quest.QuestId);
    }

    /// <summary>
    /// 通知进度变化
    /// </summary>
    private void NotifyProgressChanged(QuestRuntimeState runtime, int objectiveIndex, int currentCount, int needCount)
    {
      var context = CreateContext(runtime, objectiveIndex, currentCount, needCount);
      runtime.quest.OnProgressChanged(context);

      var objective = runtime.objectiveList[objectiveIndex];
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestProgressChanged,
        new QuestProgressChangedEventData
        {
          questId = runtime.quest.QuestId,
          objectiveIndex = objectiveIndex,
          eObjectiveType = objective.type,
          currentCount = currentCount,
          needCount = needCount,
        });
    }

    /// <summary>
    /// 通知完成任务
    /// </summary>
    private void NotifyCompleted(QuestRuntimeState runtime)
    {
      var context = CreateContext(runtime, -1, 0, 0);
      runtime.quest.OnCompleted(context);
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestCompleted, runtime.quest.QuestId);
    }

    /// <summary>
    /// 通知任务失败
    /// </summary>
    private void NotifyFailed(QuestRuntimeState runtime)
    {
      var context = CreateContext(runtime, -1, 0, 0);
      runtime.quest.OnFailed(context);
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestFailed, runtime.quest.QuestId);
    }

    /// <summary>
    /// 创建生命周期上下文
    /// </summary>
    /// <param name="runtime">任务运行时数据</param>
    /// <param name="objectiveIndex">目标序号</param>
    /// <param name="currentCount">当前进度</param>
    /// <param name="needCount">目标数量</param>
    /// <returns>生命周期上下文</returns>
    private QuestLifeCycleContext CreateContext(QuestRuntimeState runtime,
                                                int objectiveIndex,
                                                int currentCount,
                                                int needCount)
    {
      return new QuestLifeCycleContext(
        runtime.quest,
        runtime.eQuestState,
        objectiveIndex,
        currentCount,
        needCount,
        GetRemainSeconds(runtime));
    }
  }
}
