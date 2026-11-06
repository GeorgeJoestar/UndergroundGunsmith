using System;
using HarmonyLib;

namespace UndergroundGunsmith
{
    /// <summary>
    /// 判定"当前是否正在生成退休枪匠的存货"。
    /// 原版存货 lambda 在对话结束时调用 <c>PermitHelper.IsGunPermitII/III()</c> 计算 permitSteps，
    /// 我们只在这一个上下文里让展示柜中的暗号起等效作用，其它调用点（吊销、治安部商人等）保持原样。
    /// </summary>
    internal static class GunsmithStockContext
    {
        private static bool generatingClients;

        public static void BeginGeneration()
        {
            generatingClients = true;
        }

        public static void EndGeneration()
        {
            generatingClients = false;
        }

        public static bool InRetiredGunsmithStock
        {
            get
            {
                if (generatingClients)
                {
                    return false;
                }
                StoreClientInstance instance = PlayerStore.Instance == null ? null : PlayerStore.Instance.currentClientInstance;
                StoreClient client = instance == null ? null : instance.GetClientBlueprint();
                return client != null && client.identifier == "retired_gunsmith";
            }
        }

        public static bool CipherInShowcase(int level)
        {
            if (EmporiumEntry.Instance == null || EmporiumEntry.Instance.showcaseElement == null)
            {
                return false;
            }
            return ShowcaseHelper.IsShowcaseContainItemById(UndergroundCipher.IdFor(level));
        }
    }

    /// <summary>生成顾客期间禁用暗号等效，保证吊销判定与其它商人的存货完全按原版走。</summary>
    [HarmonyPatch(typeof(StoreClientManager), nameof(StoreClientManager.GenerateClient))]
    internal static class GenerateClientGenerationGuard
    {
        private static void Prefix()
        {
            GunsmithStockContext.BeginGeneration();
        }

        private static void Postfix()
        {
            GunsmithStockContext.EndGeneration();
        }

        private static Exception Finalizer(Exception __exception)
        {
            GunsmithStockContext.EndGeneration();
            return __exception;
        }
    }

    /// <summary>等级平替：展示柜里有暗号 II 时，等效"已登记原版 II 级证"（同级不叠加）。</summary>
    [HarmonyPatch(typeof(PermitHelper), nameof(PermitHelper.IsGunPermitII))]
    internal static class GunPermitIICipherPatch
    {
        private static void Postfix(ref bool __result)
        {
            if (__result || !GunsmithStockContext.InRetiredGunsmithStock)
            {
                return;
            }
            if (GunsmithStockContext.CipherInShowcase(2))
            {
                __result = true;
            }
        }
    }

    /// <summary>等级平替：展示柜里有暗号 III 时，等效"已登记原版 III 级证"（同级不叠加）。</summary>
    [HarmonyPatch(typeof(PermitHelper), nameof(PermitHelper.IsGunPermitIII))]
    internal static class GunPermitIIICipherPatch
    {
        private static void Postfix(ref bool __result)
        {
            if (__result || !GunsmithStockContext.InRetiredGunsmithStock)
            {
                return;
            }
            if (GunsmithStockContext.CipherInShowcase(3))
            {
                __result = true;
            }
        }
    }

    /// <summary>
    /// 只有暗号真的补上了一步（该级原版证没登记）时才给商人打来源标记，
    /// 否则保持完全原版外观。
    /// </summary>
    [HarmonyPatch(typeof(StoreClientListSpec), nameof(StoreClientListSpec.CreateRetiredGunsmith))]
    internal static class RetiredGunsmithSourcePatch
    {
        private static ModLog log;

        public static void SetLog(ModLog modLog)
        {
            log = modLog;
        }

        private static void Postfix(StoreClient __result)
        {
            if (__result == null)
            {
                return;
            }

            bool addedStep = (!PermitHelper.IsGunPermitII() && GunsmithStockContext.CipherInShowcase(2))
                          || (!PermitHelper.IsGunPermitIII() && GunsmithStockContext.CipherInShowcase(3));
            if (addedStep)
            {
                ModHelper.SetClientSource(__result, UndergroundCipher.ModId);
                if (log != null)
                {
                    log.Log("Retired gunsmith stock counts showcase ciphers (II:"
                        + GunsmithStockContext.CipherInShowcase(2) + ", III:"
                        + GunsmithStockContext.CipherInShowcase(3) + ").");
                }
            }
        }
    }
}
