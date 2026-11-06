using HarmonyLib;
using UnityEngine;

namespace UndergroundGunsmith
{
    /// <summary>
    /// 「Wild 的人脉」里三条暗号升级使用与「退休枪匠」相同的图标。
    ///
    /// 不能用 <c>iconKey</c> 指到原版图标名：<c>CustomUIManager.GetSprite</c> 找不到键时会回退到
    /// "default" 图并打警告，而 <c>iconKey</c> 非空时不会再回落到 <c>frame.FindIcon(id)</c>。
    /// 因此这里在原版图标解析完成后直接取原版图集里的 <c>RETIRED_GUNSMITH</c> 精灵图。
    /// </summary>
    [HarmonyPatch(typeof(WildNetworkUI), "ResolveIcon")]
    internal static class WildNetworkCipherIconPatch
    {
        private const string VanillaIconName = "RETIRED_GUNSMITH";

        private static void Postfix(string id, ref Sprite __result, WildNetworkFrame ___frame)
        {
            if (!UndergroundCipher.IsCipherUpgradeId(id) || ___frame == null)
            {
                return;
            }

            Sprite icon = ___frame.FindIcon(VanillaIconName);
            if (icon != null)
            {
                __result = icon;
            }
        }
    }
}
