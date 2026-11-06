using System.Collections.Generic;
using HarmonyLib;

namespace UndergroundGunsmith
{
    /// <summary>
    /// 改写原版"展示柜顾客预测"里的枪械买家条目：
    /// 对每个等级 N，只要展示柜里有原版 <c>permit_gun_N</c> 或地下暗号 N，就生成恰好一条该级买家条目。
    /// 概率、欠访补偿、市场行情全部沿用原版 <see cref="ShowcaseHelper.GetGunBuyerChance"/>，
    /// 其余（生成、订单内容、结算、追销、犯罪记录）完全由原版 <c>HandleNormalClient</c> 负责。
    /// </summary>
    [HarmonyPatch(typeof(ShowcaseHelper), nameof(ShowcaseHelper.GetDisplayCasePrediction))]
    internal static class CipherShowcasePatch
    {
        private static readonly string[] VanillaPermitIds =
        {
            "permit_gun_1",
            "permit_gun_2",
            "permit_gun_3"
        };

        private static readonly string[] GunBuyerClientIds =
        {
            "ll_gun_buyer",
            "merc_gun_buyer",
            "ul_gun_buyer"
        };

        private static void Postfix(ref List<(string, int, string)> __result)
        {
            if (__result == null)
            {
                return;
            }

            // 原版条目是按 permit_gun_N 逐条添加的；先全部撤掉，再按"每级一条"的规则重建，
            // 这样原版证与暗号同级同柜时不会重复掷骰。
            __result.RemoveAll(entry => IsGunBuyer(entry.Item1));

            var rebuilt = new List<(string, int, string)>(GunBuyerClientIds.Length);
            for (int i = 0; i < GunBuyerClientIds.Length; i++)
            {
                string vanillaId = VanillaPermitIds[i];
                string cipherId = UndergroundCipher.IdFor(i + 1);

                bool hasVanilla = ShowcaseHelper.IsShowcaseContainItemById(vanillaId);
                bool hasCipher = ShowcaseHelper.IsShowcaseContainItemById(cipherId);
                if (!hasVanilla && !hasCipher)
                {
                    continue;
                }

                // 吸引物优先显示原版证；只有暗号时显示暗号，顾客界面会写"由 地下枪匠暗号 N 吸引"。
                string attractedItemId = hasVanilla ? vanillaId : cipherId;
                rebuilt.Add((GunBuyerClientIds[i], ShowcaseHelper.GetGunBuyerChance(i), attractedItemId));
            }

            // 插到列表最前面，保持原版"枪械买家优先占用当日普通客名额"的顺序。
            if (rebuilt.Count > 0)
            {
                __result.InsertRange(0, rebuilt);
            }
        }

        private static bool IsGunBuyer(string clientId)
        {
            for (int i = 0; i < GunBuyerClientIds.Length; i++)
            {
                if (GunBuyerClientIds[i] == clientId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
