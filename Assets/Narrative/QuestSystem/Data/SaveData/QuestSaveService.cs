using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 任务存档读写
  /// </summary>
  public static class QuestSaveService
  {
    /// <summary>
    /// 存档文件名
    /// </summary>
    private const string SaveFileName = "quest_save.json";

    /// <summary>
    /// 存档路径
    /// </summary>
    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary>
    /// 保存任务数据
    /// </summary>
    public static void Save(QuestSaveFile saveFile)
    {
      string folderPath = Path.GetDirectoryName(SavePath);
      if (!string.IsNullOrEmpty(folderPath))
        Directory.CreateDirectory(folderPath);

      string json = JsonConvert.SerializeObject(saveFile, Formatting.Indented);
      File.WriteAllText(SavePath, json);
    }

    /// <summary>
    /// 读取任务数据
    /// </summary>
    public static QuestSaveFile Load()
    {
      if (!File.Exists(SavePath))
        return null;

      string json = File.ReadAllText(SavePath);
      return JsonConvert.DeserializeObject<QuestSaveFile>(json);
    }

    /// <summary>
    /// 清空任务存档
    /// </summary>
    public static void Clear()
    {
      if (File.Exists(SavePath))
        File.Delete(SavePath);
    }
  }
}
