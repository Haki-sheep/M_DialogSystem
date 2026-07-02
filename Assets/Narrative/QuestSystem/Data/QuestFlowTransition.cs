using System;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务流程跳转
    /// 用于指定任务流程的跳转目标
    /// </summary>
    [Serializable]
    public class QuestFlowTransition
    {
        public QuestNode toNode;
    }
}
