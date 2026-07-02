using System;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务流程跳转
    /// </summary>
    [Serializable]
    public class QuestFlowTransition
    {
        public QuestNode toNode;
    }
}
