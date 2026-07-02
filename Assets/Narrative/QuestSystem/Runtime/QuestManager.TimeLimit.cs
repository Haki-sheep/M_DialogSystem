using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  public partial class QuestManager
  {
    /// <summary>
    /// 查询任务剩余时间
    /// </summary>
    public float GetQuestRemainSeconds(int questId)
    {
      if (!stateDict.TryGetValue(questId, out var runtime)) return 0f;
      return GetRemainSeconds(runtime);
    }

    /// <summary>
    /// 启动限时任务
    /// </summary>
    private void StartTimeLimit(int questId, float seconds)
    {
      if (seconds <= 0f)
      {
        Fail(questId);
        return;
      }

      CancelTimeLimit(questId);
      var cancelSource = new CancellationTokenSource();
      timeLimitDict[questId] = cancelSource;

      if (stateDict.TryGetValue(questId, out var runtime))
        runtime.timeLimitEndAt = Time.time + seconds;

      RunTimeLimitAsync(questId, seconds, cancelSource).Forget();
    }

    /// <summary>
    /// 取消限时任务
    /// </summary>
    private void CancelTimeLimit(int questId)
    {
      if (!timeLimitDict.TryGetValue(questId, out var cancelSource)) return;
      cancelSource.Cancel();
      cancelSource.Dispose();
      timeLimitDict.Remove(questId);
    }

    /// <summary>
    /// 停止全部限时任务
    /// </summary>
    private void StopAllTimeLimit()
    {
      foreach (var pair in timeLimitDict)
      {
        pair.Value.Cancel();
        pair.Value.Dispose();
      }
      timeLimitDict.Clear();
    }

    /// <summary>
    /// 限时到期
    /// </summary>
    private async UniTaskVoid RunTimeLimitAsync(int questId, float seconds, CancellationTokenSource cancelSource)
    {
      bool isCanceled = await UniTask
        .Delay((int)(seconds * 1000f), cancellationToken: cancelSource.Token)
        .SuppressCancellationThrow();

      if (isCanceled) return;

      timeLimitDict.Remove(questId);
      cancelSource.Dispose();
      Fail(questId);
    }

    /// <summary>
    /// 查询剩余时间
    /// </summary>
    private float GetRemainSeconds(QuestRuntimeState runtime)
    {
      if (runtime.eQuestState != EQuestState.执行中) return 0f;
      if (!runtime.quest.HasTimeLimit) return 0f;
      return Mathf.Max(0f, runtime.timeLimitEndAt - Time.time);
    }
  }
}
