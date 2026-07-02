using System.Collections.Generic;
using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务运行时管理
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [SerializeField]
        List<QuestDef> questDefList = new();

        readonly Dictionary<int, QuestRuntimeState> stateDict = new();

        public static QuestManager Instance { get; private set; }

        #region 生命周期

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void OnEnable() => SubscribeEvents();

        void OnDisable() => UnsubscribeEvents();

        void Start()
        {
            RegisterQuestDefs(questDefList);
            RefreshAvailableQuests();

            foreach (var def in questDefList)
            {
                if (def != null && def.AutoAccept)
                    AcceptQuest(def.QuestId);
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 注册任务定义
        /// </summary>
        public void RegisterQuestDefs(IEnumerable<QuestDef> defs)
        {
            if (defs == null)
                return;

            foreach (var def in defs)
                RegisterQuestDef(def);
        }

        /// <summary>
        /// 注册单个任务定义
        /// </summary>
        public void RegisterQuestDef(QuestDef def)
        {
            if (def == null)
                return;

            if (!stateDict.ContainsKey(def.QuestId))
                stateDict[def.QuestId] = new QuestRuntimeState(def);
        }

        /// <summary>
        /// 接取任务
        /// </summary>
        public bool AcceptQuest(int questId)
        {
            if (!stateDict.TryGetValue(questId, out var state))
                return false;

            if (state.State != EQuestState.Available)
                return false;

            state.State = EQuestState.Active;
            Debug.Log($"[Quest] 接取 {state.Def.QuestTitle}");
            GameEventBus.Bus.TriggerEvent(GameEventKey.QuestAccepted, questId);
            return true;
        }

        /// <summary>
        /// 查询任务状态
        /// </summary>
        public EQuestState GetQuestState(int questId) =>
            stateDict.TryGetValue(questId, out var state) ? state.State : EQuestState.Inactive;

        /// <summary>
        /// 获取任务运行时状态
        /// </summary>
        public QuestRuntimeState GetRuntimeState(int questId) =>
            stateDict.TryGetValue(questId, out var state) ? state : null;

        /// <summary>
        /// 枚举所有已注册任务状态
        /// </summary>
        public IEnumerable<QuestRuntimeState> EnumerateStates() => stateDict.Values;

        #endregion

        #region 事件订阅

        void SubscribeEvents()
        {
            var bus = GameEventBus.Bus;
            bus.AddEventListener<DialogueFinishedPayload>(GameEventKey.DialogueGraphFinished, OnDialogueGraphFinished);
            bus.AddEventListener<EnemyKilledPayload>(GameEventKey.EnemyKilled, OnEnemyKilled);
            bus.AddEventListener<ItemCollectedPayload>(GameEventKey.ItemCollected, OnItemCollected);
            bus.AddEventListener<ZoneEnteredPayload>(GameEventKey.ZoneEntered, OnZoneEntered);
        }

        void UnsubscribeEvents()
        {
            var bus = GameEventBus.Bus;
            bus.RemoveListener<DialogueFinishedPayload>(GameEventKey.DialogueGraphFinished, OnDialogueGraphFinished);
            bus.RemoveListener<EnemyKilledPayload>(GameEventKey.EnemyKilled, OnEnemyKilled);
            bus.RemoveListener<ItemCollectedPayload>(GameEventKey.ItemCollected, OnItemCollected);
            bus.RemoveListener<ZoneEnteredPayload>(GameEventKey.ZoneEntered, OnZoneEntered);
        }

        void OnDialogueGraphFinished(DialogueFinishedPayload payload) =>
            AdvanceObjectives(EQuestObjectiveType.DialogueComplete, payload.graphId, null, 1);

        void OnEnemyKilled(EnemyKilledPayload payload) =>
            AdvanceObjectives(EQuestObjectiveType.Kill, 0, payload.enemyKey, payload.count > 0 ? payload.count : 1);

        void OnItemCollected(ItemCollectedPayload payload) =>
            AdvanceObjectives(EQuestObjectiveType.Collect, 0, payload.itemKey, payload.count > 0 ? payload.count : 1);

        void OnZoneEntered(ZoneEnteredPayload payload) =>
            AdvanceObjectives(EQuestObjectiveType.ReachZone, 0, payload.zoneKey, 1);

        #endregion

        #region 目标推进

        void RefreshAvailableQuests()
        {
            foreach (var pair in stateDict)
            {
                var state = pair.Value;
                if (state.State != EQuestState.Inactive)
                    continue;

                if (ArePrerequisitesMet(state.Def))
                    state.State = EQuestState.Available;
            }
        }

        bool ArePrerequisitesMet(QuestDef def)
        {
            if (def?.PrerequisiteQuestIdList == null)
                return true;

            foreach (int prerequisiteId in def.PrerequisiteQuestIdList)
            {
                if (GetQuestState(prerequisiteId) != EQuestState.Completed)
                    return false;
            }

            return true;
        }

        void AdvanceObjectives(EQuestObjectiveType type, int targetId, string targetKey, int delta)
        {
            foreach (var pair in stateDict)
            {
                var state = pair.Value;
                if (state.State != EQuestState.Active)
                    continue;

                TryAdvanceObjectives(state, type, targetId, targetKey, delta);
                TryCompleteQuest(state);
            }
        }

        void TryAdvanceObjectives(
            QuestRuntimeState state,
            EQuestObjectiveType type,
            int targetId,
            string targetKey,
            int delta)
        {
            var objectiveList = state.ObjectiveList;
            if (objectiveList == null)
                return;

            for (int i = 0; i < objectiveList.Count; i++)
            {
                var objective = objectiveList[i];
                if (objective == null || objective.objectiveType != type)
                    continue;

                if (!IsObjectiveTargetMatch(objective, targetId, targetKey))
                    continue;

                int required = objective.requiredCount > 0 ? objective.requiredCount : 1;
                if (state.ProgressList[i] >= required)
                    continue;

                state.ProgressList[i] = Mathf.Min(state.ProgressList[i] + delta, required);
                Debug.Log($"[Quest] {state.Def.QuestTitle} 目标{i + 1} 进度 {state.ProgressList[i]}/{required}");
            }
        }

        static bool IsObjectiveTargetMatch(QuestObjectiveDef objective, int targetId, string targetKey) =>
            objective.objectiveType switch
            {
                EQuestObjectiveType.DialogueComplete => objective.targetId == targetId,
                EQuestObjectiveType.Kill or EQuestObjectiveType.Collect or EQuestObjectiveType.ReachZone =>
                    !string.IsNullOrEmpty(objective.targetKey) && objective.targetKey == targetKey,
                _ => false,
            };

        void TryCompleteQuest(QuestRuntimeState state)
        {
            if (state.State != EQuestState.Active || !state.IsAllObjectiveDone())
                return;

            state.State = EQuestState.Completed;
            Debug.Log($"[Quest] 完成 {state.Def.QuestTitle}");
            GameEventBus.Bus.TriggerEvent(GameEventKey.QuestCompleted, state.Def.QuestId);
            RefreshAvailableQuests();
        }

        #endregion
    }
}
