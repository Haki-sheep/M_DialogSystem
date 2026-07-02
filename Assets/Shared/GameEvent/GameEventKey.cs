namespace Miemie.DialogSystem
{
    /// <summary>
    /// 全局游戏事件键
    /// </summary>
    public enum GameEventKey
    {
        // 对话事件触发
        DialogueEventTriggered,
        // 敌人击杀
        EnemyKilled,
        // 物品收集
        ItemCollected,
        // 区域进入
        ZoneEntered,
        // 任务接受
        QuestAccepted,
        // 任务进度变化
        QuestProgressChanged,
        // 任务完成
        QuestCompleted,
        QuestFailed,
    }
}
