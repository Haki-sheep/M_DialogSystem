using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 普通对话节点
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialog Node", menuName = "Dialog System/Dialog Node")]
    public class DialogueNode : ScriptableObject
    {
        #region 字段
        /// <summary> 节点ID </summary>
        [SerializeField]
        private int nodeId;

        /// <summary> 说话类型 </summary>
        [SerializeField]
        private SpeakEnums speakType;

        /// <summary> 说话者名称 </summary>
        [SerializeField]
        private string speakerName;

        /// <summary> 对话文本 </summary>
        [SerializeField]
        private string dialogText;
        /// <summary> 是否是选项节点 </summary>
        [SerializeField]
        private bool isOptionNode;

        /// <summary> 普通节点下一跳 </summary>
        [HideInInspector]
        [SerializeField]
        private DialogueTransition nextTransition = new();

        /// <summary> 选项出口 </summary>
        [HideInInspector]
        [SerializeField]
        private List<DialogueTransition> choiceList = new();

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
                if (nextTransition == null)
                    nextTransition = new DialogueTransition();
                return nextTransition;
            }
        }
        public List<DialogueTransition> ChoiceList => choiceList;
        #endregion

        #region 方法
        /// <summary>
        /// 验证节点
        /// </summary>
        public void VaildNode()
        {
            // 如果节点是选项节点 则检查选项列表是否为空
            if (isOptionNode)
            {
                if (choiceList is null || choiceList.Count == 0)
                    Debug.LogError("ChoiceList is null or empty");
            }

            // 如果节点是普通节点 则检查下一节点是否为空
            if (NextTransition?.toNode == null)
            {
                Debug.LogWarning("Node is Over");
            }
        }

        /// <summary>
        /// 设置下一节点
        /// </summary>
        public void SetNextNode(DialogueNode node)
        {
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
        /// 添加选项节点
        /// </summary>
        public void AddChoice(DialogueTransition choice)
        {
            if (choiceList is null)
                choiceList = new List<DialogueTransition>();
            choiceList.Add(choice);
        }

        /// <summary>
        /// 移除选项
        /// </summary>
        public void RemoveChoice(DialogueTransition choice)
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
        public DialogueTransition GetChoice(int index)
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
        #endregion
    }
}
