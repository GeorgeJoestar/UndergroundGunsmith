namespace UndergroundGunsmith
{
    /// <summary>
    /// 模组入口。只做两件事：注册三张"地下枪匠暗号"物品，
    /// 展示柜触发逻辑由 <see cref="CipherShowcasePatch"/> 负责（加载器自动应用 Harmony 补丁）。
    /// </summary>
    public class ModEntry : IMod
    {
        private ModLog log;

        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            ModHook.OnModItemDirectoryInit += UndergroundCipher.Register;
            CipherNetworkUpgrades.Register(log);
            RetiredGunsmithSourcePatch.SetLog(log);
            log.Log("Underground gunsmith ciphers registered (" + UndergroundCipher.IdFor(1) + ".." + UndergroundCipher.IdFor(UndergroundCipher.MaxLevel) + ").");
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }
    }
}
