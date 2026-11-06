using System;
using System.Collections.Generic;

namespace UndergroundGunsmith
{
    /// <summary>
    /// 「Wild 的人脉」里的三级暗号升级（卡片 Gunsmith，链式前置，300/600/900）。
    /// 购买只排队；次日开门时把对应暗号摆上前台，已持有则跳过。
    /// 不写 PlayerStore.ActivePermits，因此不触发吊销，也不把违禁枪转为管制品。
    ///
    /// 注意：<c>ModHelper.AddNetworkUpgrade</c> 内部会触发 <c>PlayerStore.Instance</c> 的懒构造，
    /// 而主菜单阶段构造 PlayerStore 会抛 NullReferenceException。
    /// 所以这里只在"已进入存档/新局"的钩子里注册，并逐级 try/catch 兜底。
    /// </summary>
    internal static class CipherNetworkUpgrades
    {
        private const int BaseCost = 300;

        private const string PendingKeyPrefix = "pending_cipher_";

        private static readonly HashSet<int> registeredLevels = new HashSet<int>();

        private static bool hooksBound;
        private static ModLog log;

        public static void Register(ModLog modLog)
        {
            if (hooksBound)
            {
                return;
            }
            hooksBound = true;
            log = modLog;

            ModHook.OnGameLoadedInit += EnsureUpgrades;
            ModHook.OnGameLoadedLate += EnsureUpgrades;
            ModHook.OnNewGameStarted += OnNewGameStarted;
            ModHook.OnShutterOpenedEarly += OnShutterOpened;
        }

        private static void OnNewGameStarted(string scenarioId)
        {
            EnsureUpgrades();
        }

        private static void OnShutterOpened()
        {
            EnsureUpgrades();
            IssuePending();
        }

        /// <summary>幂等：只在升级确实注册成功后才会跳过下一次。</summary>
        private static void EnsureUpgrades()
        {
            for (int level = 1; level <= UndergroundCipher.MaxLevel; level++)
            {
                if (registeredLevels.Contains(level))
                {
                    continue;
                }

                int captured = level;
                string titleKey = "network_cipher_" + captured + "_title";
                string descKey = "network_cipher_" + captured + "_desc";
                string[] prerequisites = captured == 1
                    ? null
                    : new[] { UndergroundCipher.UpgradeIdFor(captured - 1) };

                try
                {
                    ModHelper.AddNetworkUpgrade(
                        UndergroundCipher.ModId,
                        UndergroundCipher.UpgradeIdFor(captured),
                        BaseCost * captured,
                        "Gunsmith",
                        () => ModHelper.GetLocalized(UndergroundCipher.ModId, titleKey),
                        () => ModHelper.GetLocalized(UndergroundCipher.ModId, descKey),
                        () => QueueIssue(captured),
                        prerequisites: prerequisites);

                    registeredLevels.Add(captured);
                }
                catch (Exception ex)
                {
                    // 主菜单等 PlayerStore 尚未就绪的阶段会走到这里：下一局开始/读档时再试
                    if (log != null)
                    {
                        log.Warning("Network upgrade registration deferred (level " + captured + "): " + ex.Message);
                    }
                }
            }
        }

        private static void QueueIssue(int level)
        {
            ModHelper.SetModDataBool(UndergroundCipher.ModId, PendingKeyPrefix + level, true);
        }

        private static void IssuePending()
        {
            if (EmporiumEntry.Instance == null || PlayerStore.Instance == null)
            {
                return;
            }

            for (int level = 1; level <= UndergroundCipher.MaxLevel; level++)
            {
                string key = PendingKeyPrefix + level;
                if (!ModHelper.GetModDataBool(UndergroundCipher.ModId, key))
                {
                    continue;
                }

                ModHelper.SetModDataBool(UndergroundCipher.ModId, key, false);

                string id = UndergroundCipher.IdFor(level);
                if (EmporiumEntry.Instance.GetItemCountById(id) > 0)
                {
                    continue;
                }

                if (!PlayerStore.Instance.TryAddDirectSellingItemToTable(id, isOwend: true))
                {
                    // 前台塞不下时退回背包，避免像原版纸证那样被吞掉
                    ModHelper.MoveItemToInventory(ModHelper.SetItemOwnerShip(ModHelper.SpawnItem(id), true));
                }
                if (log != null)
                {
                    log.Log("Issued cipher level " + level + " (" + id + ") on shutter open.");
                }
            }
        }
    }
}
