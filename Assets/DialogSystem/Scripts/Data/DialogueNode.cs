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
        /// <summary> 非选项则直接跳转到下一个节点 </summary>
        [ShowIf("@!isOptionNode")][SerializeField] 
        private List<DialogueLink> linkList = new();
        /// <summary> 选项节点列表 </summary>
        [ShowIf("@isOptionNode")][SerializeField] 
        private List<DialogueChoice> choiceList = new();

        #endregion

        #region 属性
        public int NodeId => nodeId;
        public SpeakEnums SpeakType => speakType;
        public string SpeakerName => speakerName;
        public string DialogText { get => dialogText; set => dialogText = value; }
        public bool IsOptionNode { get => isOptionNode; set => isOptionNode = value; }
        public List<DialogueLink> LinkList => linkList;
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
                {
                    Debug.LogError("ChoiceList is null or empty");
                }
            }
            else if (linkList is null || linkList.Count == 0)
            {
                Debug.LogWarning("Node is Over");
            }
        }

        /// <summary>
        /// 添加连线
        /// </summary>
        /// <param name="link"></param>
        public void AddLink(DialogueLink link)
        {
            if (linkList is null)
            {
                linkList = new List<DialogueLink>();
            }
            linkList.Add(link);
        }

        /// <summary>
        /// 移除连线
        /// </summary>
        /// <param name="link"></param>
        public void RemoveLink(DialogueLink link)
        {
            if (linkList is not null)
            {
                linkList.Remove(link);
            }
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        /// <param name="choice"></param>
        public void AddChoice(DialogueChoice choice)
        {
            if (choiceList is null)
            {
                choiceList = new List<DialogueChoice>();
            }
            choiceList.Add(choice);
        }

        /// <summary>
        /// 移除选项
        /// </summary>
        /// <param name="choice"></param>
        public void RemoveChoice(DialogueChoice choice)
        {
            if (choiceList is not null)
            {
                choiceList.Remove(choice);
            }
        }

        /// <summary>
        /// 获取连线
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DialogueLink GetLink(int index)
        {
            if (linkList is not null && index >= 0 && index < linkList.Count)
            {
                return linkList[index];
            }
            return null;
        }

        /// <summary>
        /// 获取选项
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DialogueChoice GetChoice(int index)
        {
            if (choiceList is not null && index >= 0 && index < choiceList.Count)
            {
                return choiceList[index];
            }
            return null;
        }

        /// <summary>
        /// 清空连线
        /// </summary>
        public void ClearLinks()
        {
            if (linkList is not null)
            {
                linkList.Clear();
            }
        }
        
        /// <summary>
        /// 清空选项
        /// </summary>
        public void ClearChoices()
        {
            if (choiceList is not null)
            {
                choiceList.Clear();
            }
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