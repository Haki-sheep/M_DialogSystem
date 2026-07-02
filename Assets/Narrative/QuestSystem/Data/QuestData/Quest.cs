using System.Collections.Generic;

using UnityEngine;



namespace Miemie.DialogSystem.Quest
{

  [CreateAssetMenu(fileName = "Quest", menuName = "Quest System/Quest")]

  public class Quest : ScriptableObject, IQuestLifeCycle
  {

    /// <summary>
    /// 任务ID
    /// </summary>
    [SerializeField]
    private int questId;

    /// <summary>
    /// 任务分类
    /// </summary>
    [SerializeField]
    private EQuestCategory category = EQuestCategory.支线任务;

    /// <summary>
    /// 任务标题
    /// </summary>
    [SerializeField]
    private string title;

    /// <summary>
    /// 任务描述
    /// </summary>
    [SerializeField]
    [TextArea]
    private string description;

    /// <summary>
    /// 任务接受模式
    /// </summary>
    [SerializeField]
    private EQuestAcceptMode acceptMode = EQuestAcceptMode.手动接受;

    /// <summary>
    /// 任务时间限制
    /// </summary>
    [SerializeField]
    private float timeLimit;

    /// <summary>
    /// 任务目标组
    /// </summary>
    [SerializeField]
    private List<QuestObjective> objectiveList = new();

    /// <summary>
    /// 前置任务ID列表
    /// </summary>
    [SerializeField]
    private List<int> prerequisiteIdList = new();


    // 属性
    public int QuestId => questId;
    public EQuestCategory Category => category;
    public string Title => title;
    public string Description => description;
    public EQuestAcceptMode AcceptMode => acceptMode;
    public float TimeLimit => timeLimit;
    public IReadOnlyList<int> PrerequisiteIdList => prerequisiteIdList;
    public bool HasTimeLimit => timeLimit > 0f;


    /// <summary>
    /// 获取目标列表
    /// </summary>
    public IReadOnlyList<QuestObjective> GetObjectives() => objectiveList;

    /// <summary>
    /// 接受任务
    /// </summary>
    public virtual void OnAccepted(QuestLifeCycleContext context)
    {
    }

    /// <summary>
    /// 任务进度变化
    /// </summary>
    public virtual void OnProgressChanged(QuestLifeCycleContext context)
    {
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    public virtual void OnCompleted(QuestLifeCycleContext context)
    {
    }

    /// <summary>
    /// 任务失败
    /// </summary>
    public virtual void OnFailed(QuestLifeCycleContext context)
    {
    }

  }

}

