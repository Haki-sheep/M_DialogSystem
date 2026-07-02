#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话资产重命名
    /// </summary>
    static class DialogueGraphAssetRenameUtility
    {
        public static bool Apply(Object asset, string newName, ref string renameBuffer)
        {
            if (asset == null || string.IsNullOrWhiteSpace(newName) || newName == asset.name)
                return false;

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return false;

            string error = AssetDatabase.RenameAsset(path, newName.Trim());
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(error);
                renameBuffer = asset.name;
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }
    }
}
#endif
