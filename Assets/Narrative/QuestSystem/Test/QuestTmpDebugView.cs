using System.Text;
using Miemie.DialogSystem;
using TMPro;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// TMP 任务调试视图
  /// </summary>
  public class QuestTmpDebugView : MonoBehaviour
  {
    /// <summary>
    /// 任务文本
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI questText;

    /// <summary>
    /// 文本构建器
    /// </summary>
    private readonly StringBuilder stringBuilder = new();

    /// <summary>
    /// 启用监听
    /// </summary>
    private void OnEnable()
    {
      GameEventBus.Bus.AddEventListener<int>(GameEventKey.QuestAccepted, OnQuestChanged);
      GameEventBus.Bus.AddEventListener<QuestProgressChangedEventData>(GameEventKey.QuestProgressChanged, OnQuestProgressChanged);
      GameEventBus.Bus.AddEventListener<int>(GameEventKey.QuestCompleted, OnQuestChanged);
      GameEventBus.Bus.AddEventListener<int>(GameEventKey.QuestFailed, OnQuestChanged);
      RefreshView();
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    private void OnDisable()
    {
      GameEventBus.Bus.RemoveListener<int>(GameEventKey.QuestAccepted, OnQuestChanged);
      GameEventBus.Bus.RemoveListener<QuestProgressChangedEventData>(GameEventKey.QuestProgressChanged, OnQuestProgressChanged);
      GameEventBus.Bus.RemoveListener<int>(GameEventKey.QuestCompleted, OnQuestChanged);
      GameEventBus.Bus.RemoveListener<int>(GameEventKey.QuestFailed, OnQuestChanged);
    }

    /// <summary>
    /// 任务变化
    /// </summary>
    private void OnQuestChanged(int questId)
    {
      RefreshView();
    }

    /// <summary>
    /// 任务进度变化
    /// </summary>
    private void OnQuestProgressChanged(QuestProgressChangedEventData eventData)
    {
      RefreshView();
    }

    /// <summary>
    /// 刷新视图
    /// </summary>
    public void RefreshView()
    {
      if (questText == null) return;

      var manager = QuestManager.Instance;
      if (manager == null)
      {
        questText.text = "QuestManager 未找到";
        return;
      }

      stringBuilder.Clear();
      foreach (var quest in manager.RegisteredQuests)
      {
        if (quest == null) continue;
        AppendQuest(manager, quest);
      }

      questText.text = stringBuilder.Length > 0 ? stringBuilder.ToString() : "没有任务";
    }

    /// <summary>
    /// 添加任务文本
    /// </summary>
    private void AppendQuest(QuestManager manager, Quest quest)
    {
      var eQuestState = manager.GetState(quest.QuestId);
      stringBuilder.Append('[');
      stringBuilder.Append(eQuestState);
      stringBuilder.Append("] ");
      stringBuilder.Append(quest.Title);
      stringBuilder.AppendLine();

      var objectiveList = quest.GetObjectives();
      for (int i = 0; i < objectiveList.Count; i++)
      {
        var objective = objectiveList[i];
        if (objective == null) continue;

        int currentCount = manager.GetObjectiveProgress(quest.QuestId, i);
        int needCount = objective.count > 0 ? objective.count : 1;
        stringBuilder.Append("  - ");
        stringBuilder.Append(ObjectiveText(objective));
        stringBuilder.Append(' ');
        stringBuilder.Append(currentCount);
        stringBuilder.Append('/');
        stringBuilder.Append(needCount);
        stringBuilder.AppendLine();
      }

      stringBuilder.AppendLine();
    }

    /// <summary>
    /// 目标显示文本
    /// </summary>
    private static string ObjectiveText(QuestObjective objective)
    {
      if (!string.IsNullOrEmpty(objective.description))
        return objective.description;

      if (objective.type == EQuestObjectiveType.对话)
      {
        string graphName = objective.dialogueGraph != null ? objective.dialogueGraph.name : "未选择对话图";
        return $"{objective.type} {graphName} {objective.dialogueEventKey}";
      }

      return $"{objective.type} {objective.targetKey}";
    }
  }
}
