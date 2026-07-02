using System.Collections.Generic;

using UnityEngine;



namespace Miemie.DialogSystem.Quest

{

  [CreateAssetMenu(fileName = "Quest", menuName = "Quest System/Quest")]

  public class Quest : ScriptableObject

  {

    /// <summary> 任务ID </summary>

    [SerializeField] int questId;

    /// <summary> 任务分类 </summary>

    [SerializeField] EQuestCategory category = EQuestCategory.支线任务;

    /// <summary> 任务标题 </summary>

    [SerializeField] string title;

    /// <summary> 任务描述 </summary>

    [SerializeField, TextArea] string description;

    /// <summary> 任务接受模式 </summary>

    [SerializeField] EQuestAcceptMode acceptMode = EQuestAcceptMode.手动接受;

    /// <summary> 任务时间限制 </summary>

    [SerializeField] float timeLimit;

    /// <summary> 任务目标列表 顺序即流程 </summary>

    [SerializeField] List<QuestObjective> objectiveList = new();

    /// <summary> 前置任务ID列表 </summary>

    [SerializeField] List<int> prerequisiteIdList = new();


    // 属性
    public int QuestId => questId;
    public EQuestCategory Category => category;
    public string Title => title;
    public string Description => description;
    public EQuestAcceptMode AcceptMode => acceptMode;
    public float TimeLimit => timeLimit;
    public IReadOnlyList<int> PrerequisiteIdList => prerequisiteIdList;
    public bool HasTimeLimit => timeLimit > 0f;


    public IReadOnlyList<QuestObjective> GetObjectives() => objectiveList;

  }

}

