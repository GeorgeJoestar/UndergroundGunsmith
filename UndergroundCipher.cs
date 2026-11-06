namespace UndergroundGunsmith
{
    /// <summary>
    /// 三张"地下枪匠暗号"。等级 1/2/3 与展示柜里的原版武器许可证 I/II/III 一一对应，
    /// 但它只是触发条件：不写 <c>PlayerStore.ActivePermits</c>，因此不会把违禁枪转为管制品，
    /// 也不会进入治安部的吊销流程。
    /// </summary>
    internal static class UndergroundCipher
    {
        public const string ModId = "underground.gunsmith";

        public const int MaxLevel = 3;

        // 原版手写便签贴图（32x32 像素 → 2x2 格），不需要额外 Textures 资源。
        private const string SpriteAtlas = "Items/items_misc";
        private const string SpriteName = "handnote";

        public static string IdFor(int level)
        {
            return "underground_licence_" + level;
        }

        public static string UpgradeIdFor(int level)
        {
            return ModId + ".cipher_" + level;
        }

        public static bool IsCipherUpgradeId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            for (int level = 1; level <= MaxLevel; level++)
            {
                if (id == UpgradeIdFor(level))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsCipher(string identifier)
        {
            for (int level = 1; level <= MaxLevel; level++)
            {
                if (IdFor(level) == identifier)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Register(ModItemDirectory directory)
        {
            for (int level = 1; level <= MaxLevel; level++)
            {
                int captured = level;
                directory.Add(IdFor(level), () => Create(captured));
            }
        }

        private static GameItem Create(int level)
        {
            GameItem item = ItemDirectory.CreateEmptyItem(IdFor(level));
            item.SetSpriteAndShape(SpriteAtlas, SpriteName);
            item.name = ModHelper.GetLocalized(ModId, "item_underground_licence_" + level + "_name");
            item.shortDescription = ModHelper.GetLocalized(ModId, "item_underground_licence_" + level + "_desc");
            item.SetGameItemType("DOCUMENT");
            item.EnableTag("IMPORTANT_TAG");
            item.unitValue = 0L;
            return item;
        }
    }
}
