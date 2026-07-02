using MiMieEventBus;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 全局事件总线入口
    /// </summary>
    public static class GameEventBus
    {
        public static EventBus<GameEventKey> Bus { get; } = new();
    }
}
