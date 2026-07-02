using System.Collections.Generic;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 从任务图解析目标列表
    /// </summary>
    public static class QuestGraphUtility
    {
        /// <summary>
        /// 沿 Start 出口顺序收集 Objective 节点
        /// </summary>
        public static List<QuestObjectiveDef> CollectObjectives(QuestGraph graph)
        {
            var result = new List<QuestObjectiveDef>();
            if (graph == null)
                return result;

            if (graph.StartNode != null)
            {
                CollectAlongChain(graph.StartNode, result, new HashSet<QuestNode>());
                return result;
            }

            if (graph.NodeList == null)
                return result;

            foreach (var node in graph.NodeList)
            {
                if (node != null && node.NodeType == EQuestNodeType.Objective)
                    result.Add(node.Objective);
            }

            return result;
        }

        static void CollectAlongChain(QuestNode node, List<QuestObjectiveDef> result, HashSet<QuestNode> visited)
        {
            while (node != null && visited.Add(node))
            {
                if (node.NodeType == EQuestNodeType.Objective)
                    result.Add(node.Objective);

                if (node.NodeType == EQuestNodeType.End)
                    break;

                node = node.NextTransition?.toNode;
            }
        }

        /// <summary>
        /// 解析任务使用的目标列表 图优先
        /// </summary>
        public static List<QuestObjectiveDef> ResolveObjectives(QuestDef def)
        {
            if (def == null)
                return new List<QuestObjectiveDef>();

            if (def.QuestGraph != null)
            {
                var fromGraph = CollectObjectives(def.QuestGraph);
                if (fromGraph.Count > 0)
                    return fromGraph;
            }

            var fallback = new List<QuestObjectiveDef>();
            if (def.FallbackObjectiveList == null)
                return fallback;

            foreach (var objective in def.FallbackObjectiveList)
            {
                if (objective != null)
                    fallback.Add(objective);
            }

            return fallback;
        }
    }
}
