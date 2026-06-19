namespace Miemie.DialogSystem
{
    /// <summary>
    /// 条件类型枚举
    /// 仿照了Aniamtor中的状态机 不过我删除了Trigger类型 因为对话跳转通常不需要触发器
    /// </summary>
    public enum ECondition
    {
        None = 0,

        BoolTrue = 1,
        BoolFalse = 2,

        /// <summary>
        /// 浮点数因为比较不精确 所以只有比大小 没有等于 不等于
        /// </summary>
        FloatGreater = 10,
        FloatLess = 11,

        IntGreater = 20,
        IntLess = 21,
        IntEquals = 22,
        IntNotEquals = 23,
    }
}
