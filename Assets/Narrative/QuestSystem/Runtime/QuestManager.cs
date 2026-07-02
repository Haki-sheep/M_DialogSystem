using System.Collections.Generic;
using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  public class QuestManager : MonoBehaviour
  {
    [Header("拖 Quest 例如 QuestSo/TestKillSlimeQuest")]
    [SerializeField] List<Quest> questList = new();

    readonly Dictionary<int, QuestState> states = new();

    public static QuestManager Instance { get; private set; }

    public IReadOnlyList<Quest> RegisteredQuests => questList;

    #region 生命周期

    void Awake()
    {
      if (Instance != null && Instance != this) { Destroy(gameObject); return; }
      Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable() => Subscribe(true);
    void OnDisable() => Subscribe(false);

    void Start()
    {
      foreach (var quest in questList)
      {
        if (quest == null) continue;
        states[quest.QuestId] = new QuestState(quest);
        if (states[quest.QuestId].objectives.Count == 0)
          Debug.LogWarning($"[Quest] {quest.Title} 无目标 请在 objectiveList 添加步骤");
      }
      if (states.Count == 0)
        Debug.LogWarning("[Quest] questList 为空 请拖入 TestKillSlimeQuest");

      RefreshAvailable();
      foreach (var s in states.Values)
        if (s.quest.AcceptMode == EQuestAcceptMode.系统派发) Accept(s.quest.QuestId);
    }

    void Update()
    {
      float now = Time.time;
      foreach (var s in states.Values)
      {
        if (s.state != EQuestState.执行中 || !s.quest.HasTimeLimit) continue;
        if (now - s.acceptedAt < s.quest.TimeLimit) continue;
        Fail(s.quest.QuestId);
      }
    }

    #endregion

    #region 公开接口

    public bool Accept(int questId)
    {
      if (!states.TryGetValue(questId, out var s)) return false;
      if (s.state != EQuestState.可用 && s.state != EQuestState.提交 && s.state != EQuestState.失败) return false;
      s.ResetProgress();
      s.state = EQuestState.执行中;
      s.acceptedAt = Time.time;
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestAccepted, questId);
      return true;
    }

    public bool Fail(int questId)
    {
      if (!states.TryGetValue(questId, out var s) || s.state != EQuestState.执行中) return false;
      s.state = EQuestState.失败;
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestFailed, questId);
      return true;
    }

    /// <summary>
    /// 提交任务 目标须全部完成
    /// </summary>
    public bool Submit(int questId) => TrySubmit(questId);

    /// <summary>
    /// 尝试提交 目标未做完则不变状态
    /// </summary>
    public bool TrySubmit(int questId)
    {
      if (!states.TryGetValue(questId, out var s) || s.state != EQuestState.执行中) return false;
      if (!s.AllDone()) return false;
      Resolve(s);
      return s.state == EQuestState.提交;
    }

    /// <summary>
    /// GM 直接提交 填满进度
    /// </summary>
    public bool GmSubmit(int questId)
    {
      if (!states.TryGetValue(questId, out var s) || s.state != EQuestState.执行中) return false;
      for (int i = 0; i < s.objectives.Count; i++)
      {
        if (s.objectives[i] == null) continue;
        s.progress[i] = s.objectives[i].count > 0 ? s.objectives[i].count : 1;
      }
      Resolve(s);
      return s.state == EQuestState.提交;
    }

    public EQuestState GetState(int questId) =>
      states.TryGetValue(questId, out var s) ? s.state : EQuestState.未激活;

    public int GetObjectiveCount(int questId) =>
      states.TryGetValue(questId, out var s) ? s.objectives.Count : 0;

    public int GetObjectiveProgress(int questId, int index)
    {
      if (!states.TryGetValue(questId, out var s)) return 0;
      if (index < 0 || index >= s.progress.Count) return 0;
      return s.progress[index];
    }

    #endregion

    #region 事件

    void Subscribe(bool on)
    {
      var bus = GameEventBus.Bus;
      if (on)
      {
        bus.AddEventListener<DialogueFinishedPayload>(GameEventKey.DialogueGraphFinished, OnDialogue);
        bus.AddEventListener<EnemyKilledPayload>(GameEventKey.EnemyKilled, OnKill);
        bus.AddEventListener<ItemCollectedPayload>(GameEventKey.ItemCollected, OnCollect);
        bus.AddEventListener<ZoneEnteredPayload>(GameEventKey.ZoneEntered, OnReach);
      }
      else
      {
        bus.RemoveListener<DialogueFinishedPayload>(GameEventKey.DialogueGraphFinished, OnDialogue);
        bus.RemoveListener<EnemyKilledPayload>(GameEventKey.EnemyKilled, OnKill);
        bus.RemoveListener<ItemCollectedPayload>(GameEventKey.ItemCollected, OnCollect);
        bus.RemoveListener<ZoneEnteredPayload>(GameEventKey.ZoneEntered, OnReach);
      }
    }

    void OnDialogue(DialogueFinishedPayload p) =>
      Advance(EQuestObjectiveType.对话, p.graphId, null, 1);

    void OnKill(EnemyKilledPayload p) =>
      Advance(EQuestObjectiveType.击杀, 0, p.enemyKey, p.count > 0 ? p.count : 1);

    void OnCollect(ItemCollectedPayload p) =>
      Advance(EQuestObjectiveType.收集, 0, p.itemKey, p.count > 0 ? p.count : 1);

    void OnReach(ZoneEnteredPayload p) =>
      Advance(EQuestObjectiveType.到达, 0, p.zoneKey, 1);

    #endregion

    #region 推进

    void RefreshAvailable()
    {
      foreach (var s in states.Values)
      {
        if (s.state != EQuestState.未激活) continue;
        bool ok = true;
        foreach (int id in s.quest.PrerequisiteIdList)
          if (GetState(id) != EQuestState.提交) { ok = false; break; }
        if (ok) s.state = EQuestState.可用;
      }
    }

    void Advance(EQuestObjectiveType type, int targetId, string targetKey, int delta)
    {
      foreach (var s in states.Values)
      {
        if (s.state != EQuestState.执行中) continue;

        for (int i = 0; i < s.objectives.Count; i++)
        {
          var o = s.objectives[i];
          if (o == null || o.type != type || !Match(o, targetId, targetKey)) continue;
          int need = o.count > 0 ? o.count : 1;
          if (s.progress[i] >= need) continue;
          s.progress[i] = Mathf.Min(s.progress[i] + delta, need);
        }
      }
    }

    static bool Match(QuestObjective o, int targetId, string targetKey) =>
      o.type == EQuestObjectiveType.对话
        ? o.targetId == targetId
        : !string.IsNullOrEmpty(o.targetKey) && o.targetKey == targetKey;

    void Resolve(QuestState s)
    {
      if (!s.AllDone()) return;
      if (s.state != EQuestState.执行中) return;
      s.state = EQuestState.提交;
      Debug.Log($"[Quest] 提交 {s.quest.Title} (id={s.quest.QuestId})");
      GameEventBus.Bus.TriggerEvent(GameEventKey.QuestCompleted, s.quest.QuestId);
      RefreshAvailable();
    }

    #endregion

    #region 运行时状态

    sealed class QuestState
    {
      public readonly Quest quest;
      public readonly List<QuestObjective> objectives;
      public readonly List<int> progress = new();
      public EQuestState state = EQuestState.未激活;
      public float acceptedAt;

      public QuestState(Quest quest)
      {
        this.quest = quest;
        objectives = new List<QuestObjective>(quest.GetObjectives());
        for (int i = 0; i < objectives.Count; i++) progress.Add(0);
      }

      public bool AllDone()
      {
        for (int i = 0; i < objectives.Count; i++)
        {
          if (objectives[i] == null) continue;
          int need = objectives[i].count > 0 ? objectives[i].count : 1;
          if (progress[i] < need) return false;
        }
        return true;
      }

      public void ResetProgress()
      {
        for (int i = 0; i < progress.Count; i++)
          progress[i] = 0;
      }
    }

    #endregion
  }
}
