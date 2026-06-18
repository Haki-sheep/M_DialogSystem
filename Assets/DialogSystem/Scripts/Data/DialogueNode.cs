using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    [CreateAssetMenu(fileName = "New Dialog Node", menuName = "Dialog System/Dialog Node")]
    public class DialogueNode : ScriptableObject
    {
        #region 字段
        /// <summary> 节点ID </summary>
        [SerializeField]
        private int nodeId;
        /// <summary> 说话类型 </summary>
        [SerializeField] private SpeakEnums speakType;
        /// <summary> 说话者名称 </summary>
        [SerializeField] private string speakerName;
        /// <summary> 对话文本 </summary>
        [SerializeField] private string dialogText;

        /// <summary> 是否是选项节点 </summary>
        [SerializeField] private bool isOptionNode;

        /// <summary> 普通节点下一跳 </summary>
        [HideInInspector][SerializeField]
        private DialogueTransition nextTransition = new();

        /// <summary> 旧版连线列表 仅迁移用 </summary>
        [HideInInspector][SerializeField]
        private List<DialogueLink> linkList = new();

        /// <summary> 选项出口 </summary>
        [HideInInspector][SerializeField]
        private List<DialogueChoice> choiceList = new();
        #endregion

        #region 属性
        public int NodeId => nodeId;
        public SpeakEnums SpeakType => speakType;
        public string SpeakerName => speakerName;
        public string DialogText { get => dialogText; set => dialogText = value; }
        public bool IsOptionNode { get => isOptionNode; set => isOptionNode = value; }
        public DialogueTransition NextTransition
        {
            get
            {
                EnsureNextTransitionMigrated();
                return nextTransition;
            }
        }
        public List<DialogueChoice> ChoiceList => choiceList;
        #endregion

        #region 方法
        /// <summary>
        /// 验证节点
        /// </summary>
        public void VaildNode()
        {
            if (isOptionNode)
            {
                if (choiceList is null || choiceList.Count == 0)
                    Debug.LogError("ChoiceList is null or empty");
            }
            else if (NextTransition?.toNode == null)
            {
                Debug.LogWarning("Node is Over");
            }
        }

        /// <summary>
        /// 设置下一节点
        /// </summary>
        public void SetNextNode(DialogueNode node)
        {
            EnsureNextTransitionMigrated();
            if (nextTransition == null)
                nextTransition = new DialogueTransition();
            nextTransition.toNode = node;
        }

        /// <summary>
        /// 清除下一节点
        /// </summary>
        public void ClearNextNode()
        {
            if (nextTransition != null)
                nextTransition.toNode = null;
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        public void AddChoice(DialogueChoice choice)
        {
            if (choiceList is null)
                choiceList = new List<DialogueChoice>();
            choiceList.Add(choice);
        }

        /// <summary>
        /// 移除选项
        /// </summary>
        public void RemoveChoice(DialogueChoice choice)
        {
            if (choiceList is not null)
                choiceList.Remove(choice);
        }

        /// <summary>
        /// 从末尾删除一个选项 至少保留一个
        /// </summary>
        public bool TryRemoveLastChoice()
        {
            if (choiceList is null || choiceList.Count <= 1)
                return false;

            choiceList.RemoveAt(choiceList.Count - 1);
            return true;
        }

        /// <summary>
        /// 获取选项
        /// </summary>
        public DialogueChoice GetChoice(int index)
        {
            if (choiceList is not null && index >= 0 && index < choiceList.Count)
                return choiceList[index];
            return null;
        }

        /// <summary>
        /// 清空选项
        /// </summary>
        public void ClearChoices()
        {
            if (choiceList is not null)
                choiceList.Clear();
        }

        /// <summary>
        /// 播放节点
        /// </summary>
        public void PlayNode()
        {
            Debug.Log($"[{nodeId}] {speakerName}: {dialogText}");
        }

        void EnsureNextTransitionMigrated()
        {
            if (nextTransition == null)
                nextTransition = new DialogueTransition();

            if (nextTransition.toNode != null || linkList == null || linkList.Count == 0)
                return;

            var legacy = linkList[0];
            if (legacy == null)
                return;

            nextTransition.toNode = legacy.toNode;
            foreach (var condition in legacy.GetEffectiveConditions())
            {
                if (condition != null && !condition.NoneContion)
                    nextTransition.Conditions.Add(condition);
            }
        }
        #endregion
    }
}
